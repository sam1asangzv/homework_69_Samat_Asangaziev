using Microsoft.AspNetCore.Http;

namespace JobBoard.Services;

public interface IFileService
{
    bool IsValidImage(IFormFile file);
    Task<string> SaveAvatarAsync(IFormFile file);
    void Delete(string? relativePath);
}

