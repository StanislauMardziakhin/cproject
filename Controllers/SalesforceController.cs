using CourseProject.Services;
using CourseProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.Models;

namespace CourseProject.Controllers;

[Authorize]
public class SalesforceController : Controller
{
    private readonly SalesforceService _salesforceService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public SalesforceController(SalesforceService salesforceService, IStringLocalizer<SharedResources> localizer)
    {
        _salesforceService = salesforceService;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult CreateAccountAndContact()
    {
        return View(new SalesforceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccountAndContact(SalesforceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("<br>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = errors;
            return RedirectToAction("CreateAccountAndContact");
        }

        try
        {
            var accountDto = new SalesforceAccountDto
            {
                Name = model.AccountName
            };
            var accountId = await _salesforceService.CreateAccountAsync(accountDto);

            var contactDto = new SalesforceContactDto
            {
                FirstName = model.ContactFirstName,
                LastName = model.ContactLastName,
                Email = model.ContactEmail,
                AccountId = accountId
            };
            await _salesforceService.CreateContactAsync(contactDto);

            TempData["Success"] = _localizer["AccountAndContactCreated"].Value;
            return RedirectToAction("CreateAccountAndContact");
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = _localizer["SalesforceIntegrationError"].Value;
            return RedirectToAction("CreateAccountAndContact");
        }
    }
} 