using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace LearningManagementSystem.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly IMapper mapper;
        private readonly LMSDbContext lMSDbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public HomeController(IMapper mapper,LMSDbContext lMSDbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.mapper = mapper;
            this.lMSDbContext = lMSDbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
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
            TempData["SuccessMessage"] = "Thank you! We have received your message.";

            return RedirectToAction("Index", TempData["SuccessMessage"]);
        }
        [HttpGet]
        public IActionResult DeleteAccount()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(AccountDeletionReason accountDeletionReason)
        {
            if (!ModelState.IsValid)
            {
                return View("DeleteAccount",accountDeletionReason);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
          
            // Save reason in database
            var deletionReason = new AccountDeletionReasonDM
            {
                UserId = user.Id,
                Reason = accountDeletionReason.Reason,
                DeletedAt = DateTime.Now
            };
            var reason = mapper.Map<AccountDeletionReason>(accountDeletionReason);
            lMSDbContext.AccountDeletionReasons.Add(reason);
            await lMSDbContext.SaveChangesAsync();

            // Delete user
            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await signInManager.SignOutAsync();
                TempData["SuccessMessage"] = "Your account has been deleted successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("DeleteAccount", accountDeletionReason);
        }
    }
}
