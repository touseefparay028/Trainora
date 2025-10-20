using LearningManagementSystem.Controllers.Account;
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

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        [Route("Dashboard")]
        
       
        public IActionResult Dashboard()
        {
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
