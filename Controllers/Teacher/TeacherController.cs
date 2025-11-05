using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.RoleEnums;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace LearningManagementSystem.Controllers.Account
{
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;

        public TeacherController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<ApplicationRole> roleManager, 
            LMSDbContext lMSDbContext, IMapper mapper,IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            this.lMSDbContext = lMSDbContext;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        [Route("TeacherRegister")]
        public IActionResult Create()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("TeacherDashboard", "TeacherDashboard");
            }
            return View();
        }

        [HttpPost("PostRegister")]
        public async Task<IActionResult> CreateTeacher(RegisterDTO registerDTO)
        {
            ModelState.Remove("Address");
            ModelState.Remove("DateOfBirth");
            ModelState.Remove("EnrollmentNumber");
            ModelState.Remove("Course");
            ModelState.Remove("Gender");
            ModelState.Remove("BatchDMId");
            if (!ModelState.IsValid)
            {
                ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage);
                return View("Create",registerDTO);
            }
            ApplicationUser user = mapper.Map<ApplicationUser>(registerDTO);
            //ApplicationUser user = new ApplicationUser()
            //{
            //    Name = registerDTO.Name,
            user.Email = registerDTO.Email;
            user.PhoneNumber = registerDTO.Phone;
            user.UserName = registerDTO.Email;

            //};
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                // Generate confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string Name = registerDTO.Name;
                // Build confirmation link
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Teacher",
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
            Trainora
        </div>
        <div class='body'>
            <h2>Hello {Name},</h2>
            <p>Thank you for registering as a Teacher on Trainora.</p>
            <p>To complete your registration and access your account, please verify your email address by clicking the button below:</p>
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='btn'>Verify Email</a>
            </p>
            <p>If the button above does not work, copy and paste the following link into your browser:</p>
            <p style='word-break: break-all; color: #4CAF50;'>{confirmationLink}</p>
            <p>Once verified, you can log in immediately.</p>
            <p>Welcome aboard!</p>
            <p>Best regards,<br/>Trainora</p>
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

                if (await _roleManager.FindByNameAsync(UserTypeOptions.Teacher.ToString()) is null)
                {
                   
                    ApplicationRole applicationRole = new ApplicationRole()
                    {
                        Name = UserTypeOptions.Teacher.ToString()
                    };
                    await _roleManager.CreateAsync(applicationRole);
                }
                await _userManager.AddToRoleAsync(user, UserTypeOptions.Teacher.ToString());

                return RedirectToAction("RegistrationSuccessful");

            }


            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }

            return View("Create",registerDTO);

        }
        public IActionResult RegistrationSuccessful()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return View("Error");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return View("Error");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                string Name = user.Name;
                string Email = user.Email;
                string subject = "Teacher Account Verification Successful";

                string body = $"Hello {Name},\n\n" +
                              "Your account has been successfully verified as a teacher.\n\n" +
                              "You can now access your teaching dashboard, create and manage courses, upload study materials, and interact with your students effectively. " +
                              "As a verified teacher, you are an essential part of our academic network, contributing to the growth and learning of our students. " +
                              "We encourage you to explore the available tools and resources to deliver engaging and impactful learning experiences.\n\n" +
                              "If you encounter any issues or need assistance, please contact our support team at support@Trainora.com.\n\n" +
                              "Best regards,\n" +
                              "Team Train𝓞ra";

                await emailService.SendMail(Email, subject, body);
                return View("ConfirmEmailSuccess");
            }
                

            return View("Error");
        }
        public IActionResult ConfirmEmailSuccess()
        { 
            return View();
        }

        //[Route("LoginTeacher")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginTeacher()
        {
            // Force ASP.NET to check all authentication schemes
            var adminAuth = await HttpContext.AuthenticateAsync("AdminAuth");
            var teacherAuth = await HttpContext.AuthenticateAsync("TeacherAuth");
            var studentAuth = await HttpContext.AuthenticateAsync("StudentAuth");

            // Block access if logged in as Admin or Student
            if ((adminAuth.Succeeded && adminAuth.Principal != null) ||
                (studentAuth.Succeeded && studentAuth.Principal != null))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            //  If already logged in as Teacher, go to Dashboard
            if (teacherAuth.Succeeded && teacherAuth.Principal != null)
            {
                var principal = teacherAuth.Principal;

                // Double-check role from claims to be safe
                if (principal.IsInRole("Teacher"))
                {
                    return RedirectToAction("TeacherDashboard", "TeacherDashboard");
                }

                return RedirectToAction("AccessDenied", "Home");
            }

            // Not authenticated — show login form
            return View();
        }

        [AllowAnonymous]
        [HttpPost("PostTeacherLogin")]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl = null)
        {
            if (!ModelState.IsValid)
                return View("LoginTeacher", loginDTO);

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Teacher doesn't exist");
                return View("LoginTeacher", loginDTO);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Email not verified");
                return View("LoginTeacher", loginDTO);
            }

            if (!await _userManager.IsInRoleAsync(user, UserTypeOptions.Teacher.ToString()))
            {
                ModelState.AddModelError(string.Empty, "User is not a teacher");
                return View("LoginTeacher", loginDTO);
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid password");
                return View("LoginTeacher", loginDTO);
            }

            // ✅ Sign in with TeacherAuth scheme
            var claims = new List<Claim>
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Role, "Teacher")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "TeacherAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("TeacherAuth", claimsPrincipal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = loginDTO.RememberMe,
                ExpiresUtc = DateTime.UtcNow.AddHours(4)
            });

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);

            return RedirectToAction("TeacherDashboard", "TeacherDashboard");
        }

        //public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl = null)
        //{


        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage);
        //        return View("LoginTeacher", loginDTO);
        //    }


        //    ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
        //    if (user != null)
        //    {
        //        // ✅ Check if email is confirmed
        //        if (!await _userManager.IsEmailConfirmedAsync(user))
        //        {
        //            ModelState.AddModelError(string.Empty,
        //                "Your email is not veirified yet. Please check your email inbox to verify your account.");
        //            return View("LoginTeacher", loginDTO);
        //        }
        //        if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Teacher.ToString()))
        //        {
        //            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, isPersistent: loginDTO.RememberMe, false);

        //            if (result.Succeeded)
        //            {


        //                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        //                {
        //                    return Redirect(ReturnUrl);
        //                }
        //                return RedirectToAction("TeacherDashboard", "TeacherDashboard");
        //            }


        //            ModelState.AddModelError(string.Empty, "Invalid Username or Password");
        //            return View("LoginTeacher", loginDTO);

        //        }
        //        ModelState.AddModelError(string.Empty, "User Is Not A Teacher");
        //        return View("LoginTeacher", loginDTO);

        //    }
        //    ModelState.AddModelError(string.Empty, "Teacher Doesn't Exist ");
        //    return View("LoginTeacher", loginDTO);

        //}

        [Route("LogoutTeacher")]
        [Authorize(AuthenticationSchemes ="TeacherAuth")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("TeacherAuth");
            return RedirectToAction("LoginTeacher", "Teacher");
        }
        [HttpGet]
        [Route("ChangePassword")]
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginTeacher", "Teacher");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("ChangePassword", "Teacher"); // or wherever you want
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("ChangePassword",changePassword);
        }

        public async Task<IActionResult> IsEmailRegisteredAlready(String Email)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(Email);
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
        [Route("StartConference")]
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        public async Task<IActionResult> StartConference(Guid batchId, Guid CourseId)
        {
            var now = DateTime.Now; // Local time
            var today = now.Date;

            // 1️⃣ Check if the class is scheduled now
            var timetable = await lMSDbContext.TimeTables
                .Where(t => t.CourseId == CourseId && t.Day == now.DayOfWeek.ToString())
                .ToListAsync();

            if (timetable == null)
            {
                TempData["Message"] = "No class is scheduled for today.";
                return RedirectToAction("GetCourses", "Course");
            }
            // Find if any timetable matches the current time
            var currentTimetable = timetable
                .FirstOrDefault(t => now.TimeOfDay >= t.StartTime && now.TimeOfDay <= t.EndTime);

            if (currentTimetable == null)
            {
                TempData["Message"] = "No class is scheduled right now.";
                return RedirectToAction("GetCourses", "Course");
            }
            //// Compare time range (assuming timetable has StartTime and EndTime columns)
            //var startTime = timetable.StartTime; // e.g., 10:00 AM
            //var endTime = timetable.EndTime;     // e.g., 11:00 AM

            //if (now.TimeOfDay < startTime || now.TimeOfDay > endTime)
            //{
            //    TempData["Message"] = "No class is scheduled right now.";
            //    return RedirectToAction("GetCourses","Course");
            //}

            // 2️⃣ Continue with conference creation
            var meetingLink = $"https://meet.jit.si/{Guid.NewGuid()}";
            var teacherIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (teacherIdClaim == null)
            {
                return Unauthorized("User identity claim not found. Please log in again.");
            }

            var teacherId = new Guid(teacherIdClaim.Value);
            var conference = new VideoConference
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                CourseId = CourseId,
                TeacherId = new Guid(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value),
                StartTime = now,
                MeetingLink = meetingLink
            };

            lMSDbContext.VideoConference.Add(conference);
            await lMSDbContext.SaveChangesAsync();

            // Create class session if not already created for today
            var existingSession = await lMSDbContext.ClassSessions
                .FirstOrDefaultAsync(s => s.CourseId == CourseId && s.SessionDate.Date == today);

            if (existingSession == null)
            {
                existingSession = new ClassSessions
                {
                    Id = Guid.NewGuid(),
                    CourseId = CourseId,
                    SessionDate = today,
                    SessionTime = now.TimeOfDay
                };
                lMSDbContext.ClassSessions.Add(existingSession);

                var enrolledStudents = await lMSDbContext.StudentCourses
                    .Where(sc => sc.CourseId == CourseId && sc.IsApproved)
                    .ToListAsync();

                foreach (var sc in enrolledStudents)
                {
                    var studentId = sc.StudentId;
                    var exists = await lMSDbContext.Attendances.AnyAsync(a =>
                        a.CourseId == CourseId &&
                        a.StudentId == studentId &&
                        a.Date.Date == today);

                    if (!exists)
                    {
                        lMSDbContext.Attendances.Add(new AttendanceDM
                        {
                            Id = Guid.NewGuid(),
                            CourseId = CourseId,
                            StudentId = studentId,
                            BatchDMId = batchId,
                            Date = today,
                            IsPresent = false,
                            JoinTime = null,
                            Remark = null
                        });
                    }
                }

                await lMSDbContext.SaveChangesAsync();
            }

            // 4️⃣ Redirect teacher to the conference page
            return RedirectToAction("ConferenceRoom", new { id = conference.Id });
        }

        //Page that embeds the video conference
        [Route("ConferenceRoom")]
        public IActionResult ConferenceRoom(Guid id)
        {
            var conference = lMSDbContext.VideoConference.FirstOrDefault(c => c.Id == id);
            if (conference == null) return NotFound();

            return View(conference); // Pass conference to view
        }
        [Route("Conferencess")]
        [Authorize(AuthenticationSchemes ="TeacherAuth",Roles ="Teacher")]
        public IActionResult GetConferences()
        {
            // Get the logged-in teacher's ID
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            // Fetch conferences created by this teacher
            var conferences = lMSDbContext.VideoConference
                .Where(c => c.TeacherId == userId)
                .Include(c=> c.batch)
                .OrderByDescending(c => c.StartTime)
                .Select(c => new VideoConferenceVM
                {
                    Id = c.Id,
                    BatchId = c.BatchId,
                    BatchName = c.batch.Name,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    MeetingLink = c.MeetingLink
                })
                .ToList();
            

            return View(conferences);

        }
        [HttpGet]
        [Route("EndConference")]
        public IActionResult DeleteConference(Guid Id)
        {
            var conference = lMSDbContext.VideoConference.FirstOrDefault(c => c.Id == Id);
            if (conference == null) return
                    NotFound();
            lMSDbContext.VideoConference.Remove(conference);
            lMSDbContext.SaveChanges();
            return RedirectToAction("GetConferences");
        }


    }
}
