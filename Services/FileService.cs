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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly LMSDbContext lMSDbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public FileService(UserManager<ApplicationUser> userManager,LMSDbContext lMSDbContext, IWebHostEnvironment webHostEnvironment, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
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
            var bid = lMSDbContext.BatchDMs.Find(assignmentVM.BatchDMId);
            var bname= bid != null ? bid.Name : "Unknown Batch";
            var annoucement = new Announcements
            {
                Description = $"Assignment: '{assignmentVM.Title}' for Batch: '{bname}' has been created.",
                FilePath=null,
                CreatedBy = Guid.Parse(UserId),
                CreatedAt = DateTime.Now
            };
            await lMSDbContext.Announcements.AddAsync(annoucement);
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
        public async Task<List<TeacherAssignmentVM>> GetCreatedAssignments()
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var uid = Guid.Parse(userId);
            if (string.IsNullOrEmpty(userId))
            {
               
                return new List<TeacherAssignmentVM>();  // if not logged in.(Optional)
            }

            // Get only assignments created by this user
            var assignments = await lMSDbContext.AssignmentDMs
                .Where(a => a.ApplicationUserId == uid)
                .Include(a => a.BatchDM)
                .ToListAsync();
            var assignment = mapper.Map<List<TeacherAssignmentVM>>(assignments);
            return assignment;
        }
        public async Task<List<TeacherAssignmentVM>> GetFilesAsync()
        {
            var studentId = httpContextAccessor.HttpContext?.User?
    .FindFirstValue(ClaimTypes.NameIdentifier);

            var studentGuid = Guid.Parse(studentId);

            // 1️⃣ Get the student’s batch
            var studentBatchId = await userManager.Users
                .Where(s => s.Id == studentGuid)
                .Select(s => s.BatchDMId)
                .FirstOrDefaultAsync();

            // 2️⃣ Get IDs of assignments already submitted by this student
            var submittedIds = await lMSDbContext.StudentAssignmentDM
                .Where(sa => sa.StudentId == studentGuid)
                .Select(sa => sa.assignmentDMId)
                .ToListAsync();

            // Get pending assignments for this batch
            var pendingAssignments = await lMSDbContext.AssignmentDMs
                .Where(a => a.BatchDMId == studentBatchId && !submittedIds.Contains(a.Id))
                .ToListAsync();

            // 4️⃣ Map to ViewModel
            var assignmentVM = mapper.Map<List<TeacherAssignmentVM>>(pendingAssignments);

            return assignmentVM;

        }
        public async Task<List<StudentAssignmentVM>> SubmittedAssignments(Guid Id)
        {
            var assignments = await lMSDbContext.StudentAssignmentDM
           .Where(sa => sa.assignmentDMId == Id)
           .ToListAsync();
            var SubmittedAssignments = mapper.Map<List<StudentAssignmentVM>>(assignments);
            return SubmittedAssignments;
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
            var UserId = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(UserId);
            var submission = new StudentAssignmentDM
            {
                Id = assignmentVM.Id,
                StudentName = user.Name,
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
            var user = await userManager.FindByIdAsync(UserId);
            StudyMaterialsDM StudyMaterial = mapper.Map<StudyMaterialsDM>(studyMaterialsVM);
            StudyMaterial.FilePath = FileName;
            //StudyMaterial.Title = studyMaterialsVM.Title;
            StudyMaterial.UploadedBy = user?.Name?? "Unknown";
            //StudyMaterial.Description = studyMaterialsVM.Description;
            StudyMaterial.UploadedOn = DateTime.Now;
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
        public async Task CreateAnnouncements(AnnouncementsVM announcements)
        {
            string FolderPath = Path.Combine(webHostEnvironment.WebRootPath, "Announcements");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string FileName = Guid.NewGuid().ToString() + "_" + announcements.File.FileName;
            string FullPath = Path.Combine(FolderPath, FileName);
            using (FileStream FileStream = new FileStream(FullPath, FileMode.Create))
            {
                await announcements.File.CopyToAsync(FileStream);
            }
            var UserId = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(UserId))
            {
                throw new InvalidOperationException("No user is logged in right now");
            }
            Announcements data = mapper.Map<Announcements>(announcements);
            data.FilePath = FileName;
            
            data.CreatedBy = Guid.Parse(UserId);
            await lMSDbContext.Announcements.AddAsync(data);
            await lMSDbContext.SaveChangesAsync();
        }
        public async Task<List<AnnouncementsVM>> GetAllAnnouncements()
        {
            var announcements = await lMSDbContext.Announcements.ToListAsync();
            var announcementsVM = mapper.Map<List<AnnouncementsVM>>(announcements);
            return announcementsVM;
        }
        public async Task UploadCourseMaterialAsync(CourseMaterialVM courseMaterialVM)
        {
            string FolderPath = Path.Combine(webHostEnvironment.WebRootPath, "CourseMaterials");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string FileName = Guid.NewGuid().ToString() + "_" + courseMaterialVM.File.FileName;
            string FullPath = Path.Combine(FolderPath, FileName);
            using (FileStream FileStream = new FileStream(FullPath, FileMode.Create))
            {
                await courseMaterialVM.File.CopyToAsync(FileStream);
            }

            var UserId = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(UserId))
            {
                throw new InvalidOperationException("No user is logged in right now");
            }
            var user = await userManager.FindByIdAsync(UserId);
            CourseMaterial CourseMaterial = mapper.Map<CourseMaterial>(courseMaterialVM);
            CourseMaterial.FilePath = FileName;
            CourseMaterial.UploadedOn = DateTime.Now;
            //StudyMaterial.Title = studyMaterialsVM.Title
            //StudyMaterial.Description = studyMaterialsVM.Description;
            CourseMaterial.UploadedOn = DateTime.Now;
            //CourseMaterial.ApplicationUserId = Guid.Parse(UserId);
            await lMSDbContext.CourseMaterial.AddAsync(CourseMaterial);
            await lMSDbContext.SaveChangesAsync();
        }
        public async Task<List<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(Guid courseId)
        {
            var material= await lMSDbContext.CourseMaterial
                .Where(m => m.CourseId == courseId)
                .OrderByDescending(m => m.UploadedOn)
                .ToListAsync();
           
            return material;
        }


    }
}
