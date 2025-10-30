using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers.TimeTable
{
    public class TimeTableController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;

        public TimeTableController(UserManager<ApplicationUser> userManager, LMSDbContext lMSDbContext, IMapper mapper)
        {
            this.userManager = userManager;
            this.lMSDbContext = lMSDbContext;
            this.mapper = mapper;
        }
        [Route("CreateTimeTable")]
        // GET: Create new slot for a specific course
        [Authorize(AuthenticationSchemes = "TeacherAuth,AdminAuth", Roles = "Teacher,Admin")]
        public IActionResult Create(Guid courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }
        [Authorize(AuthenticationSchemes ="TeacherAuth,AdminAuth", Roles = "Teacher,Admin")]
        public IActionResult CreateTimeTable(TimeTableVM TimeTable)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate slot (same day, time, and location)
                bool slotExists = lMSDbContext.TimeTables.Any(t =>
                    t.Day == TimeTable.Day &&
                    t.StartTime == TimeTable.StartTime &&
                    t.EndTime == TimeTable.EndTime &&
                    t.LabLocation == TimeTable.LabLocation &&
                    t.CourseId != TimeTable.CourseId); // ensure it belongs to another course
                bool SlotAlreadyAssigned = lMSDbContext.TimeTables.Any(t =>
                    t.Day == TimeTable.Day &&
                    t.StartTime == TimeTable.StartTime &&
                    t.EndTime == TimeTable.EndTime &&
                    t.LabLocation == TimeTable.LabLocation &&
                    t.CourseId == TimeTable.CourseId);
                bool isOverlapping = lMSDbContext.TimeTables.Any(t =>
                    t.Day == TimeTable.Day &&       // same day
                    (
                     (TimeTable.StartTime >= t.StartTime && TimeTable.StartTime < t.EndTime) || // starts inside another slot
                     (TimeTable.EndTime > t.StartTime && TimeTable.EndTime <= t.EndTime) ||     // ends inside another slot
                     (TimeTable.StartTime <= t.StartTime && TimeTable.EndTime >= t.EndTime)     // completely covers another slot
                    )
                );
                var duration = TimeTable.EndTime - TimeTable.StartTime;

                if (slotExists)
                {
                    ModelState.AddModelError("", "This time slot and location are already assigned to another course.");
                    ViewBag.CourseId = TimeTable.CourseId;
                    return View("Create",TimeTable);
                }
                else if(SlotAlreadyAssigned)
                {
                    ModelState.AddModelError("", "This time slot and location are already assigned to this course.");
                    ViewBag.CourseId = TimeTable.CourseId;
                    return View("Create",TimeTable);
                }
                else if (isOverlapping)
                {
                    ModelState.AddModelError("", "This time slot overlaps with an existing slot for this course.");
                    ViewBag.CourseId = TimeTable.CourseId;
                    return View("Create",TimeTable);
                }
                else if (duration != TimeSpan.FromHours(1))
                {
                    ModelState.AddModelError("", "Slot must be exactly 1 hour long.");
                    return View("Create",TimeTable);
                }
                var TimeTables = mapper.Map<TimeTableDM>(TimeTable);
                lMSDbContext.TimeTables.Add(TimeTables);
                lMSDbContext.SaveChanges();
                if (User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"))
                {
                    return RedirectToAction("AdminGetDetails", "Course", new { id = TimeTable.CourseId });
                }

                return RedirectToAction("Details", "Course", new { id = TimeTable.CourseId });
            }

            ViewBag.CourseId = TimeTable.CourseId;
            return View("Create",TimeTable);
        }
        [Authorize(AuthenticationSchemes = "TeacherAuth,AdminAuth", Roles = "Teacher,Admin")]
        public IActionResult ManageTimeTable(Guid courseId)
        {
            var course = lMSDbContext.Courses
                .Include(c => c.TimeTables)
                .Include(c => c.Teacher)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            var courseVM = mapper.Map<CourseVM>(course);
            return View(courseVM);
        }
        [Route("DeletSlot")]
        public IActionResult DeleteSlot(Guid id)
        {
            var timeTable = lMSDbContext.TimeTables.Find(id);
            if (timeTable == null)
                return NotFound();
            lMSDbContext.TimeTables.Remove(timeTable);
            lMSDbContext.SaveChanges();
            return RedirectToAction("ManageTimeTable", new { courseId = timeTable.CourseId });
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public async Task<IActionResult> ViewTimeTable()
        {
            // Get currently logged-in student
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Student");

            // Get all approved enrollments of this student
            var enrollments = await lMSDbContext.StudentCourses
                .Where(e => e.StudentId == user.Id && e.IsApproved)
                .Include(e => e.Course)
                    .ThenInclude(c=>c.Teacher)
                .Include(e=>e.Course)
                    .ThenInclude(t => t.TimeTables) // include timetable
                .ToListAsync();

            // Prepare model for the view
            var model = new List<CourseVM>();

            foreach (var enrollment in enrollments)
            {
                var course = enrollment.Course;

                // Get teacher name manually because TeacherId (Guid) != IdentityUser.Id (string)
                string teacherName = await lMSDbContext.Users
                    .Where(u => u.Id == course.TeacherId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync() ?? "N/A";

                model.Add(new CourseVM
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    TeacherId = course.TeacherId,
                    TeacherName = teacherName,
                    TimeTables = course.TimeTables.ToList()
                });
            }

            return View(model);
        }

        [Authorize(AuthenticationSchemes = "TeacherAuth,AdminAuth", Roles = "Teacher,Admin")]

        public async Task<IActionResult> GeneralTimeTable()
        {
            var timeTables = await lMSDbContext.TimeTables
                .Include(t => t.Course)
                    .Include(c=>c.Course.Teacher)    
                //.ThenInclude(c => c.Teacher)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
            var timeTableVMs = mapper.Map<List<TimeTableVM>>(timeTables);
            

            return View(timeTableVMs);
        }


    }
}
