using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Create(Guid courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }
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
                if(User.IsInRole("Admin"))
                {
                    return RedirectToAction("AdminGetDetails","Course", new {id = TimeTable.CourseId });
                }
                return RedirectToAction("Details", "Course", new { id = TimeTable.CourseId });
            }

            ViewBag.CourseId = TimeTable.CourseId;
            return View("Create",TimeTable);
        }
        
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
        public async Task<IActionResult> ViewTimeTable()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Get all approved enrollments of the student
            var enrollments = await lMSDbContext.StudentCourses
                .Where(e => e.StudentId == user.Id && e.IsApproved)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher) // Include teacher details
                .Include(e => e.Course)
                    .ThenInclude(c => c.TimeTables) // Include timetable
                .ToListAsync();

            // Prepare dynamic model for the view
            var model = enrollments.Select(e => new
            {
                e.Course.Title,
                e.Course.Description,
                TeacherName = e.Course.Teacher.Name,
                TimeTables = e.Course.TimeTables.Select(t => new
                {
                    t.Day,
                    t.StartTime,
                    t.EndTime,
                    t.LabLocation
                }).ToList()
            }).ToList();

            return View(model);
        }
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GeneralTimeTable()
        {
            var timeTables = await lMSDbContext.TimeTables
                .Include(t => t.Course)
                    .ThenInclude(c => c.Teacher)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
            var timeTableVMs = mapper.Map<List<TimeTableVM>>(timeTables);

            return View(timeTableVMs);
        }


    }
}
