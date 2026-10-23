using JobBoard.Data;
using JobBoard.Models;
using JobBoard.Services;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Data.Repositories;

/// <summary>Contains the job-board database queries and persistence operations.</summary>
public sealed class JobBoardRepository : IJobBoardRepository
{
    private const int VacancyPageSize = 20;
    private readonly AppDbContext _db;

    public JobBoardRepository(AppDbContext db) => _db = db;

    public async Task<VacancyListViewModel> GetVacanciesAsync(string? queryText, int? categoryId, string sort, int page)
    {
        var query = _db.Vacancies.Include(v => v.Category).Include(v => v.Employer).Where(v => v.IsPublished);
        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var term = queryText.Trim().ToLower();
            query = query.Where(v => v.Title.ToLower().Contains(term));
        }
        if (categoryId.HasValue) query = query.Where(v => v.CategoryId == categoryId.Value);
        query = sort switch
        {
            "salary_desc" => query.OrderByDescending(v => v.Salary.HasValue).ThenByDescending(v => v.Salary).ThenByDescending(v => v.UpdatedAt),
            "salary_asc" => query.OrderBy(v => v.Salary).ThenByDescending(v => v.UpdatedAt),
            _ => query.OrderByDescending(v => v.UpdatedAt)
        };
        var totalPages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)VacancyPageSize));
        page = Math.Clamp(page, 1, totalPages);
        return new VacancyListViewModel
        {
            Items = await query.Skip((page - 1) * VacancyPageSize).Take(VacancyPageSize).ToListAsync(),
            Categories = await GetCategoriesAsync(), Query = queryText, CategoryId = categoryId,
            Sort = sort, Page = page, TotalPages = totalPages
        };
    }

    public Task<Vacancy?> GetVacancyAsync(int id) => _db.Vacancies.Include(v => v.Category).Include(v => v.Employer).FirstOrDefaultAsync(v => v.Id == id);
    public Task<List<Resume>> GetSeekerResumesAsync(string seekerId) => _db.Resumes.Where(r => r.SeekerId == seekerId).OrderByDescending(r => r.UpdatedAt).ToListAsync();
    public Task<int?> GetApplicationIdAsync(int vacancyId, string seekerId) => _db.JobApplications.Where(a => a.VacancyId == vacancyId && a.SeekerId == seekerId).Select(a => (int?)a.Id).FirstOrDefaultAsync();
    public Task<List<Vacancy>> GetEmployerVacanciesAsync(string employerId) => _db.Vacancies.Include(v => v.Category).Where(v => v.EmployerId == employerId).OrderByDescending(v => v.UpdatedAt).ToListAsync();
    public Task<List<Vacancy>> GetPublishedEmployerVacanciesAsync(string employerId) => _db.Vacancies.Where(v => v.EmployerId == employerId && v.IsPublished).OrderByDescending(v => v.UpdatedAt).ToListAsync();
    public Task<bool> CategoryExistsAsync(int? categoryId) => categoryId.HasValue ? _db.Categories.AnyAsync(c => c.Id == categoryId.Value) : Task.FromResult(true);
    public Task<List<Category>> GetCategoriesAsync() => _db.Categories.OrderBy(c => c.Name).ToListAsync();
    public Task<List<SelectListItem>> GetCategoryOptionsAsync() => _db.Categories.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
    public Task<Vacancy?> GetOwnedVacancyAsync(int id, string employerId) => _db.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.EmployerId == employerId);
    public async Task AddVacancyAsync(Vacancy vacancy) { _db.Vacancies.Add(vacancy); await _db.SaveChangesAsync(); }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    public Task<List<Resume>> GetSeekerResumesWithCategoryAsync(string seekerId) => _db.Resumes.Include(r => r.Category).Where(r => r.SeekerId == seekerId).OrderByDescending(r => r.UpdatedAt).ToListAsync();
    public Task<List<JobApplication>> GetSeekerApplicationsAsync(string seekerId) => _db.JobApplications.Include(a => a.Vacancy).ThenInclude(v => v!.Employer).Include(a => a.Resume).Include(a => a.Messages).Where(a => a.SeekerId == seekerId).OrderByDescending(a => a.CreatedAt).ToListAsync();
    public Task<Resume?> GetOwnedResumeForEditAsync(int id, string seekerId) => _db.Resumes.Include(r => r.WorkExperiences).Include(r => r.Educations).FirstOrDefaultAsync(r => r.Id == id && r.SeekerId == seekerId);
    public Task<Resume?> GetResumeAsync(int id) => _db.Resumes.Include(r => r.Category).Include(r => r.Seeker).Include(r => r.WorkExperiences).Include(r => r.Educations).FirstOrDefaultAsync(r => r.Id == id);
    public Task<Resume?> GetOwnedResumeAsync(int id, string seekerId) => _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.SeekerId == seekerId);
    public async Task<(List<Resume> Items, int Page, int TotalPages)> GetPublishedResumeCatalogAsync(int? categoryId, int page, int pageSize)
    {
        var query = _db.Resumes.Include(r => r.Category).Include(r => r.Seeker).Where(r => r.IsPublished);
        if (categoryId.HasValue) query = query.Where(r => r.CategoryId == categoryId.Value);
        var totalPages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = await query.OrderByDescending(r => r.UpdatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, page, totalPages);
    }
    public Task<List<Resume>> GetPublishedResumesForSeekerAsync(string seekerId) => _db.Resumes.Include(r => r.Category).Where(r => r.SeekerId == seekerId && r.IsPublished).OrderByDescending(r => r.UpdatedAt).ToListAsync();
    public async Task AddResumeAsync(Resume resume) { _db.Resumes.Add(resume); await _db.SaveChangesAsync(); }

    public async Task<EmployerApplicationsViewModel> GetEmployerApplicationsAsync(string employerId, int? vacancyId)
    {
        var vacancies = await _db.Vacancies.Where(v => v.EmployerId == employerId).OrderByDescending(v => v.UpdatedAt).ToListAsync();
        var query = _db.JobApplications.Include(a => a.Vacancy).Include(a => a.Resume).Include(a => a.Seeker).Include(a => a.Messages).Where(a => a.Vacancy!.EmployerId == employerId);
        if (vacancyId.HasValue) query = query.Where(a => a.VacancyId == vacancyId.Value);
        return new EmployerApplicationsViewModel { Applications = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(), Vacancies = vacancies, VacancyId = vacancyId };
    }
    public Task<List<JobApplication>> GetDialogsAsync(string userId) => _db.JobApplications.Include(a => a.Vacancy).ThenInclude(v => v!.Employer).Include(a => a.Seeker).Include(a => a.Resume).Include(a => a.Messages).Where(a => a.SeekerId == userId || a.Vacancy!.EmployerId == userId).OrderByDescending(a => a.Messages.Max(m => (DateTime?)m.SentAt) ?? a.CreatedAt).ToListAsync();
    public Task<Vacancy?> GetPublishedVacancyAsync(int vacancyId) => _db.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancyId && v.IsPublished);
    public Task<JobApplication?> GetApplicationAsync(int vacancyId, string seekerId) => _db.JobApplications.FirstOrDefaultAsync(a => a.VacancyId == vacancyId && a.SeekerId == seekerId);
    public Task<Resume?> GetOwnedResumeAsync(int resumeId, string seekerId, bool requirePublished) => _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && (requirePublished ? r.IsPublished : r.SeekerId == seekerId) && (requirePublished || r.SeekerId == seekerId));
    public async Task<JobApplication> CreateApplicationAsync(int vacancyId, int resumeId, string seekerId)
    {
        var application = new JobApplication { VacancyId = vacancyId, ResumeId = resumeId, SeekerId = seekerId, CreatedAt = DateTime.UtcNow };
        _db.JobApplications.Add(application); await _db.SaveChangesAsync(); return application;
    }
    public async Task<JobApplication?> GetParticipantApplicationAsync(int id, string userId)
    {
        var application = await _db.JobApplications.Include(a => a.Vacancy).ThenInclude(v => v!.Employer).Include(a => a.Resume).Include(a => a.Seeker).FirstOrDefaultAsync(a => a.Id == id);
        return application != null && (application.SeekerId == userId || application.Vacancy!.EmployerId == userId) ? application : null;
    }
    public Task<List<ChatMessage>> GetMessagesAsync(int applicationId) => _db.ChatMessages.Include(m => m.Sender).Where(m => m.JobApplicationId == applicationId).OrderBy(m => m.SentAt).ThenBy(m => m.Id).ToListAsync();
    public async Task AddMessageAsync(ChatMessage message) { _db.ChatMessages.Add(message); await _db.SaveChangesAsync(); }
}

