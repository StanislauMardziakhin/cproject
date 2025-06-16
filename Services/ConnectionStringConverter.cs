namespace CourseProject.Services;

public class ConnectionStringConverter
{
    private readonly string _databaseUrl;
    private readonly string _defaultConnection;

    public ConnectionStringConverter(string databaseUrl, string defaultConnection)
    {
        _databaseUrl = databaseUrl;
        _defaultConnection = defaultConnection;
    }

    public string GetConnectionString()
    {
        var connectionString = string.IsNullOrWhiteSpace(_databaseUrl) ? _defaultConnection : _databaseUrl;

        if (!string.IsNullOrWhiteSpace(_databaseUrl))
        {
            try
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                if (userInfo.Length != 2)
                {
                    throw new InvalidOperationException("Invalid user info format");
                }

                var efConnectionString =
                    $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
                Console.WriteLine($"Converted to EF format: {efConnectionString}");
                return efConnectionString;
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine($"Error parsing URI: {ex.Message}");
                throw new InvalidOperationException("Invalid DATABASE_URL format", ex);
            }
        }
        return connectionString;
    }
}