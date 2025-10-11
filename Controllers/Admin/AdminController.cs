using AutoMapper;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.RoleEnums;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LearningManagementSystem.Controllers.Account
{
    //[AllowAnonymous]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;

        public AdminController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IMapper Mapper, IEmailService emailService )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            mapper = Mapper;
            this.emailService = emailService;
        }

        [Route("AdminRegister")]
        public IActionResult Create()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }
            return View();
        }
        [HttpPost("PostCreate")]
        public async  Task<IActionResult> CreateUser(RegisterDTO registerDTO)
        {
            ModelState.Remove("Address");
            ModelState.Remove("DateOfBirth");
            ModelState.Remove("EnrollmentNumber");
            ModelState.Remove("Course");
            ModelState.Remove("Gender");
            if (!ModelState.IsValid)
            {
              ViewBag.Error=  ModelState.Values.SelectMany(x => x.Errors).Select(y=>y.ErrorMessage);
                return View("Create",registerDTO);
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
           IdentityResult result=await _userManager.CreateAsync(user,registerDTO.Password);
            if(result.Succeeded)
            {
                // Generate confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string Name = registerDTO.Name;
                // Build confirmation link
                var confirmationLink = Url.Action(nameof(AdminConfirmEmail), "Admin",
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
            <p>Thank you for registering as a Admin on Trainora.</p>
            <p>To complete your registration and access your account, please verify your email address by clicking the button below:</p>
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='btn'>Verify Email</a>
            </p>
            <p>If the button above does not work, copy and paste the following link into your browser:</p>
            <p style='word-break: break-all; color: #4CAF50;'>{confirmationLink}</p>
            <p>Once verified, you can log in and begin managing users, courses, and system settings on Trainora</p>
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
                if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                {
                
                    ApplicationRole applicationRole = new ApplicationRole()
                    {
                        Name = UserTypeOptions.Admin.ToString()
                    };
                    await _roleManager.CreateAsync(applicationRole);
                }
                await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());

             
                return RedirectToAction("AdminRegistrationSuccessful");
        
            }

           
                foreach(IdentityError error in result.Errors)
                {
                ModelState.AddModelError(string.Empty, error.Description);

            }

            return View("Create", registerDTO);
            
        }
        public IActionResult AdminRegistrationSuccessful()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AdminConfirmEmail(string userId, string token)
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
                string subject = "Admin Verified Successfully";

                string body = $"Hello {Name},\n\n" +
              "Your account has been successfully verified as an administrator.\n\n" +
              "You can now access your admin dashboard, manage users, oversee courses, and monitor platform activities efficiently. " +
              "As a verified administrator, you play a vital role in ensuring the smooth functioning of Trainora and maintaining a productive learning environment.  " +
              "We encourage you to utilize your tools responsibly, support instructors and students, and contribute to continuous academic excellence.\n\n" +
              "If you have any questions or face any issues, feel free to reach out to our support team at support@Trainora.com for assistance.\n\n" +
              "Best regards,\n" +
              "Team Train𝓞ra";
                await emailService.SendMail(Email, subject, body);
                return View("AdminConfirmEmailSuccess");
            }


            return View("Error");
        }
        public IActionResult AdminConfirmEmailSuccess()
        {
            return View();
        }


        [Route("Account/Login")]
        //[Authorize(Roles ="Admin", Policy = "NotAuthenticated")]
        public IActionResult Login()
        {
            if(User.Identity!=null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        [Route("AdminLogin")]
        public async Task<IActionResult> LoginUser(LoginDTO loginDTO,string? ReturnUrl=null)
        {


            if (!ModelState.IsValid)
            {
                ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage);
                return View("Login",loginDTO);
            }

            
            ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if(user!=null)
            {
                // ✅ Check if email is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty,
                        "Your email is not veirified yet. Please check your email inbox to verify your account.");
                    return View("Login", loginDTO);
                }
                if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, isPersistent:loginDTO.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("Dashboard", "Dashboard");
                    }
                   

                    ModelState.AddModelError(string.Empty, "Invalid Username or Password");


                    return View("Login", loginDTO);

                }
                ModelState.AddModelError(string.Empty, "User is not an admin");
                return View("Login",loginDTO);

            }
            ModelState.AddModelError(string.Empty, "User Doesn't Exist ");
            return View("Login", loginDTO);

        }
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        [Route("isitavailable")]
        public async Task<IActionResult> IsEmailRegisteredAlready(string Email)
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
        public async Task<IActionResult> GetTeachers()
        {
            var users = await _userManager.GetUsersInRoleAsync("Teacher");

            var teacherList = users.Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.UserName
            }).ToList();

            return View(teacherList);
        }


    }
}
