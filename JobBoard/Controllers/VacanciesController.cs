using JobBoard.Models;
using JobBoard.Services;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Controllers;

[Authorize]
public class VacanciesController : Controller
{
    private readonly IJobBoardService _jobBoard;
    private readonly UserManager<ApplicationUser> _users;

    public VacanciesController(IJobBoardService jobBoard, UserManager<ApplicationUser> users)
    {
        _jobBoard = jobBoard;
        _users = users;
    }

    public async Task<IActionResult> Index(string? q, int? categoryId, string sort = "new", int page = 1) => View(await BuildListModel(q, categoryId, sort, page));

    [HttpGet]
    public async Task<IActionResult> Search(string? q, int? categoryId, string sort = "new", int page = 1) => PartialView("_VacancyResults", await BuildListModel(q, categoryId, sort, page));

    private Task<VacancyListViewModel> BuildListModel(string? q, int? categoryId, string sort, int page) => _jobBoard.GetVacanciesAsync(q, categoryId, sort, page);

    public async Task<IActionResult> Details(int id)
    {
        var vacancy = await _jobBoard.GetVacancyAsync(id);
        var userId = _users.GetUserId(User);
        if (vacancy == null || (!vacancy.IsPublished && vacancy.EmployerId != userId)) return NotFound();
        var model = new VacancyDetailsViewModel { Vacancy = vacancy };
        ViewBag.IsOwner = vacancy.EmployerId == userId;
        if (User.IsInRole(Roles.Seeker))
        {
            model.MyResumes = await _jobBoard.GetSeekerResumesAsync(userId!);
            model.ExistingApplicationId = await _jobBoard.GetApplicationIdAsync(id, userId!);
        }
        return View(model);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new VacancyFormViewModel();
        await FillCategories(model);
        return View("Form", model);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VacancyFormViewModel model)
    {
        model.Id = null;
        await ValidateCategory(model);
        ValidateExperience(model);
        if (!ModelState.IsValid) { await FillCategories(model); return View("Form", model); }
        var vacancy = new Vacancy { EmployerId = _users.GetUserId(User)!, CreatedAt = DateTime.UtcNow };
        Map(model, vacancy);
        await _jobBoard.AddVacancyAsync(vacancy);
        TempData["Success"] = "???????? ???????";
        return RedirectToAction("Index", "Employer");
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null) return NotFound();
        var model = new VacancyFormViewModel { Id = vacancy.Id, Title = vacancy.Title, Salary = vacancy.Salary, Description = vacancy.Description, Requirements = vacancy.Requirements, ExperienceFrom = vacancy.ExperienceFrom, ExperienceTo = vacancy.ExperienceTo, CategoryId = vacancy.CategoryId };
        await FillCategories(model);
        return View("Form", model);
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VacancyFormViewModel model)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null) return NotFound();
        model.Id = id;
        await ValidateCategory(model);
        ValidateExperience(model);
        if (!ModelState.IsValid) { await FillCategories(model); return View("Form", model); }
        Map(model, vacancy);
        await _jobBoard.SaveChangesAsync();
        TempData["Success"] = "???????? ?????????";
        return RedirectToAction("Index", "Employer");
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(int id)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null) return NotFound();
        vacancy.UpdatedAt = DateTime.UtcNow;
        await _jobBoard.SaveChangesAsync();
        return RedirectToAction("Index", "Employer");
    }

    [Authorize(Roles = Roles.Employer)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id, string? returnUrl)
    {
        var vacancy = await FindOwnVacancy(id);
        if (vacancy == null) return NotFound();
        vacancy.IsPublished = !vacancy.IsPublished;
        await _jobBoard.SaveChangesAsync();
        TempData["Success"] = vacancy.IsPublished ? "???????? ????????????" : "???????? ????? ? ??????????";
        return returnUrl == "details" ? RedirectToAction(nameof(Details), new { id }) : RedirectToAction("Index", "Employer");
    }

    private Task<Vacancy?> FindOwnVacancy(int id) => _jobBoard.GetOwnedVacancyAsync(id, _users.GetUserId(User)!);
    private async Task FillCategories(VacancyFormViewModel model) => model.Categories = await _jobBoard.GetCategoryOptionsAsync();
    private async Task ValidateCategory(VacancyFormViewModel model)
    {
        if (model.CategoryId.HasValue && !await _jobBoard.CategoryExistsAsync(model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "????????? ????????? ?? ??????????");
    }
    private static void Map(VacancyFormViewModel model, Vacancy vacancy)
    {
        vacancy.Title = model.Title.Trim(); vacancy.Salary = model.Salary; vacancy.Description = model.Description.Trim();
        vacancy.Requirements = string.IsNullOrWhiteSpace(model.Requirements) ? null : model.Requirements.Trim();
        vacancy.ExperienceFrom = model.ExperienceFrom; vacancy.ExperienceTo = model.ExperienceTo;
        vacancy.CategoryId = model.CategoryId!.Value; vacancy.UpdatedAt = DateTime.UtcNow;
    }
    private void ValidateExperience(VacancyFormViewModel model)
    {
        if (model.ExperienceFrom > model.ExperienceTo) ModelState.AddModelError(nameof(model.ExperienceFrom), "??????????? ???? ?? ????? ???? ?????? ?????????????");
    }
}
