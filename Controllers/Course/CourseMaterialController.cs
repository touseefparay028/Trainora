using AutoMapper;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.Course
{
    public class CourseMaterialController : Controller
    {
        private readonly IFileService fileService;
        private readonly IMapper mapper;

        public CourseMaterialController(IFileService fileService,IMapper mapper)
        {
            this.fileService = fileService;
            this.mapper = mapper;
        }
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        [HttpGet]
        public IActionResult UploadCourseMaterial(Guid CourseId)
        {
            ViewBag.CourseId = CourseId;
            return View();
        }
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        public async Task<IActionResult> UploadCourseMaterials(CourseMaterialVM courseMaterialVM)
        {
            if (ModelState.IsValid)
            {

                await fileService.UploadCourseMaterialAsync(courseMaterialVM);
                return RedirectToAction("GetCourseMaterials", new { courseId = courseMaterialVM.CourseId });
            }
            ViewBag.CourseId = courseMaterialVM.CourseId;
            return View("UploadCourseMaterial", courseMaterialVM);
        }
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> GetCourseMaterials(Guid courseId)
        {
            var materials = await fileService.GetCourseMaterialsByCourseIdAsync(courseId);
            var materialVMs = materials.Select(m => new CourseMaterialVM
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                FilePath = m.FilePath,
                UploadedOn = m.UploadedOn,
                CourseId = m.CourseId
            }).ToList();
            ViewBag.CourseId = courseId;
            return View(materialVMs);
        }

    }
}
