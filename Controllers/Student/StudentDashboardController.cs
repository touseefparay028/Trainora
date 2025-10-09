using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers.Student
{
    [Authorize(Roles ="Student")]
    public class StudentDashboardController : Controller
    {
        private readonly LMSDbContext lMSDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public StudentDashboardController(LMSDbContext lMSDbContext,UserManager<ApplicationUser> userManager)
        {
            this.lMSDbContext = lMSDbContext;
            this.userManager = userManager;
        }

        [Route("StudentDashboard")]
        public async Task<IActionResult> StudentDashboard()
        {
            //int CountOfAssignments;
            //var studentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); 
            //var pendingAssignments = await lMSDbContext.AssignmentDMs
            //    .Where(a => !lMSDbContext.StudentAssignmentDM
            //    .Any(sa => sa.assignmentDMId == a.Id && sa.StudentId == studentId))
            //    .CountAsync();
            //ViewBag.CountOfAssignments = pendingAssignments;
            var studentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userIdd = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userIdd);
            ViewBag.Name = user?.Name ?? "Unknown";
            // get student's batch id
            var studentBatchId = await lMSDbContext.Users
                .Where(s => s.Id == studentId)
                .Select(s => s.BatchDMId)
                .FirstOrDefaultAsync();

            var pendingAssignments = await lMSDbContext.AssignmentDMs
                .Where(a => a.BatchDMId == studentBatchId) // filter by batch
                .Where(a => !lMSDbContext.StudentAssignmentDM
                    .Any(sa => sa.assignmentDMId == a.Id && sa.StudentId == studentId))
                .CountAsync();

            ViewBag.CountOfAssignments = pendingAssignments;

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var batchId = lMSDbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.BatchDMId)
                .FirstOrDefault();

            int meetingCount = 0;

            if (batchId != null)
            {
                meetingCount = lMSDbContext.VideoConference
                    .Count(c => c.BatchId == batchId && (c.EndTime == null || c.EndTime >= DateTime.Now));
            }

            ViewBag.MeetingCount = meetingCount;
        

            return View();
             
        }
    }
}
