using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.Announcements
{
    
    public class AnnouncementsController : Controller
    {

        private readonly IFileService fileService;
        private readonly LMSDbContext lMSDbContext;
        private readonly RoleManager<ApplicationRole> roleManager;

        public AnnouncementsController(IFileService fileService,LMSDbContext lMSDbContext,RoleManager<ApplicationRole> roleManager)
        {
            this.fileService = fileService;
            this.lMSDbContext = lMSDbContext;
            this.roleManager = roleManager;
        }
        [Authorize(Roles="Admin,Teacher")]
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> CreateAnnouncement(AnnouncementsVM announcementsVM)
        {
            if (ModelState.IsValid)
            {

                await fileService.CreateAnnouncements(announcementsVM);
                return RedirectToAction("GetAnnouncements");
            }
            return View("Create",announcementsVM);
        }
        [Authorize(Roles="Admin,Teacher,Student")]
        public async Task<IActionResult> GetAnnouncements()
        {
            var announcements = await fileService.GetAllAnnouncements();
            return View(announcements);
        }
        [Authorize(Roles="Admin,Teacher,Student")]
        public IActionResult ViewAnnouncement(string FilePath)
        {
            return new VirtualFileResult($"Announcements/{FilePath}", "application/application.pdf")
            {
                FileDownloadName = FilePath
            };
            
        }
        public IActionResult DeleteAnnouncement(Guid Id)
        {
            var ann = lMSDbContext.Announcements.Find(Id);
            if (ann == null) {
            return NotFound();
            }
            lMSDbContext.Announcements.Remove(ann);
            lMSDbContext.SaveChanges();
            return RedirectToAction("GetAnnouncements");
        }

    }
}
