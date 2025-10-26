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
        [HttpGet]
        public async Task<IActionResult> MarkAttendance(Guid ConferenceId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            // Find the conference
            var conference = await lMSDbContext.VideoConference.FindAsync(ConferenceId);
            if (conference == null)
                return NotFound();

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek.ToString();
            // Get all timetables for this course (and optionally this batch)
            var timetables = await lMSDbContext.TimeTables
                .Where(t => t.CourseId == conference.CourseId && t.Day == currentDay)
                .ToListAsync();
            if (timetables == null || !timetables.Any())
            {
                TempData["Message"] = "No timetable found for this course today.";
                return RedirectToAction("MyBatchConference", "Student");
            }

            // Check if any timetable matches the current time
            var validSlot = timetables.Any(t => currentTime >= t.StartTime && currentTime <= t.EndTime);
            if (!validSlot)
                return Redirect(conference.MeetingLink); // Allow joining even if not in time slot

            // Check if already marked to prevent duplicates
            var alreadyMarked = await lMSDbContext.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == user.Id &&
                               a.CourseId == conference.CourseId &&
                               a.Date == DateTime.Today);
            if (alreadyMarked ==null)
            {
                var attendance = new AttendanceDM
                {
                    Id = Guid.NewGuid(),
                    StudentId = user.Id,
                    BatchDMId = user.BatchDMId ?? Guid.Empty,
                    CourseId = conference.CourseId,
                    Date = DateTime.Today,
                    JoinTime = currentTime,
                    IsPresent = true
                };

                lMSDbContext.Attendances.Add(attendance);
                await lMSDbContext.SaveChangesAsync();
                return Redirect(conference.MeetingLink);
            }// If already present, optionally update join time only if null or earlier than existing

            else
            {
               if (!alreadyMarked.IsPresent || alreadyMarked.JoinTime == null)
            {
                    alreadyMarked.IsPresent = true;
                    alreadyMarked.JoinTime = now.TimeOfDay;
                    lMSDbContext.Attendances.Update(alreadyMarked);
                    await lMSDbContext.SaveChangesAsync();
                }
            }

            return Redirect(conference.MeetingLink);
          
            //// Check if time slot matches
            //if (currentTime < timetable.StartTime || currentTime > timetables.EndTime)
            //    return BadRequest("Not within the course time slot.");

           
        }
        // 🧩 TEACHER — SELECT STUDENT
        [Authorize(Roles = "Teacher")]
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
                StudentName = student.Name?? "N/A",
                CourseId = course.Id,
                CourseName = course.Title ?? "N/A",
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
                    id=Guid.NewGuid(),
                    StudentId = user.Id.ToString(),
                    StudentName = user.Name,
                    CourseId = g.Key.Id,
                    CourseName = g.Key.Title,
                    TotalClasses = g.Count(),
                    ClassesAttended = g.Count(x => x.IsPresent)
                }).ToList();
           

            return View(grouped);
        }
        [Authorize(Roles ="Student")]
        public async Task<IActionResult> ViewStudentCourseAttendance(Guid courseid)
        {
            var user =await userManager.GetUserAsync(User);
            if(user == null)
                return NotFound("User not found");
            var studentId=user.Id;
            

            var course = await lMSDbContext.Courses.FindAsync(courseid);
            if (course == null) return NotFound();

            var attendanceRecords = await lMSDbContext.Attendances
                .Where(a => a.StudentId ==studentId && a.CourseId == courseid)
                .ToListAsync();

            var summary = new StudentAttendanceSummaryVM
            {
                StudentId = studentId.ToString(),
                StudentName = user.Name ?? "N/A",
                CourseId = course.Id,
                CourseName = course.Title ?? "N/A",
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
