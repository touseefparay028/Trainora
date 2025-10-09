using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers.StudyMaterials
{
    public class StudyMaterialsController : Controller
    {
        private readonly IFileService fileService;
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;

        public StudyMaterialsController(IFileService fileService,LMSDbContext lMSDbContext,IMapper mapper)
        {
            this.fileService = fileService;
            this.lMSDbContext = lMSDbContext;
            this.mapper = mapper;
        }
        [Route("StudyMaterials")]
        public IActionResult UploadStudyMaterial()
        {
            return View();
        }
        [HttpPost("UploadNow")]
        public async Task<IActionResult> UploadStudyMaterials(StudyMaterialsVM studyMaterialsVM)
        {
            if (ModelState.IsValid)
            {

                await fileService.UploadStudyMaterialAsync(studyMaterialsVM);
                return RedirectToAction("GetMaterials");
            }

            return View("UploadStudyMaterial", studyMaterialsVM);
        }
        [Route("GetMaterial")]
        public async Task<IActionResult> GetMaterials()
        {
            return View(await fileService.GetMaterialAsync());
        }
        public IActionResult MyMaterials()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uid = Guid.Parse(userId);
            var materials = lMSDbContext.StudyMaterials
                .Where(m => m.ApplicationUserId == uid)
                .ToList();
            var materialVMs = mapper.Map<List<StudyMaterialsVM>>(materials);

            return View(materialVMs);
        }
        public IActionResult DownloadMaterial(string FilePath)
        {

            return new VirtualFileResult($"StudyMaterials/{FilePath}", "application/octet-stream")
            {
                FileDownloadName = FilePath
            };
        }
        public IActionResult DeleteMaterial(Guid id)
        {
            // 1. Fetch from DM (database)
            var StudyMaterial = lMSDbContext.StudyMaterials.FirstOrDefault(a => a.Id == id);
            if (StudyMaterial == null)
            {
                ModelState.AddModelError(string.Empty, "not found");
                return View("Mymaterials", StudyMaterial);
            }

            // 2. Delete file if exists
            if (!string.IsNullOrEmpty(StudyMaterial.FilePath))
            {
                var filePath = Path.Combine("wwwroot", "StudyMaterials", StudyMaterial.FilePath); // adjust if needed
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // 3. Remove record from DB
            lMSDbContext.StudyMaterials.Remove(StudyMaterial);
            lMSDbContext.SaveChanges();

            // 4. Redirect back to List of the assignments.
            return RedirectToAction("MyMaterials");
        }
    }
}