using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly LMSDbContext lMSDbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly IFileService fileService;

        public CourseController(LMSDbContext lMSDbContext, UserManager<ApplicationUser> userManager, IMapper mapper,IFileService fileService)
        {
            this.lMSDbContext = lMSDbContext;
            this.userManager = userManager;
            this.mapper = mapper;
            this.fileService = fileService;
        }
        [Route("GetCourses")]
        [Authorize(Roles = "Teacher")]
        // GET: Courses (List of courses)
        public async Task<IActionResult> GetCourses()
        {
           
            var courses = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c=>c.Batch)
                .Include(c => c.TimeTables)
                .Include(c => c.Enrollments)
                .ToListAsync();
            //var batches = lMSDbContext.Courses.ToListAsync();
            var batches = await lMSDbContext.BatchDMs.ToListAsync();

            var model = new VideoConference
            {
                BatchList = batches
            };
            var CourseVM = mapper.Map<List<CourseVM>>(courses);
           
            
            
            CourseVM.ForEach(c =>
             c.EnrolledStudentsCount = c.Enrollments != null
         ? c.Enrollments.Count(e => e.IsApproved)
         : 0
 );
            return View(CourseVM);
        }
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminGetCourses()
        {
            var courses = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.TimeTables)
                .Include(c=> c.Enrollments)
                .ToListAsync();
            var CourseVM = mapper.Map<List<CourseVM>>(courses);
            CourseVM.ForEach(c =>
      c.EnrolledStudentsCount = c.Enrollments != null
          ? c.Enrollments.Count(e => e.IsApproved)
          : 0
  );

            return View(CourseVM);

        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var course = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c=>c.Batch)
                .Include(c => c.TimeTables)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }
            var courseVM = mapper.Map<CourseVM>(course);
            return View(courseVM);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminGetDetails(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var course = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.TimeTables)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }
            var courseVM = mapper.Map<CourseVM>(course);
            return View(courseVM);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create()
        {
            var model = new CourseVM
            {
                BatchList = await fileService.GetBatchSelectListAsync()
            };
            return View(model);
        }
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreateCourse()
        {
            // Get all users with the "Teacher" role
            var teachers = await userManager.GetUsersInRoleAsync("Teacher");
            var courseVM = new CourseVM
            {
                BatchList = await fileService.GetBatchSelectListAsync(),
                TeacherList = teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),      // This will be the selected value (Teacher Id)
                    Text = t.Name ?? t.UserName  // Display name in dropdown
                }).ToList()
            };

            return View(courseVM);
        }
        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(CourseVM course)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                var courseDM = new CourseDM
                {
                    Id = course.Id != Guid.Empty ? course.Id : Guid.NewGuid(),
                    Title = course.Title,
                    Description = course.Description,
                    TeacherId = user.Id,    // ✅ only assign TeacherId
                    BatchId = course.BatchId,        // optional
                    Enrollments = new List<StudentCourseDM>(),  // initialize empty collections
                    TimeTables = new List<TimeTableDM>()
                };

                await lMSDbContext.Courses.AddAsync(courseDM);
                await lMSDbContext.SaveChangesAsync();
                return RedirectToAction("GetCourses");
            }
            course.BatchList = await fileService.GetBatchSelectListAsync();
            return View("Create", course);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreate(CourseVM course)
        {
            if (ModelState.IsValid)
            {


                var courseDM = new CourseDM
                {
                    Id = course.Id != Guid.Empty ? course.Id : Guid.NewGuid(),
                    Title = course.Title,
                    Description = course.Description,
                    TeacherId = course.TeacherId,    // ✅ only assign TeacherId
                    BatchId = course.BatchId,        // optional
                    Enrollments = new List<StudentCourseDM>(),  // initialize empty collections
                    TimeTables = new List<TimeTableDM>()
                };

                await lMSDbContext.Courses.AddAsync(courseDM);
                await lMSDbContext.SaveChangesAsync();
                return RedirectToAction("AdminGetCourses");
            }
            course.BatchList = await fileService.GetBatchSelectListAsync();
            course.TeacherList = await userManager.GetUsersInRoleAsync("Teacher")
                .ContinueWith(t => t.Result.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name ?? u.UserName
                }));
            return View("AdminCreateCourse", course);
        }
        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCourseEdit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var course = await lMSDbContext.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Ensure teacher can edit only their own course (unless Admin)
            var user = await userManager.GetUserAsync(User);
            if (User.IsInRole("Teacher") && course.TeacherId != user.Id)
            {
                return Forbid();
            }
            var courseVM = mapper.Map<CourseVM>(course);
            courseVM.TeacherList = await userManager.GetUsersInRoleAsync("Teacher")
               .ContinueWith(t => t.Result.Select(u => new SelectListItem
               {
                   Value = u.Id.ToString(),
                   Text = u.Name ?? u.UserName
               }));
            return View(courseVM);
        }
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }
            var course = await lMSDbContext.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            // Ensure teacher can edit only their own course (unless Admin)
            var user = await userManager.GetUserAsync(User);
            if (User.IsInRole("Teacher") && course.TeacherId != user.Id)
            {
                return Forbid();
            }
            var courseVM = mapper.Map<CourseVM>(course);
            {   courseVM.TeacherId = user.Id; }
            return View(courseVM);
        }




        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(Guid id, CourseVM course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                var courseDM = mapper.Map<CourseDM>(course);
                {                  
                    courseDM.TeacherId = user.Id;
                }
                try
                {
                    lMSDbContext.Courses.Update(courseDM);
                    await lMSDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!lMSDbContext.Courses.Any(e => e.Id == course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(GetCourses));
            }
            return View("Edit", course);
        }
        public async Task<IActionResult> AdminEdit(Guid id, CourseVM course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var courseDM = mapper.Map<CourseDM>(course);
                try
                {
                    lMSDbContext.Courses.Update(courseDM);
                    await lMSDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!lMSDbContext.Courses.Any(e => e.Id == course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AdminGetCourses));
            }
            course.TeacherList = await userManager.GetUsersInRoleAsync("Teacher")
               .ContinueWith(t => t.Result.Select(u => new SelectListItem
               {
                   Value = u.Id.ToString(),
                   Text = u.Name ?? u.UserName
               }));
            return View("AdminCourseEdit", course);
        }
        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var course = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            //  Check if any students are enrolled in this course
            bool hasEnrolledStudents = await lMSDbContext.StudentCourses
                .AnyAsync(e => e.CourseId == id);

            if (hasEnrolledStudents)
            {
                TempData["Message"]= "Cannot delete course. There are students enrolled in this course.";
                var courseVMM = mapper.Map<CourseVM>(course);
                return RedirectToAction("GetCourses");


            }
            if (course == null)
            {
                return NotFound();
            }
            var courseVM = mapper.Map<CourseVM>(course);
            return View(courseVM);
        }


        public async Task<IActionResult> AdminDelete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var course = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            //  Check if any students are enrolled in this course
            bool hasEnrolledStudents = await lMSDbContext.StudentCourses
                .AnyAsync(e => e.CourseId == id);

            if (hasEnrolledStudents)
            {
                TempData["Message"] = "Cannot delete course. There are students enrolled in this course.";
                var courseVMM = mapper.Map<CourseVM>(course);
                return RedirectToAction("AdminGetCourses");


            }
            if (course == null)
            {
                return NotFound();
            }
            var courseVM = mapper.Map<CourseVM>(course);
            return View(courseVM);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var course = await lMSDbContext.Courses.FindAsync(id);
            if (course != null)
            {
                lMSDbContext.Courses.Remove(course);
                await lMSDbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(GetCourses));
        }
        [HttpPost, ActionName("AdminDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDeleteConfirmed(Guid id)
        {
            var course = await lMSDbContext.Courses.FindAsync(id);
            if (course != null)
            {
                lMSDbContext.Courses.Remove(course);
                await lMSDbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AdminGetCourses));
        }
        [Route("ExploreCourses")]
        public async Task<IActionResult> ExploreCourses()
        {
           
            var courses = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.TimeTables)
                .Include(c => c.Enrollments)
                .ToListAsync();

            var courseVM = mapper.Map<List<CourseVM>>(courses);
            {  foreach (var course in courseVM)
                {
                    course.EnrolledStudentsCount = course.Enrollments?.Count ?? 0;
                }
            }
            
            return View(courseVM);
        }
        [HttpPost]
        public async Task<IActionResult> Enroll(Guid courseId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginStudent", "Student");
            // Check existing enrollment request
            var existingEnrollment = await lMSDbContext.StudentCourses
                .FirstOrDefaultAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsApproved)
                {
                    TempData["Message"] = "You are already enrolled in this course.";
                    return RedirectToAction("ExploreCourses");
                }
                else
                {
                    TempData["Message"] = "Your enrollment request is still pending approval.";
                    return RedirectToAction("ExploreCourses");
                }
            }
            var enrollment = new StudentCourseDM
            {
                StudentId = user.Id,
                CourseId = courseId,
                IsApproved = false
            };

            lMSDbContext.StudentCourses.Add(enrollment);
            await lMSDbContext.SaveChangesAsync();

            TempData["Message"] = "Enrollment request submitted successfully. Waiting for admin approval.";
            return RedirectToAction("ExploreCourses");
        }
        public async Task<IActionResult> MyCourses()
        {
            var user = await userManager.GetUserAsync(User);

            var courses = await lMSDbContext.StudentCourses
                .Where(e => e.StudentId == user.Id && e.IsApproved)
                .Include(e => e.Course)
                .Select(e => new
                {
                    e.Course.Title,
                    e.Course.Description
                })
                .ToListAsync();

            return View(courses);
        }
        public async Task<IActionResult> EnrollmentRequests()
        {
            var pending = await lMSDbContext.StudentCourses
                .Where(e => !e.IsApproved)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToListAsync();

            return View(pending);
        }
        [HttpPost]
        public async Task<IActionResult> ApproveEnrollment(Guid id)
        {
            var enrollment = await lMSDbContext.StudentCourses.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            enrollment.IsApproved = true;
            lMSDbContext.StudentCourses.Update(enrollment);
            await lMSDbContext.SaveChangesAsync();
            return RedirectToAction("EnrollmentRequests");
        }
        public async Task<IActionResult> RejectEnrollment(Guid id)
        {
            var enrollment = await lMSDbContext.StudentCourses.FindAsync(id);
            if (enrollment == null)
                return NotFound();

           lMSDbContext.StudentCourses.Remove(enrollment);
            lMSDbContext.SaveChanges();
            return RedirectToAction("EnrollmentRequests");

        }
            




    }
}
