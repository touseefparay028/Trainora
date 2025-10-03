using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.RoleEnums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LearningManagementSystem.Controllers.Account
{
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;

        public TeacherController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<ApplicationRole> roleManager, 
            LMSDbContext lMSDbContext, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            this.lMSDbContext = lMSDbContext;
            this.mapper = mapper;
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
                if (await _roleManager.FindByNameAsync(UserTypeOptions.Teacher.ToString()) is null)
                {

                    ApplicationRole applicationRole = new ApplicationRole()
                    {
                        Name = UserTypeOptions.Teacher.ToString()
                    };
                    await _roleManager.CreateAsync(applicationRole);
                }
                await _userManager.AddToRoleAsync(user, UserTypeOptions.Teacher.ToString());
               //create teacher entry at the same time
                //var teacher = new TeacherDM
                //{
                //    ApplicationUserId = user.Id  // 👈 link ApplicationUser to TeacherDM
                //};

                //lMSDbContext.TeacherDMs.Add(teacher);
                //await lMSDbContext.SaveChangesAsync();

                return RedirectToAction("LoginTeacher");

            }


            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }

            return View("Create",registerDTO);

        }

        [Route("LoginTeacher")]
        public IActionResult LoginTeacher()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("TeacherDashboard", "TeacherDashboard");
            }

            return View();
        }

        [HttpPost("PostTeacherLogin")]
        public async Task<IActionResult> Login(LoginDTO loginDTO,string? ReturnUrl=null)
        {


            if (!ModelState.IsValid)
            {
                ViewBag.Error = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage);
                return View("LoginTeacher", loginDTO);
            }


            ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Teacher.ToString()))
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, isPersistent:loginDTO.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("TeacherDashboard", "TeacherDashboard");
                    }


                    ModelState.AddModelError(string.Empty, "Invalid Username or Password");
                     return View("LoginTeacher", loginDTO);

                }
                ModelState.AddModelError(string.Empty,"User Is Not A Teacher");
                return View("LoginTeacher", loginDTO);

            }
            ModelState.AddModelError(string.Empty, "Teacher Doesn't Exist ");
            return View("LoginTeacher", loginDTO);

        }
        [Route("LogoutTeacher")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("LoginTeacher");
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
        public IActionResult StartConference(Guid batchId)
        {
            var meetingLink = $"https://meet.jit.si/{Guid.NewGuid()}"; // Unique meeting link
            var conference = new VideoConference
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                TeacherId = new Guid(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value),
                StartTime = DateTime.Now,
                MeetingLink = meetingLink
            };

            lMSDbContext.VideoConference.Add(conference);
            lMSDbContext.SaveChanges();

            // Redirect teacher to the conference page
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
        public IActionResult GetConferences()
        {
            // Get the logged-in teacher's ID
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            // Fetch conferences created by this teacher
            var conferences = lMSDbContext.VideoConference
                .Where(c => c.TeacherId == userId)
                .OrderByDescending(c => c.StartTime)
                .Select(c => new VideoConferenceVM
                {
                    Id = c.Id,
                    BatchId = c.BatchId,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    MeetingLink = c.MeetingLink
                })
                .ToList();

            return View(conferences);

        }


    }
}
