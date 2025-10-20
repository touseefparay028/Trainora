using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
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

        public CourseController(LMSDbContext lMSDbContext, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            this.lMSDbContext = lMSDbContext;
            this.userManager = userManager;
            this.mapper = mapper;
        }
        [Route("GetCourses")]
        [Authorize(Roles = "Teacher")]
        // GET: Courses (List of courses)
        public async Task<IActionResult> GetCourses()
        {
            var courses = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.TimeTables)
                .ToListAsync();
            var CourseVM = mapper.Map<List<CourseVM>>(courses);
            return View(CourseVM);
        }
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminGetCourses()
        {
            var courses = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.TimeTables)
                .ToListAsync();
            var CourseVM = mapper.Map<List<CourseVM>>(courses);
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
        public IActionResult Create()
        {
            return View();
        }
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreateCourse()
        {
            // Get all users with the "Teacher" role
            var teachers = await userManager.GetUsersInRoleAsync("Teacher");
            var courseVM = new CourseVM
            {
                TeacherList = teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),               // This will be the selected value (Teacher Id)
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

                var CourseDM = mapper.Map<CourseDM>(course);
                {
                    CourseDM.TeacherId = user.Id;
                }
                await lMSDbContext.Courses.AddAsync(CourseDM);
                await lMSDbContext.SaveChangesAsync();
                return RedirectToAction("GetCourses");
            }
            return View("Create", course);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreate(CourseVM course)
        {
            if (ModelState.IsValid)
            {


                var CourseDM = mapper.Map<CourseDM>(course);
                {
                    CourseDM.TeacherId = course.TeacherId;
                }
                await lMSDbContext.Courses.AddAsync(CourseDM);
                await lMSDbContext.SaveChangesAsync();
                return RedirectToAction("AdminGetCourses");
            }
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
    }
}
