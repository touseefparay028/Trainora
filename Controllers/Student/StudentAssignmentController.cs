using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
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
            ModelState.Remove("AssignmentTitle");
            ModelState.Remove("Path");
            ModelState.Remove("StudentName");
            if (ModelState.IsValid)
            {
                await fileService.SubmitAssignmentAsync(assignmentVM, studentID);
                return RedirectToAction("StudentDashboard", "StudentDashboard");
            }
         return View("SubmitAssignment", assignmentVM);
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public async Task<IActionResult> MySubmissions()
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Load DM list
            var submissionsDM = await lMSDbContext.StudentAssignmentDM
                .Include(x => x.assignmentDM)
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            // Map DM → VM
            List<StudentAssignmentVM> submissionsVM = submissionsDM.Select(x => new StudentAssignmentVM
            {
                Id = x.Id,
                assignmentDMId = x.assignmentDMId,
                AssignmentTitle = x.assignmentDM?.Title ?? "No Title",
                Path = x.Path,
                IsReverted = x.IsReverted

            }).ToList();

            return View(submissionsVM);
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public IActionResult ViewAssignment(string Path)
        {

            return new VirtualFileResult($"StudentFiles/{Path}", "application/pdf")
            {
                FileDownloadName = Path
            };
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var submission = await lMSDbContext.StudentAssignmentDM
                .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

            if (submission == null) return NotFound();
            if (!submission.IsReverted) return Forbid(); // safety

            var vm = new UpdateAssignmentVM
            {
                Id = submission.Id
            };

            return View(vm);
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateAssignmentVM model)
        {
            if (!ModelState.IsValid)
                return View("Update",model);

            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var submission = await lMSDbContext.StudentAssignmentDM
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.StudentId == studentId);

            if (submission == null) return NotFound();

            // ----- DELETE OLD FILE -----
            var oldFilePath = Path.Combine("wwwroot/StudentFiles", submission.Path);

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            // ----- SAVE NEW FILE -----
            var newFileName = $"{Guid.NewGuid()}_{model.NewFile.FileName}";
            var newPath = Path.Combine("wwwroot/StudentFiles", newFileName);

            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                await model.NewFile.CopyToAsync(stream);
            }

            // ----- UPDATE DB RECORD -----
            submission.Path = newFileName;
            submission.SubmittedAt = DateTime.Now;
            submission.IsReverted = false; // Now it goes back to normal

            await lMSDbContext.SaveChangesAsync();

            return RedirectToAction("MySubmissions");
        }



    }
}
