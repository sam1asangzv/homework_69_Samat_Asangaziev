using JobBoard.Models;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobBoard.Services;

/// <summary>Business-logic facade used by the presentation layer.</summary>
public sealed class JobBoardService : IJobBoardService
{
    private readonly IJobBoardRepository _repository;

    public JobBoardService(IJobBoardRepository repository) => _repository = repository;

    public Task<VacancyListViewModel> GetVacanciesAsync(string? query, int? categoryId, string sort, int page) => _repository.GetVacanciesAsync(query, categoryId, sort, page);
    public Task<Vacancy?> GetVacancyAsync(int id) => _repository.GetVacancyAsync(id);
    public Task<List<Resume>> GetSeekerResumesAsync(string seekerId) => _repository.GetSeekerResumesAsync(seekerId);
    public Task<int?> GetApplicationIdAsync(int vacancyId, string seekerId) => _repository.GetApplicationIdAsync(vacancyId, seekerId);
    public Task<List<Vacancy>> GetEmployerVacanciesAsync(string employerId) => _repository.GetEmployerVacanciesAsync(employerId);
    public Task<List<Vacancy>> GetPublishedEmployerVacanciesAsync(string employerId) => _repository.GetPublishedEmployerVacanciesAsync(employerId);
    public Task<bool> CategoryExistsAsync(int? categoryId) => _repository.CategoryExistsAsync(categoryId);
    public Task<List<Category>> GetCategoriesAsync() => _repository.GetCategoriesAsync();
    public Task<List<SelectListItem>> GetCategoryOptionsAsync() => _repository.GetCategoryOptionsAsync();
    public Task<Vacancy?> GetOwnedVacancyAsync(int id, string employerId) => _repository.GetOwnedVacancyAsync(id, employerId);
    public Task AddVacancyAsync(Vacancy vacancy) => _repository.AddVacancyAsync(vacancy);
    public Task SaveChangesAsync() => _repository.SaveChangesAsync();
    public Task<List<Resume>> GetSeekerResumesWithCategoryAsync(string seekerId) => _repository.GetSeekerResumesWithCategoryAsync(seekerId);
    public Task<List<JobApplication>> GetSeekerApplicationsAsync(string seekerId) => _repository.GetSeekerApplicationsAsync(seekerId);
    public Task<Resume?> GetOwnedResumeForEditAsync(int id, string seekerId) => _repository.GetOwnedResumeForEditAsync(id, seekerId);
    public Task<Resume?> GetResumeAsync(int id) => _repository.GetResumeAsync(id);
    public Task<Resume?> GetOwnedResumeAsync(int id, string seekerId) => _repository.GetOwnedResumeAsync(id, seekerId);
    public Task<(List<Resume> Items, int Page, int TotalPages)> GetPublishedResumeCatalogAsync(int? categoryId, int page, int pageSize) => _repository.GetPublishedResumeCatalogAsync(categoryId, page, pageSize);
    public Task<List<Resume>> GetPublishedResumesForSeekerAsync(string seekerId) => _repository.GetPublishedResumesForSeekerAsync(seekerId);
    public Task AddResumeAsync(Resume resume) => _repository.AddResumeAsync(resume);
    public Task<EmployerApplicationsViewModel> GetEmployerApplicationsAsync(string employerId, int? vacancyId) => _repository.GetEmployerApplicationsAsync(employerId, vacancyId);
    public Task<List<JobApplication>> GetDialogsAsync(string userId) => _repository.GetDialogsAsync(userId);
    public Task<Vacancy?> GetPublishedVacancyAsync(int vacancyId) => _repository.GetPublishedVacancyAsync(vacancyId);
    public Task<JobApplication?> GetApplicationAsync(int vacancyId, string seekerId) => _repository.GetApplicationAsync(vacancyId, seekerId);
    public Task<Resume?> GetOwnedResumeAsync(int resumeId, string seekerId, bool requirePublished) => _repository.GetOwnedResumeAsync(resumeId, seekerId, requirePublished);
    public Task<JobApplication> CreateApplicationAsync(int vacancyId, int resumeId, string seekerId) => _repository.CreateApplicationAsync(vacancyId, resumeId, seekerId);
    public Task<JobApplication?> GetParticipantApplicationAsync(int id, string userId) => _repository.GetParticipantApplicationAsync(id, userId);
    public Task<List<ChatMessage>> GetMessagesAsync(int applicationId) => _repository.GetMessagesAsync(applicationId);
    public Task AddMessageAsync(ChatMessage message) => _repository.AddMessageAsync(message);
}



