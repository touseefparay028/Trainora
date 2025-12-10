using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace LearningManagementSystem.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly IEmailService emailService;
        private readonly IMapper mapper;
        private readonly LMSDbContext lMSDbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public HomeController(IEmailService emailService, IMapper mapper,LMSDbContext lMSDbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.emailService = emailService;
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
        public IActionResult AccessDenied(string? returnurl)
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
        [Authorize(AuthenticationSchemes ="AdminAuth,TeacherAuth,StudentAuth",Roles ="Admin,Teacher,Student")]
        public IActionResult DeleteAccount()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes ="AdminAuth,TeacherAuth,StudentAuth",Roles ="Admin,Teacher,Student")]
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
                await HttpContext.SignOutAsync("AdminAuth");
                await HttpContext.SignOutAsync("TeacherAuth");
                await HttpContext.SignOutAsync("StudentAuth");
                TempData["SuccessMessage"] = "Your account has been deleted successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("DeleteAccount", accountDeletionReason);
        }
        public IActionResult newmodel()
        {
            return View();
        }
        public IActionResult Documentation()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Message"] = "A reset link is sent to the email.";
                return RedirectToAction("ForgotPassword");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var link = Url.Action("ResetPassword", "Home",
                new { email = email, token = token },
                protocol: HttpContext.Request.Scheme);
            string subject = "Password Reset Link";
            string htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0' />
<title>Password Reset</title>
<style>
    body {{
        font-family: Arial, sans-serif;
        background-color: #f5f6fa;
        padding: 0;
        margin: 0;
    }}
    .container {{
        max-width: 600px;
        margin: auto;
        background: white;
        padding: 30px;
        border-radius: 10px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
    }}
    .header {{
        text-align: center;
        margin-bottom: 30px;
    }}
    .header h2 {{
        color: #0056b3;
        font-size: 28px;
        margin-bottom: 10px;
    }}
    .content p {{
        font-size: 15px;
        color: #333;
        line-height: 1.6;
    }}
    .btn {{
        display: inline-block;
        padding: 12px 25px;
        background-color: #0056b3;
        color: white !important;
        border-radius: 6px;
        text-decoration: none;
        font-weight: bold;
        margin: 20px 0;
    }}
    .footer {{
        text-align: center;
        margin-top: 30px;
        font-size: 12px;
        color: #777;
    }}
</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>TrainOra Password Reset</h2>
        </div>

        <div class='content'>
            <p>Hello,</p>

            <p>We received a request to reset your TrainOra account password. 
            If this was you, click the button below to set a new password.</p>

            <p style='text-align:center;'>
                <a href='{link}' class='btn'>Reset Password</a>
            </p>

            <p>If you did not request a password reset, you can safely ignore this email. 
            Your current password will continue to work.</p>

            <p>This link will expire in <b>30 minutes</b> for your security.</p>
        </div>

        <div class='footer'>
            <p>TrainOra © {DateTime.Now.Year} — Smart Learning Management System</p>
        </div>
    </div>
</body>
</html>";

            await emailService.SendResetLinkAsync(email, subject, htmlContent);

            TempData["Message"] = "Check your email for reset instructions.";
            return RedirectToAction("ForgotPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null)
                return BadRequest("Invalid reset link.");

            ViewBag.Email = email;
            ViewBag.Token = token;

            // We return ChangePasswordVM but we DO NOT USE CurrentPassword
            return View(new ChangePasswordVM());
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View("ResetPassword",model);

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return View("ResetPassword",model);
            }

            // Reset password directly WITHOUT using CurrentPassword
            var resetResult = await userManager.ResetPasswordAsync(
                user, token, model.NewPassword
            );

            if (resetResult.Succeeded)
            {
               
                var roles = await userManager.GetRolesAsync(user);

                
                if (roles.Contains("Student"))
                {
                    TempData["Message"] = "Password reset successful.";
                    return RedirectToAction("LoginStudent", "Student");
                   
                }
                else if (roles.Contains("Teacher"))
                {
                    TempData["Message"] = "Password reset successful.";
                    return RedirectToAction("LoginTeacher", "Teacher");
                   
                }
                else if (roles.Contains("Admin"))
                {
                    TempData["Message"] = "Password reset successful.";
                    return RedirectToAction("Login", "Admin");
                    
                }
            }

            foreach (var error in resetResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View("ResetPassword", model);
        }


    }
}
