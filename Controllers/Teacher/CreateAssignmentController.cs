using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LearningManagementSystem.Controllers.Teacher
{
    [Authorize(Roles ="Teacher")]
    public class CreateAssignmentController : Controller
    {
      
        private readonly LMSDbContext lMSDbContext;
        private readonly IFileService fileService;
        private readonly IMapper mapper;

        public CreateAssignmentController(LMSDbContext lMSDbContext,IFileService fileService, IMapper mapper )
        {
            
            this.lMSDbContext = lMSDbContext;
            this.fileService = fileService;
            this.mapper = mapper;
        }

        [Route("CreateAssignment")]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> CreateAssignment()
        {
            var model = new TeacherAssignmentVM
            {
                BatchList = await fileService.GetBatchSelectListAsync()
            };
            return View(model);
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
       public async Task<IActionResult> Create(TeacherAssignmentVM assignmentVM)
        {
            if(ModelState.IsValid)
            {
                
                await fileService.CreateAssignmentAsync(assignmentVM);
                return RedirectToAction("GetCreatedAssignments");
            }
            assignmentVM.BatchList = await fileService.GetBatchSelectListAsync();
            return View("CreateAssignment", assignmentVM);
        }
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> GetCreatedAssignments()
        {
         return View(await fileService.GetCreatedAssignments());
        }
        [Route("CreateAssignment/GetFilesAsync")]
       
        public async Task<IActionResult> GetListAsync()
        {

            return View(await fileService.GetFilesAsync());
        }
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> SubmittedAssignments(Guid Id)
        {
            
            return View(await fileService.SubmittedAssignments(Id));
        }
        [Route("CreateAssignment/Download")]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public IActionResult Download(string FilePath)
        {

            return new VirtualFileResult($"Files/{FilePath}", "application/octet-stream")
            {
                FileDownloadName = FilePath
            };
        }
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        public IActionResult Delete(Guid id)
        {
            // 1. Fetch from DM (database)
            var assignmentDM = lMSDbContext.AssignmentDMs.FirstOrDefault(a => a.Id == id);
            if (assignmentDM == null)
            {
                ModelState.AddModelError(string.Empty, "not found");
                return View("GetCreatedAssignments", assignmentDM);
            }

            // 2. Delete file if exists
            if (!string.IsNullOrEmpty(assignmentDM.Path))
            {
                var filePath = Path.Combine("wwwroot", "Files", assignmentDM.Path); // adjust if needed
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // 3. Remove record from DB
            lMSDbContext.AssignmentDMs.Remove(assignmentDM);
            lMSDbContext.SaveChanges();

            // 4. Redirect back to List of the assignments.
            return RedirectToAction("GetCreatedAssignments");
        }
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        public IActionResult DeleteSubmission(Guid id)
        {
           
            var submission  = lMSDbContext.StudentAssignmentDM.FirstOrDefault(a => a.Id == id);
            if (submission == null)
            {
                ModelState.AddModelError(string.Empty, "not found");
                return View("SubmittedAssignments", submission);
            }

          
            if (!string.IsNullOrEmpty(submission.Path))
            {
                var filePath = Path.Combine("wwwroot", "StudentFiles", submission.Path); 
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

        
            lMSDbContext.StudentAssignmentDM.Remove(submission);
            lMSDbContext.SaveChanges();

            // 4. Redirect back to List of the assignments.
            return RedirectToAction("SubmittedAssignments");
        }
        //[HttpPost("Edit/{id}")]
        //public IActionResult Edit(Guid id)
        //{
        //    var assignment = lMSDbContext.AssignmentDMs.FirstOrDefault(x => x.Id == id);
        //    if (assignment == null)
        //    {
        //        return NotFound();
        //    }

        //    // Map DM to VM
        //    TeacherAssignmentVM teacherAssignmentVM = mapper.Map<TeacherAssignmentVM>(assignment);

        //    return View(teacherAssignmentVM);
        //}
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        public IActionResult ViewAssignment(string Path)
        {

            return new VirtualFileResult($"StudentFiles/{Path}", "application/pdf")
            {
                FileDownloadName = Path
            };
        }

    }
}
