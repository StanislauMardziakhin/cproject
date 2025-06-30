using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

[Authorize]
    public class FormsController : Controller
    {
        private readonly FormService _formService;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public FormsController(FormService formService, IStringLocalizer<SharedResources> localizer)
        {
            _formService = formService;
            _localizer = localizer;
        }
        
        [HttpGet]
        public async Task<IActionResult> Fill(int templateId)
        {
            var form = await _formService.GetFormForFillingAsync(templateId, GetUserId(), User.IsInRole("Admin"));
            if (form == null) return NotFound();
            return View(form);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fill(Form form, Dictionary<int, string> answers)
        {
            if (!ModelState.IsValid)
            {
                var reloadedForm = await _formService.GetFormForFillingAsync(form.TemplateId, GetUserId(), User.IsInRole("Admin"));
                if (reloadedForm == null) return NotFound();
                return View(reloadedForm);
            }

            var (success, errorMessage) = await _formService.SaveFormAsync(form, answers, GetUserId());
            if (!success)
            {
                ModelState.AddModelError(string.Empty, _localizer[errorMessage].Value);
                var reloadedForm = await _formService.GetFormForFillingAsync(form.TemplateId, GetUserId(), User.IsInRole("Admin"));
                if (reloadedForm == null) return NotFound();
                return View(reloadedForm);
            }

            TempData["Success"] = _localizer["SuccessFormSubmitted"].Value;
            return RedirectToAction("Index", "Templates");
        }
        
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var form = await _formService.GetFormForViewingAsync(id, GetUserId(), User.IsInRole("Admin"));
            if (form == null) return NotFound();
            return View(form);
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var forms = await _formService.GetFormsForUserAsync(userId);
            return View(forms);
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }