using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers.Student
{
    [Authorize(Roles ="Student")]
    public class StudentAssignmentController : Controller
    {
        private readonly IFileService fileService;
        private readonly LMSDbContext lMSDbContext;

        public StudentAssignmentController(IFileService fileService, LMSDbContext lMSDbContext)
        {
            this.fileService = fileService;
            this.lMSDbContext = lMSDbContext;
        }
        [Route("Submit")]
        [Authorize(AuthenticationSchemes ="StudentAuth",Roles ="Student")]
        public async Task<IActionResult> SubmitAssignment(Guid Id)
        {

            var studentID = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingSubmission = await lMSDbContext.StudentAssignmentDM
                .FirstOrDefaultAsync(sa => sa.StudentId == studentID && sa.assignmentDMId == Id);
            if (existingSubmission != null)
            {
                TempData["ErrorMessage"] = "You have already submitted this assignment.";
                return RedirectToAction("GetListA", "Student");
            }
            return View(new StudentAssignmentVM { assignmentDMId=Id });
        }
        [HttpPost("PostSubmit")]
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public async Task<IActionResult> Submit(StudentAssignmentVM assignmentVM)
        {
            var studentID = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ModelState.Remove("Path");
            ModelState.Remove("StudentName");
            if (ModelState.IsValid)
            {
                await fileService.SubmitAssignmentAsync(assignmentVM, studentID);
                return RedirectToAction("StudentDashboard", "StudentDashboard");
            }
         return View("SubmitAssignment", assignmentVM);
        }
    }
}
