using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly LMSDbContext lMSDbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;

        public CourseController(LMSDbContext lMSDbContext, UserManager<ApplicationUser> userManager,IMapper mapper)
        {
            this.lMSDbContext = lMSDbContext;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        // GET: Courses (List of courses)
        public async Task<IActionResult> GetCourses()
        {
            var courses = await lMSDbContext.Courses
                .Include(c => c.Teacher)
                .ToListAsync();

            return View(courses);
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

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Courses/Edit/5
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
            return View(courseVM);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CourseVM course)
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
                return RedirectToAction(nameof(GetCourses));
            }
            return View("Edit",course);
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

            return View(course);
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
    }
}
