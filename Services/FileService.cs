using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Services
{
    public class FileService : IFileService
    {

        private readonly LMSDbContext lMSDbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public FileService(LMSDbContext lMSDbContext, IWebHostEnvironment webHostEnvironment, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {

            this.lMSDbContext = lMSDbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<SelectListItem>> GetBatchSelectListAsync()
        {
            return await lMSDbContext.BatchDMs
            .Select(b => new SelectListItem
            {
                Value = b.id.ToString(),
                Text = b.Name
            }).ToListAsync();
        }
        public async Task CreateAssignmentAsync(TeacherAssignmentVM assignmentVM)
        {
            string FolderPath = Path.Combine(webHostEnvironment.WebRootPath, "Files");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string FileName = Guid.NewGuid().ToString() + "_" + assignmentVM.File.FileName;
            string FullPath = Path.Combine(FolderPath, FileName);
            using (FileStream FileStream = new FileStream(FullPath, FileMode.Create))
            {
                await assignmentVM.File.CopyToAsync(FileStream);
            }
            var UserId = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(UserId))
            {
                throw new InvalidOperationException("No user is logged in right now");
            }
            TeacherAssignmentDM data = mapper.Map<TeacherAssignmentDM>(assignmentVM);
            data.Path = FileName;
            data.BatchDMId = (Guid)assignmentVM.BatchDMId;
            data.ApplicationUserId = Guid.Parse(UserId);
            await lMSDbContext.AssignmentDMs.AddAsync(data);
            await lMSDbContext.SaveChangesAsync();
        }
        public async Task<List<TeacherAssignmentVM>> GetFilteredFiles()
        {
            var data = await lMSDbContext.AssignmentDMs
            .Include(a => a.BatchDM)
            .Include(a => a.ApplicationUser)
            .ToListAsync();
            return mapper.Map<List<TeacherAssignmentVM>>(data);
        }
        public async Task<List<TeacherAssignmentVM>> GetFilesAsync()
        {
            var studentId = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            //var stid = Guid.Parse(studentId);
            //var data = await lMSDbContext.AssignmentDMs.ToListAsync();

            //// Map to VM
            //var assignmentVM = mapper.Map<List<TeacherAssignmentVM>>(data);

            //// Check submissions for the current student
            //foreach (var assignment in assignmentVM)
            //{
            //    assignment.IsSubmitted = await lMSDbContext.StudentAssignmentDM
            //        .AnyAsync(s => s.assignmentDMId == assignment.Id && s.StudentId == stid);
            //}
            // All assignments

            //THIS ONE USED TO GET THE LIST OF THE PENDING ASSIGNMENTS ONLY.
            var assignments = await lMSDbContext.AssignmentDMs.ToListAsync();

            // Submissions by this student
            var submittedIds = await lMSDbContext.StudentAssignmentDM
                .Where(sa => sa.StudentId == Guid.Parse(studentId))
                .Select(sa => sa.assignmentDMId)
                .ToListAsync();

            // Only pending assignments
            var pendingAssignments = assignments
                .Where(a => !submittedIds.Contains(a.Id))
                .ToList();

            // Map to your VM
            var assignmentVM = mapper.Map<List<TeacherAssignmentVM>>(pendingAssignments);
            return assignmentVM;
            //List<TeacherAssignmentDM> data = await lMSDbContext.AssignmentDMs.ToListAsync();
            //List<TeacherAssignmentVM> assignmentVM = mapper.Map<List<TeacherAssignmentVM>>(data);
            //return assignmentVM;
        }
        public async Task SubmitAssignmentAsync(StudentAssignmentVM assignmentVM, Guid StudentID)
        {

            string folderPath = Path.Combine(webHostEnvironment.WebRootPath, "StudentFiles");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string FileName = Guid.NewGuid().ToString() + "_" + assignmentVM.File.FileName;
            string FullPath = Path.Combine(folderPath, FileName);
            using (var stream = new FileStream(FullPath, FileMode.Create))
            {
                await assignmentVM.File.CopyToAsync(stream);
            }
            var submission = new StudentAssignmentDM
            {
                Id = assignmentVM.Id,
                StudentName = assignmentVM.StudentName,
                StudentId = StudentID,
                assignmentDMId = (Guid)assignmentVM.assignmentDMId,
                Path = FileName,
                SubmittedAt = DateTime.Now
            };
            await lMSDbContext.StudentAssignmentDM.AddAsync(submission);
            await lMSDbContext.SaveChangesAsync();
        }
        public async Task UploadStudyMaterialAsync(StudyMaterialsVM studyMaterialsVM)
        {
            string FolderPath = Path.Combine(webHostEnvironment.WebRootPath, "StudyMaterials");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string FileName = Guid.NewGuid().ToString() + "_" + studyMaterialsVM.File.FileName;
            string FullPath = Path.Combine(FolderPath, FileName);
            using (FileStream FileStream = new FileStream(FullPath, FileMode.Create))
            {
                await studyMaterialsVM.File.CopyToAsync(FileStream);
            }

            var UserId = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(UserId))
            {
                throw new InvalidOperationException("No user is logged in right now");
            }
            StudyMaterialsDM StudyMaterial = mapper.Map<StudyMaterialsDM>(studyMaterialsVM);
            StudyMaterial.FilePath = FileName;
            //StudyMaterial.Title = studyMaterialsVM.Title;
            //StudyMaterial.UploadedBy =studyMaterialsVM.UploadedBy;
            //StudyMaterial.Description = studyMaterialsVM.Description;
            //StudyMaterial.UploadedOn= studyMaterialsVM.UploadedOn;
            StudyMaterial.ApplicationUserId = Guid.Parse(UserId);
            await lMSDbContext.StudyMaterials.AddAsync(StudyMaterial);
            await lMSDbContext.SaveChangesAsync();
        }
        public async Task<List<StudyMaterialsVM>> GetMaterialAsync()
        {
            var studyMaterials = await lMSDbContext.StudyMaterials.ToListAsync();
            var studyMaterialsVM = mapper.Map<List<StudyMaterialsVM>>(studyMaterials);
            return studyMaterialsVM;
        }

    }
}
