using LearningManagementSystem.Controllers.Account;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.RoleEnums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        
       
        public IActionResult Dashboard()
        {
            var pendingCount = lMSDbContext.StudentCourses
      .Where(e => e.IsApproved == false)
      .Count();

          
            ViewBag.PendingCount = pendingCount;
            var user = userManager.GetUserAsync(User).Result;
            ViewBag.Name = user.Name;
            return View();
        }
        [Route("Privacy")]
        
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
