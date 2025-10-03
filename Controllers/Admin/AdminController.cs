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
           
            if(!ModelState.IsValid)
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
            //    PhoneNumber=registerDTO.Phone,
            //    UserName=registerDTO.Email
            
            //};
           IdentityResult result=await _userManager.CreateAsync(user,registerDTO.Password);
            if(result.Succeeded)
            {
                if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                {
                
                    ApplicationRole applicationRole = new ApplicationRole()
                    {
                        Name = UserTypeOptions.Admin.ToString()
                    };
                    await _roleManager.CreateAsync(applicationRole);
                }
                await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());

                await emailService.SendMail(registerDTO.Email);
                return RedirectToAction("Login");
        
            }

           
                foreach(IdentityError error in result.Errors)
                {
                ModelState.AddModelError(string.Empty, error.Description);

            }

            return View("Create", registerDTO);
            
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


    }
}
