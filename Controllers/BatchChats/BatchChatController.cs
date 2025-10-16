using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.BatchChats
{
    public class BatchChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public BatchChatController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Route("GoChat/{id:guid}")]
        public async Task<IActionResult> Chat(Guid id) // id = BatchId
        {
            var user = await _userManager.GetUserAsync(User);

            // If user or batch is invalid, redirect or show message
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (id == Guid.Empty)
                return BadRequest("Invalid batch selected.");

            var model = new BatchChatViewModel
            {
                BatchDMId = id,
                UserName = user.Name ?? user.UserName
            };

            return View(model);
        }
    }
}
