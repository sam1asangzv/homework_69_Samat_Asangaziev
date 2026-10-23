using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobBoard.Services;

/// <summary>Application operations that read and write persistent job-board data.</summary>
public interface IJobBoardService
{
    Task<VacancyListViewModel> GetVacanciesAsync(string? query, int? categoryId, string sort, int page);
    Task<Vacancy?> GetVacancyAsync(int id);
    Task<List<Resume>> GetSeekerResumesAsync(string seekerId);
    Task<int?> GetApplicationIdAsync(int vacancyId, string seekerId);
    Task<List<Vacancy>> GetEmployerVacanciesAsync(string employerId);
    Task<List<Vacancy>> GetPublishedEmployerVacanciesAsync(string employerId);
    Task<bool> CategoryExistsAsync(int? categoryId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<SelectListItem>> GetCategoryOptionsAsync();
    Task<Vacancy?> GetOwnedVacancyAsync(int id, string employerId);
    Task AddVacancyAsync(Vacancy vacancy);
    Task SaveChangesAsync();

    Task<List<Resume>> GetSeekerResumesWithCategoryAsync(string seekerId);
    Task<List<JobApplication>> GetSeekerApplicationsAsync(string seekerId);
    Task<Resume?> GetOwnedResumeForEditAsync(int id, string seekerId);
    Task<Resume?> GetResumeAsync(int id);
    Task<Resume?> GetOwnedResumeAsync(int id, string seekerId);
    Task<(List<Resume> Items, int Page, int TotalPages)> GetPublishedResumeCatalogAsync(int? categoryId, int page, int pageSize);
    Task<List<Resume>> GetPublishedResumesForSeekerAsync(string seekerId);
    Task AddResumeAsync(Resume resume);

    Task<EmployerApplicationsViewModel> GetEmployerApplicationsAsync(string employerId, int? vacancyId);
    Task<List<JobApplication>> GetDialogsAsync(string userId);
    Task<Vacancy?> GetPublishedVacancyAsync(int vacancyId);
    Task<JobApplication?> GetApplicationAsync(int vacancyId, string seekerId);
    Task<Resume?> GetOwnedResumeAsync(int resumeId, string seekerId, bool requirePublished);
    Task<JobApplication> CreateApplicationAsync(int vacancyId, int resumeId, string seekerId);
    Task<JobApplication?> GetParticipantApplicationAsync(int id, string userId);
    Task<List<ChatMessage>> GetMessagesAsync(int applicationId);
    Task AddMessageAsync(ChatMessage message);
}
