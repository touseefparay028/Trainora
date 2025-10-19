using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers.TimeTable
{
    public class TimeTableController : Controller
    {
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;

        public TimeTableController(LMSDbContext lMSDbContext, IMapper mapper)
        {
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

                if (slotExists)
                {
                    ModelState.AddModelError("", "This time slot and location are already assigned to another course.");
                    ViewBag.CourseId = TimeTable.CourseId;
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
    }
}
