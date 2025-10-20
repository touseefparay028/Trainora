using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly LMSDbContext lMSDbContext;

        public HomeController(LMSDbContext lMSDbContext)
        {
            this.lMSDbContext = lMSDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Route("About")]
        public IActionResult About()
        {
            return View();
        }
        [Route("Join")]
        public IActionResult Join()
        {
            return View();
        }
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [Route("Features")]
        public IActionResult Features()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Submit contact form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("Contact",model);
            }
            lMSDbContext.Contact.Add(new ContactDM
            {
                id=Guid.NewGuid(),
                Name=model.Name,
                Email=model.Email,
                Message=model.Message
            });
            lMSDbContext.SaveChanges();
            // TODO: save to database or send email
            TempData["Message"] = "Thank you! We have received your message.";

            return RedirectToAction("Index");
        }
    }
}
