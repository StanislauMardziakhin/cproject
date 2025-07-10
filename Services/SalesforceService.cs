using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using CourseProject.Models;
using Microsoft.Extensions.Localization;

namespace CourseProject.Services;

public class SalesforceService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private DateTime _accessTokenExpiry;
    private string? _instanceUrl;
    private string? _lastAuthError;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public SalesforceService(IConfiguration configuration, IStringLocalizer<SharedResources> localizer)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _localizer = localizer;
    }

    private async Task<(bool Success, string? Error)> AuthenticateAsync()
    {
        var clientId = _configuration["Salesforce:ClientId"];
        var username = _configuration["Salesforce:Username"];
        var authUrl = _configuration["Salesforce:AuthEndpoint"] ?? "https://login.salesforce.com/services/oauth2/token";
        var privateKeyPem = _configuration["Salesforce:PrivateKey"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(privateKeyPem))
            throw new InvalidOperationException(_localizer["SalesforceInvalidConfig"]!);

        privateKeyPem = privateKeyPem.Replace("\\n", "\n");

        try
        {
            var now = DateTime.UtcNow;
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());
            var securityKey = new RsaSecurityKey(rsa);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateJwtSecurityToken(
                issuer: clientId,
                audience: "https://login.salesforce.com",
                subject: new ClaimsIdentity(new[] { new Claim("sub", username) }),
                notBefore: now,
                expires: now.AddMinutes(3),
                signingCredentials: creds
            );
            var jwt = handler.WriteToken(token);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", jwt)
            });

            var response = await _httpClient.PostAsync(authUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(_localizer["SalesforceAuthError"]!);

            using var doc = JsonDocument.Parse(json);
            _accessToken = doc.RootElement.GetProperty("access_token").GetString();
            _instanceUrl = doc.RootElement.GetProperty("instance_url").GetString();
            _accessTokenExpiry = DateTime.UtcNow.AddSeconds(3500);
            return (true, null);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(_localizer["SalesforceConnectionError"]!);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(_localizer["SalesforceResponseError"]!);
        }
    }

    private async Task<bool> EnsureAuthenticatedAsync()
    {
        if (_accessToken == null || DateTime.UtcNow > _accessTokenExpiry)
        {
            var (success, error) = await AuthenticateAsync();
            if (!success)
            {
                _lastAuthError = error;
            }
            return success;
        }
        return true;
    }

    public async Task<string> CreateAccountAsync(SalesforceAccountDto dto)
    {
        await EnsureAuthenticatedAsync();
        var url = BuildSObjectUrl("Account");
        var response = await PostToSalesforceAsync(url, dto);
        return ExtractIdFromResponse(response);
    }

    public async Task<string> CreateContactAsync(SalesforceContactDto dto)
    {
        await EnsureAuthenticatedAsync();
        var url = BuildSObjectUrl("Contact");
        var response = await PostToSalesforceAsync(url, dto);
        return ExtractIdFromResponse(response);
    }

    private string BuildSObjectUrl(string sobject)
    {
        var apiVersion = _configuration["Salesforce:ApiVersion"] ?? "v64.0";
        return $"{_instanceUrl}/services/data/{apiVersion}/sobjects/{sobject}";
    }

    private async Task<string> PostToSalesforceAsync(string url, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(_localizer["SalesforceGeneralError"]!);
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(_localizer["SalesforceConnectionError"]!);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(_localizer["SalesforceResponseError"]!);
        }
    }

    private string ExtractIdFromResponse(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);
        if (doc.RootElement.TryGetProperty("id", out var idProp))
            return idProp.GetString()!;
        throw new InvalidOperationException(_localizer["SalesforceGeneralError"]!);
    }
}