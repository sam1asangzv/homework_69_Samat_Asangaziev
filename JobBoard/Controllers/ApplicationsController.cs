using JobBoard.Models;
using JobBoard.Services;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private const int MaxMessageLength = 4000;
    private readonly IJobBoardService _jobBoard;
    private readonly UserManager<ApplicationUser> _users;

    public ApplicationsController(IJobBoardService jobBoard, UserManager<ApplicationUser> users)
    {
        _jobBoard = jobBoard;
        _users = users;
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Employer(int? vacancyId) => View(await _jobBoard.GetEmployerApplicationsAsync(_users.GetUserId(User)!, vacancyId));

    [HttpGet]
    public async Task<IActionResult> Dialogs() => View(await _jobBoard.GetDialogsAsync(_users.GetUserId(User)!));

    [Authorize(Roles = Roles.Seeker)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int vacancyId, int resumeId)
    {
        var userId = _users.GetUserId(User)!;
        if (await _jobBoard.GetPublishedVacancyAsync(vacancyId) == null) return NotFound();
        var existing = await _jobBoard.GetApplicationAsync(vacancyId, userId);
        if (existing != null) return RedirectToAction(nameof(Chat), new { id = existing.Id });
        if (await _jobBoard.GetOwnedResumeAsync(resumeId, userId) == null)
        {
            TempData["Error"] = "Выберите резюме для отклика";
            return RedirectToAction("Details", "Vacancies", new { id = vacancyId });
        }
        var application = await _jobBoard.CreateApplicationAsync(vacancyId, resumeId, userId);
        TempData["Success"] = "Отклик отправлен";
        return RedirectToAction(nameof(Chat), new { id = application.Id });
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StartEmployerConversation(int vacancyId, int resumeId)
    {
        var employerId = _users.GetUserId(User)!;
        var vacancy = await _jobBoard.GetOwnedVacancyAsync(vacancyId, employerId);
        var resume = await _jobBoard.GetOwnedResumeAsync(resumeId, employerId, requirePublished: true);
        if (vacancy == null || resume == null) return NotFound();
        var application = await _jobBoard.GetApplicationAsync(vacancyId, resume.SeekerId);
        application ??= await _jobBoard.CreateApplicationAsync(vacancyId, resumeId, resume.SeekerId);
        return RedirectToAction(nameof(Chat), new { id = application.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Chat(int id)
    {
        var userId = _users.GetUserId(User)!;
        var application = await _jobBoard.GetParticipantApplicationAsync(id, userId);
        if (application == null) return NotFound();
        return View(BuildChatModel(application, userId, await _jobBoard.GetMessagesAsync(id)));
    }

    [HttpGet]
    public async Task<IActionResult> Messages(int id)
    {
        var userId = _users.GetUserId(User)!;
        var application = await _jobBoard.GetParticipantApplicationAsync(id, userId);
        if (application == null) return NotFound();
        return PartialView("_Messages", BuildChatModel(application, userId, await _jobBoard.GetMessagesAsync(id)));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int id, string? text)
    {
        var userId = _users.GetUserId(User)!;
        if (await _jobBoard.GetParticipantApplicationAsync(id, userId) == null) return NotFound();
        text = text?.Trim();
        if (string.IsNullOrEmpty(text)) { TempData["Error"] = "Сообщение не может быть пустым"; return RedirectToAction(nameof(Chat), new { id }); }
        if (text.Length > MaxMessageLength) { TempData["Error"] = $"Максимальная длина сообщения — {MaxMessageLength} символов"; return RedirectToAction(nameof(Chat), new { id }); }
        await _jobBoard.AddMessageAsync(new ChatMessage { JobApplicationId = id, SenderId = userId, Text = text, SentAt = DateTime.UtcNow });
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = true });
        return RedirectToAction(nameof(Chat), new { id });
    }

    private static ChatViewModel BuildChatModel(JobApplication application, string userId, List<ChatMessage> messages) => new()
    {
        Application = application, CurrentUserId = userId, IsSeeker = application.SeekerId == userId, Messages = messages
    };
}
