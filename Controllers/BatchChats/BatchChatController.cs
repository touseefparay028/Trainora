using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.BatchChats
{
    public class BatchChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LMSDbContext lMSDbContext;

        public BatchChatController(UserManager<ApplicationUser> userManager, LMSDbContext lMSDbContext)
        {
            _userManager = userManager;
            this.lMSDbContext = lMSDbContext;
        }

        [Route("GoChat/{id:guid}")]
        public async Task<IActionResult> Chat(Guid id) // id = BatchId
        {
            var user = await _userManager.GetUserAsync(User);

            // If user or batch is invalid, redirect or show message
            if (user == null)
                return RedirectToAction("LoginStudent", "Student");

            if (id == Guid.Empty)
                return BadRequest("Invalid batch selected.");

            var model = new BatchChatViewModel
            {
                BatchDMId = id,
                UserName = user.Name ?? user.UserName
            };

            var batch = lMSDbContext.BatchDMs.Find(model.BatchDMId);
            ViewBag.batchname = batch.Name;

            return View(model);
        }
    }
}
