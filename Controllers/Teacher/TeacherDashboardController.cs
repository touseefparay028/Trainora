using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers.Teacher
{
    
    [Authorize(Roles = "Teacher")]
    public class TeacherDashboardController : Controller
    {
        private readonly LMSDbContext lMSDbContext;

        public TeacherDashboardController(LMSDbContext lMSDbContext)
        {
            this.lMSDbContext = lMSDbContext;
        }
        [Route("TeacherDashboard")]
       
        public async Task<IActionResult> TeacherDashboard()
        {
            var batches = await lMSDbContext.BatchDMs.ToListAsync();

            var model = new VideoConference
            {
                BatchList = batches
            };

            return View(model);
        }

        
    }
}
