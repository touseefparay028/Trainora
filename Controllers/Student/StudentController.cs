using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.RoleEnums;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Text.Encodings.Web;

namespace LearningManagementSystem.Controllers.Student
{
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;
        private readonly IFileService fileService;

        public LMSDbContext LMSDbContext { get; }

        public StudentController(LMSDbContext lMSDbContext,UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IMapper Mapper, IEmailService emailService, IFileService fileService)
        {
            LMSDbContext = lMSDbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            mapper = Mapper;
            this.emailService = emailService;
            this.fileService = fileService;
        }
        [Route("StudentRegister")]
        public async Task<IActionResult> Create()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("StudentDashboard", "StudentDashboard");
            }
            var model = new RegisterDTO
            {
                BatchList = await fileService.GetBatchSelectListAsync()
            };
            return View(model);
        }
        [HttpPost("PostStudentRegister")]
        public async Task<IActionResult> CreateStudent(RegisterDTO registerDTO)
        {

            if (!ModelState.IsValid)
            {
              
                ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage);
                registerDTO.BatchList=await fileService.GetBatchSelectListAsync();
                return View("Create", registerDTO);
            }
            //AutoMApper
            ApplicationUser user = mapper.Map<ApplicationUser>(registerDTO);
            //ApplicationUser user = new ApplicationUser()
            //{
            //    Name=registerDTO.Name,  
            //    Email=registerDTO.Email,    
            user.PhoneNumber = registerDTO.Phone;
            //    UserName=registerDTO.Email

            //};
            IdentityResult result = await userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                // Generate confirmation token
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                string Name = registerDTO.Name;
                // Build confirmation link
                var confirmationLink = Url.Action(nameof(StudentConfirmEmail), "Student",
                    new { userId = user.Id, token }, Request.Scheme);
                string emailBody = $@"
<!DOCTYPE html>
<html lang='en'>
@""
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verification</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f6f8;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border-top: 5px solid #4CAF50;
        }}
        .header {{
            background-color: #4CAF50;
            color: #ffffff;
            padding: 20px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
        }}
        .body {{
            padding: 30px 20px;
            color: #333333;
            line-height: 1.6;
            font-size: 16px;
        }}
        .body h2 {{
            color: #4CAF50;
        }}
        .btn {{
            display: inline-block;
            margin: 20px 0;
            padding: 12px 25px;
            background-color: #4CAF50;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 50px;
            font-weight: bold;
            font-size: 16px;
        }}
        .footer {{
            background-color: #f4f6f8;
            text-align: center;
            color: #999999;
            font-size: 12px;
            padding: 20px;
        }}
        .footer a {{
            color: #4CAF50;
            text-decoration: none;
        }}
        @media screen and (max-width: 600px) {{
            .container {{
                margin: 20px;
            }}
            .body {{
                padding: 20px 15px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            Trainora LMS
        </div>
        <div class='body'>
            <h2>Hello {Name},</h2>
            <p>Thank you for registering as a student on Trainora Learning Management System.</p>
            <p>To complete your registration and access your account, please verify your email address by clicking the button below:</p>
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='btn'>Verify Email</a>
            </p>
            <p>If the button above does not work, copy and paste the following link into your browser:</p>
            <p style='word-break: break-all; color: #4CAF50;'>{confirmationLink}</p>
            <p>Once verified, you can log in and start exploring courses and learning materials immediately.</p>
            <p>Welcome aboard!</p>
            <p>Best regards,<br/>Trainora LMS Team</p>
        </div>
        <div class='footer'>
            &copy; 2025 Trainora LMS. All rights reserved.<br/>
            For support, contact <a href='mailto:support@trainora.com'>support@trainora.com</a>
        </div>
    </div>
</body>
</html>
""

</html>";
                // Send Email
                await emailService.SendVerificationEmailAsync(registerDTO.Email, "Confirm your email",
                   emailBody);

                if (await roleManager.FindByNameAsync(UserTypeOptions.Student.ToString()) is null)
                {

                    ApplicationRole applicationRole = new ApplicationRole()
                    {
                        Name = UserTypeOptions.Student.ToString()
                    };
                    await roleManager.CreateAsync(applicationRole);
                }
                await userManager.AddToRoleAsync(user, UserTypeOptions.Student.ToString());

               
                return RedirectToAction("StudentRegistrationSuccessful");

            }


            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
              
            }
            registerDTO.BatchList = await fileService.GetBatchSelectListAsync();
            return View("Create", registerDTO);

        }

        public IActionResult StudentRegistrationSuccessful()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> StudentConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return View("Error");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return View("Error");

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                string Name = user.Name;
                string Email = user.Email;
                string subject = "Student Registration Successful";

                string body = $"Hello {Name},\n\n" +
              "Your account has been successfully verified as a student.\n\n" +
              "You can now access your student dashboard, explore available courses, and start engaging with learning materials, assignments, and announcements. " +
              "As a verified student, you are now part of our academic community dedicated to growth, collaboration, and excellence. " +
              "We encourage you to stay active, participate in discussions, and make the most of the opportunities available to you.\n\n" +
              "If you have any questions or face any issues, feel free to reach out to our support team at support@Trainora.com for assistance.\n\n" +
              "Best regards,\n" +
              "Team Train𝓞ra";
                await emailService.SendMail(Email, subject, body);
                return View("StudentConfirmEmailSuccess");
            }
                

            return View("Error");
        }
        public IActionResult StudentConfirmEmailSuccess()
        {
            return View();
        }


        [Route("StudentLogin")]
        public IActionResult LoginStudent()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("StudentDashboard", "StudentDashboard");
            }

            return View();
        }

        [HttpPost("PostStudentLogin")]
        
        public async Task<IActionResult> StudentLogin(LoginDTO loginDTO, string? ReturnUrl = null)
        {


            if (!ModelState.IsValid)
            {
                ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage);
                return View("LoginStudent", loginDTO);
            }


            ApplicationUser? user = await userManager.FindByEmailAsync(loginDTO.Email);
            if (user != null)
            {
                // ✅ Check if email is confirmed
                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty,
                        "Your email is not veirified yet. Please check your email inbox to verify your account.");
                    return View("LoginStudent", loginDTO);
                }
                if (await userManager.IsInRoleAsync(user, UserTypeOptions.Student.ToString()))
                {
                    var result = await signInManager.PasswordSignInAsync(user, loginDTO.Password, isPersistent: loginDTO.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("StudentDashboard", "StudentDashboard");
                    }


                    ModelState.AddModelError(string.Empty, "Invalid Username or Password");


                    return View("LoginStudent", loginDTO);

                }
                ModelState.AddModelError(string.Empty, "User is not a student");
                return View("LoginStudent", loginDTO);

            }
            ModelState.AddModelError(string.Empty, "User Doesn't Exist ");
            return View("LoginStudent", loginDTO);

        }
        [Route("StudentLogout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("LoginStudent");
        }
        [Route("Student/ChangePassword")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordNow(ChangePasswordVM changePassword)
        {
            if (!ModelState.IsValid)
                return View(changePassword);

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginStudent", "Student");
            }

            var result = await userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("StudentDashboard", "StudentDashboard"); 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("ChangePassword", changePassword);
        }

        [Route("/Student/GetListAAsync")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetListAAsync()
        {

            return View(await fileService.GetFilesAsync());
        }
        [Route("Student/Download")]
        public IActionResult Download(string FilePath)
        {

            return new VirtualFileResult($"Files/{FilePath}", "application/pdf");
            
        }
        [Route("EmailAvailability")]
        public async Task<IActionResult> IsEmailRegisteredAlready(string Email)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                //return new JsonResult(false);
                return Json(false);
            }
            else
            {
                //return new JsonResult(true);
                return Json(true);
            }
        }
        [Authorize(Roles = "Student")]
        [Route("TakeQuiz")]
        public IActionResult TakeQuiz()
        {
            var questions = new List<QuizQuestionVM>
         {
        new QuizQuestionVM
        {
            QuestionId = 1,
            QuestionText = "Which data structure uses LIFO (Last In, First Out)?",
            Options = new List<string> { "Queue", "Stack", "Array", "Linked List" }
        },
        new QuizQuestionVM
        {
            QuestionId = 2,
            QuestionText = "Which language is primarily used for web development?",
            Options = new List<string> { "C", "Python", "JavaScript", "Assembly" }
        },
        new QuizQuestionVM
        {
            QuestionId = 3,
            QuestionText = "Which of these is an operating system?",
            Options = new List<string> { "Oracle", "Linux", "MySQL", "HTML" }
        },
        new QuizQuestionVM
        {
            QuestionId = 4,
            QuestionText = "In databases, SQL stands for?",
            Options = new List<string> { "Structured Query Language", "Simple Query Logic", "Sequential Query Language", "Systematic Query Log" }
        },
        new QuizQuestionVM
        {
            QuestionId = 5,
            QuestionText = "Which protocol is used to access websites?",
            Options = new List<string> { "FTP", "SMTP", "HTTP", "SSH" }
        }
    };

            return View(questions);
        }
        [Authorize(Roles = "Student")]
        [HttpPost("SubmitQuiz")]
        public IActionResult SubmitQuiz(List<QuizQuestionVM> Answers)
        {
            int score = 0;

            var correctAnswers = new Dictionary<int, string>
    {
        { 1, "Stack" },
        { 2, "JavaScript" },
        { 3, "Linux" },
        { 4, "Structured Query Language" },
        { 5, "HTTP" }
    };

            foreach (var ans in Answers)
            {
                if (correctAnswers[ans.QuestionId] == ans.SelectedAnswer)
                {
                    score++;
                }
            }

            ViewBag.Score = score;
            ViewBag.Total = Answers.Count;

            return View("QuizResult");
        }
        [Authorize(Roles = "Student")]
        [Route("QuizResult")]
        public IActionResult QuizResult()
        {
            return View();
        }
        [Route("MyBtachConference")]
        public IActionResult MyBatchConference()
        {
            // ✅ Get the current logged-in user's ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var uid = Guid.Parse(userId);

            // ✅ Fetch the ApplicationUser (only Id + BatchDMId)
            var student = LMSDbContext.Users
                .Where(u => u.Id == uid)
                .Select(u => new { u.Id, u.BatchDMId })
                .FirstOrDefault();

            if (student == null || student.BatchDMId == null)
            {
                return View(new List<VideoConferenceVM>()); // Return empty list if not found
            }

            var batchId = student.BatchDMId;

            // ✅ Fetch all active or upcoming conferences for this batch
            var conferences = LMSDbContext.VideoConference
                .Where(c => c.BatchId == batchId && (c.EndTime == null || c.EndTime >= DateTime.Now))
                .OrderByDescending(c => c.StartTime)
                .Select(c => new VideoConferenceVM
                {
                    Id = c.Id,
                  TeacherId = c.TeacherId,
                    MeetingLink = c.MeetingLink,
                    BatchId = c.BatchId,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime
                })
                .ToList();

            // ✅ Return the list to the view
            return View(conferences);
        }


    }
}
