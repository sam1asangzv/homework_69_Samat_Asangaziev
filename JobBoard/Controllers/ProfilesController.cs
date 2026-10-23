using JobBoard.Models;
using JobBoard.Services;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Controllers;

[Authorize(Roles = Roles.Employer)]
public class ProfilesController : Controller
{
    private readonly IJobBoardService _jobBoard;
    private readonly UserManager<ApplicationUser> _users;

    public ProfilesController(IJobBoardService jobBoard, UserManager<ApplicationUser> users)
    {
        _jobBoard = jobBoard;
        _users = users;
    }

    public async Task<IActionResult> Seeker(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null || !await _users.IsInRoleAsync(user, Roles.Seeker))
        {
            return NotFound();
        }

        var model = new SeekerProfileViewModel
        {
            User = user,
            Resumes = await _jobBoard.GetPublishedResumesForSeekerAsync(id)
        };
        return View(model);
    }
}
