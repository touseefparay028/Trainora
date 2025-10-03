using LearningManagementSystem.Controllers.Account;
using LearningManagementSystem.RoleEnums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        [Route("Dashboard")]
        
       
        public IActionResult Dashboard()
        {
            return View();
        }
        [Route("Privacy")]
        
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
