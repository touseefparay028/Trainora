using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers.Attendance
{
    public class AttendanceController : Controller
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;

        public AttendanceController(UserManager<ApplicationUser> userManager, LMSDbContext lMSDbContext,IMapper mapper)
        {
            this.userManager = userManager;
            this.lMSDbContext = lMSDbContext;
            this.mapper = mapper;
        }
        //  MARK ATTENDANCE (Auto or Manual)
        [HttpPost]
        public async Task<IActionResult> MarkAttendance(Guid courseId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var timetable = await lMSDbContext.TimeTables
                .FirstOrDefaultAsync(t => t.CourseId == courseId);

            if (timetable == null)
                return NotFound("No timetable found for this course.");

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            // Check if time slot matches
            if (currentTime < timetable.StartTime || currentTime > timetable.EndTime)
                return BadRequest("Not within the course time slot.");

            var attendance = new AttendanceDM
            {
                Id = Guid.NewGuid(),
                StudentId = user.Id,
                BatchDMId = user.BatchDMId ?? Guid.Empty,
                CourseId = courseId,
                Date = DateTime.Today,
                JoinTime = currentTime,
                IsPresent = true
            };

            lMSDbContext.Attendances.Add(attendance);
            await lMSDbContext.SaveChangesAsync();
            return Ok("Attendance marked successfully!");
        }
        // 🧩 TEACHER — SELECT STUDENT
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetStudentsByCourse(Guid courseId)
        {
            // Fetch course with enrolled students
            var course = await lMSDbContext.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student) // Assuming StudentCourseDM.Student is ApplicationUser
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound("Course not found.");

            // Map enrolled students
            var Students = mapper.Map<List<RegisterDTO>>(
            course.Enrollments
            .Where(e => e.IsApproved)   // only approved enrollments
            .Select(e => e.Student)
            .ToList()
);

            //var students = course.Enrollments
            //    .Where(e => e.Student != null)
            //    .Select(e => new RegisterDTO
            //    {
            //        Id = e.Student.Id,
            //        Name = e.Student.Name,
            //        Email = e.Student.Email
            //    })
            //    .ToList();


            // Get all batches
            var batches = await lMSDbContext.BatchDMs.ToListAsync();

            // Group students by batch
            var batchStudents = batches.Select(batch => new BatchVM
            {
                Name = batch.Name,
                ApplicationUser = Students
                            .Where(u => u.BatchDMId == batch.id) // link students to batch
                            .Select(u => new ApplicationUser
                            {
                                Id = u.Id,
                                Name = u.Name,
                                Email = u.Email,
                                EnrollmentNumber = u.EnrollmentNumber,
                                BatchDMId = batch.id
                            })
                            .ToList()
            }).ToList();
            //// Create ViewModel
            //var StudentVM = new CourseVM
            //{
            //    Id = course.Id,
            //    Title = course.Titleb,
            //    TeacherName = course.Teacher?.Name,
            //    Students = Students
            //};
            ViewBag.CourseId = courseId;
            return View(batchStudents);
        }



        ////  TEACHER — VIEW STUDENT COURSES
        ////[Authorize(Roles = "Teacher")]
        //public async Task<IActionResult> ViewStudentCourses(string studentId)
        //{
        //    var student = await userManager.FindByIdAsync(studentId);
        //    if (student == null) return NotFound();

        //    //var courses = await lMSDbContext.Courses
        //    //    .Where(c => c.Batchid == student.BatchDMId)
        //    //    .ToListAsync();

        //    ViewBag.StudentName = student.Name;
        //    ViewBag.StudentId = student.Id;
        //    return View();
        //}

        //  TEACHER — VIEW COURSE ATTENDANCE
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ViewCourseAttendance(Guid studentId, Guid courseId)
        {
            
            var student = await userManager.FindByIdAsync(studentId.ToString());
            if (student == null) return NotFound();

            var course = await lMSDbContext.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var attendanceRecords = await lMSDbContext.Attendances
                .Where(a => a.StudentId == studentId && a.CourseId == courseId)
                .ToListAsync();

            var summary = new StudentAttendanceSummaryVM
            {
                StudentId = student.Id.ToString(),
                StudentName = student.Name,
                CourseId = course.Id,
                CourseName = course.Title,
                TotalClasses = attendanceRecords.Count,
                ClassesAttended = attendanceRecords.Count(a => a.IsPresent),
                AttendanceRecords = attendanceRecords.Select(a => new AttendanceDetailVM
                {
                    Date = a.Date,
                    IsPresent = a.IsPresent,
                    Remark = a.Remark
                }).ToList()
            };

            return View(summary);
        }

        //  STUDENT — VIEW OWN ATTENDANCE SUMMARY
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentAttendanceSummary()
        {
            var user = await userManager.GetUserAsync(User);

            var attendances = await lMSDbContext.Attendances
                .Include(a => a.Course)
                .Where(a => a.StudentId == user.Id)
                .ToListAsync();

            var grouped = attendances
                .GroupBy(a => a.Course)
                .Select(g => new StudentAttendanceSummaryVM
                {
                    StudentId = user.Id.ToString(),
                    StudentName = user.Name,
                    CourseId = g.Key.Id,
                    CourseName = g.Key.Title,
                    TotalClasses = g.Count(),
                    ClassesAttended = g.Count(x => x.IsPresent)
                }).ToList();

            return View(grouped);
        }

        // 🧩 EDIT ATTENDANCE (Optional)
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditAttendance(Guid id)
        {
            var attendance = await lMSDbContext.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();
            return View(attendance);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditAttendance(AttendanceDM model)
        {
            lMSDbContext.Attendances.Update(model);
            await lMSDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(GetStudentsByCourse));
        }

    }
}
