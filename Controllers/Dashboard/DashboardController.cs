using LearningManagementSystem.Controllers.Account;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.RoleEnums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly LMSDbContext lMSDbContext;

        public DashboardController(UserManager<ApplicationUser> userManager,LMSDbContext lMSDbContext)
        {
            this.userManager = userManager;
            this.lMSDbContext = lMSDbContext;
        }
        [Route("Dashboard")]

        [Authorize(AuthenticationSchemes ="AdminAuth",Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var pendingCount = lMSDbContext.StudentCourses
      .Where(e => e.IsApproved == false)
      .Count();
            var messages = await lMSDbContext.Contact.ToListAsync();

            // Manual mapping
            var contactVMs = messages.Select(m => new ContactVM
            {
                id = m.id,
                Name = m.Name,
                Email = m.Email,
                Message = m.Message
            }).ToList();

            ViewBag.PendingCount = pendingCount;
            var user = userManager.GetUserAsync(User).Result;
            ViewBag.Name = user.Name;
            return View(contactVMs);
        }
        [Route("Privacy")]
        
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
