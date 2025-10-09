using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers.Teacher
{
    
    [Authorize(Roles = "Teacher")]
    public class TeacherDashboardController : Controller
    {
        private readonly LMSDbContext lMSDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public TeacherDashboardController(LMSDbContext lMSDbContext,UserManager<ApplicationUser> userManager)
        {
            this.lMSDbContext = lMSDbContext;
            this.userManager = userManager;
        }
        [Route("TeacherDashboard")]
       
        public async Task<IActionResult> TeacherDashboard()
        {
            var batches = await lMSDbContext.BatchDMs.ToListAsync();

            var model = new VideoConference
            {
                BatchList = batches
            };
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            ViewBag.Name = user?.Name ?? "Unknown";

            return View(model);
        }

        
    }
}
