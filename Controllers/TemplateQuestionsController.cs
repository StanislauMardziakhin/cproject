using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.ViewModels;

namespace CourseProject.Controllers;

[Authorize]
public class TemplateQuestionsController : Controller
{
    private readonly QuestionService _questionService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TemplateQuestionsController(QuestionService questionService, IStringLocalizer<SharedResources> localizer)
    {
        _questionService = questionService;
        _localizer = localizer;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("TemplateQuestions/AddQuestion")]
    public async Task<IActionResult> AddQuestion(int templateId, Question question)
    {
        if (!ModelState.IsValid) return Json(new { succeeded = false, error = "InvalidModelState" });
        var (succeeded, error) = await _questionService.AddQuestionAsync(templateId, question, GetUserId(), IsAdmin());
        if (!succeeded) return Json(new { succeeded = false, error });
        return Json(new { succeeded = true, error = string.Empty, question = ToQuestionDto(question) });
    }

    [HttpGet]
    [Route("TemplateQuestions/EditQuestion")]
    public async Task<IActionResult> EditQuestion(int id)
    {
        var question = await _questionService.GetQuestionAsync(id, GetUserId(), IsAdmin());
        if (question == null) return NotFound();
        var viewModel = ToQuestionViewModel(question);
        
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        
        if (isAjax)
        {
            return PartialView("_EditQuestion", viewModel);
        }
        else
        {
            return RedirectToAction("Edit", "Templates", new { id = question.TemplateId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("TemplateQuestions/EditQuestion")]
    public async Task<IActionResult> EditQuestion(int id, QuestionViewModel model)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        
        if (!ModelState.IsValid) 
        {
            return isAjax 
                ? Json(new { succeeded = false, error = "Validation failed" })
                : PartialView("_EditQuestion", model);
        }
        
        var question = await _questionService.GetQuestionAsync(id, GetUserId(), IsAdmin());
        if (question == null) return NotFound();
        question.Title = model.Title;
        question.Description = model.Description;
        question.Type = Enum.Parse<QuestionType>(model.Type);
        question.Order = model.Order;
        question.IsVisibleInResults = model.IsVisibleInResults;
        var (succeeded, error) = await _questionService.UpdateQuestionAsync(id, question, GetUserId(), IsAdmin());
        if (!succeeded)
        {
            ModelState.AddModelError("", _localizer[error].Value);
            return isAjax 
                ? Json(new { succeeded = false, error = _localizer[error].Value })
                : PartialView("_EditQuestion", model);
        }
        
        return isAjax 
            ? Json(new { succeeded = true, message = _localizer["SuccessQuestionUpdated"].Value })
            : RedirectToAction("Edit", "Templates", new { id = question.TemplateId });
    }

    [HttpGet]
    [Route("TemplateQuestions/DeleteQuestion")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _questionService.GetQuestionAsync(id, GetUserId(), IsAdmin());
        if (question == null)
        {
            TempData["Error"] = _localizer["QuestionNotFoundOrAccessDenied"].Value;
            return RedirectToAction("Index", "Templates");
        }
        var (succeeded, error) = await _questionService.DeleteQuestionAsync(id, GetUserId(), IsAdmin());
        TempData[succeeded ? "Success" : "Error"] = _localizer[succeeded ? "SuccessQuestionDeleted" : error].Value;
        return RedirectToAction("Edit", "Templates", new { id = question.TemplateId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("TemplateQuestions/UpdateQuestionOrder")]
    public async Task<IActionResult> UpdateQuestionOrder(List<QuestionOrder> orders)
    {
        var (succeeded, error) = await _questionService.UpdateQuestionOrderAsync(orders, GetUserId(), IsAdmin());
        return Json(new { succeeded, error });
    }

    private QuestionDto ToQuestionDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            TemplateId = question.TemplateId,
            Title = question.Title,
            Description = question.Description,
            Type = question.Type,
            Order = question.Order,
            IsVisibleInResults = question.IsVisibleInResults
        };
    }

    private QuestionViewModel ToQuestionViewModel(Question question)
    {
        return new QuestionViewModel
        {
            Id = question.Id,
            TemplateId = question.TemplateId,
            Title = question.Title,
            Description = question.Description,
            Type = question.Type.ToString(),
            Order = question.Order,
            IsVisibleInResults = question.IsVisibleInResults
        };
    }
} 