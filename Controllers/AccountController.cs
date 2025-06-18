using CourseProject.Services;
using CourseProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

public class AccountController : Controller
{
    private readonly AccountService _accountService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AccountController(AccountService accountService, IStringLocalizer<SharedResources> localizer)
    {
        _accountService = accountService;
        _localizer = localizer;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = "")
    {
        var model = new LoginViewModel { ReturnUrl = returnUrl };
        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;
        if (!ModelState.IsValid)
        {
            TempData["Error"] = _localizer["ErrorInvalidInput"].Value;
            return View(model);
        }
        var (succeeded, errors) = await _accountService.LoginAsync(model.Email, model.Password, model.RememberMe);
        if (succeeded)
        {
            TempData["Success"] = _localizer["SuccessLogin"].Value;
            return RedirectToLocal(model.ReturnUrl);
        }
        foreach (var error in errors) ModelState.AddModelError("", error);
        TempData["Error"] = _localizer["ErrorLogin"].Value;
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = _localizer["ErrorInvalidInput"].Value;
            return View();
        }

        var (succeeded, errors) = await _accountService.RegisterAsync(model.Email, model.Password, model.Name);
        if (succeeded)
        {
            TempData["Success"] = _localizer["SuccessRegister"].Value;
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in errors) ModelState.AddModelError(string.Empty, error);
        TempData["Error"] = _localizer["ErrorRegister"].Value;
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        TempData["Success"] = _localizer["SuccessLogout"].Value;
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}