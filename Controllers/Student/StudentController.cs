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
                if (await roleManager.FindByNameAsync(UserTypeOptions.Student.ToString()) is null)
                {

                    ApplicationRole applicationRole = new ApplicationRole()
                    {
                        Name = UserTypeOptions.Student.ToString()
                    };
                    await roleManager.CreateAsync(applicationRole);
                }
                await userManager.AddToRoleAsync(user, UserTypeOptions.Student.ToString());

                await emailService.SendMail(registerDTO.Email);
                return RedirectToAction("LoginStudent");

            }


            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);

            }
            return View("Create", registerDTO);

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
