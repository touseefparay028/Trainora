using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.Announcements
{
    public class AnnouncementsController : Controller
    {
        private readonly IFileService fileService;

        public AnnouncementsController(IFileService fileService)
        {
            this.fileService = fileService;
        }
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> Create(AnnouncementsVM announcementsVM)
        {
            if (ModelState.IsValid)
            {

                await fileService.CreateAnnouncements(announcementsVM);
                return RedirectToAction("GetAnnouncements");
            }
            return View("Create",announcementsVM);
        }
    }
}
