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


            ViewBag.batchid = user?.BatchDMId;
            ViewBag.Name = user?.Name ?? "Unknown";

            // get student's batch id
            var studentBatchId = await lMSDbContext.Users
                .Where(s => s.Id == studentId)
                .Select(s => s.BatchDMId)
                .FirstOrDefaultAsync();

            var announcements = await lMSDbContext.Announcements
        .OrderByDescending(a => a.CreatedAt)
        .Take(5)
        .ToListAsync();

            ViewBag.Announcements = announcements;


            ViewBag.enrolledCount = await lMSDbContext.StudentCourses
    .Where(e => e.StudentId == user.Id && e.IsApproved)
    .CountAsync();


            var pendingAssignments = await lMSDbContext.AssignmentDMs
                .Where(a => a.BatchDMId == studentBatchId) // filter by batch
                .Where(a => !lMSDbContext.StudentAssignmentDM
                    .Any(sa => sa.assignmentDMId == a.Id && sa.StudentId == studentId))
                .CountAsync();

            ViewBag.CountOfAssignments = pendingAssignments;
            var fewCourses = lMSDbContext.Courses
         .Include(c => c.Teacher) // make sure you have "using Microsoft.EntityFrameworkCore;"
         .Take(3) // send only few courses
         .ToList();

            ViewBag.FewCourses = fewCourses;

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
