using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.StudyMaterials
{
    public class StudyMaterialsController : Controller
    {
        private readonly IFileService fileService;

        public StudyMaterialsController(IFileService fileService)
        {
            this.fileService = fileService;
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
    }
}
