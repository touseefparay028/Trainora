using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains.TestDM;
using LearningManagementSystem.Models.DTO.TestVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers.Test
{
    public class TestController : Controller
    {
        private readonly LMSDbContext lMSDbContext;

        public TestController(LMSDbContext lMSDbContext)
        {
            this.lMSDbContext = lMSDbContext;
        }
        // GET: /Test/ListByCourse/{courseId}
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> ListByCourse(Guid courseId)
        {
            var tests = await lMSDbContext.Tests
                .Include(t => t.Course)
                .Where(t => t.CourseId == courseId)
                .ToListAsync();

            if (!tests.Any())
            {
                ViewBag.Message = "No tests found for this course.";
            }

            var TestList = tests.Select(t => new CreateTestVM
            {
                TestId = t.TestId,
                Title = t.Title,
                TotalMarks = t.TotalMarks,
                CourseId = t.CourseId
            }).ToList();
            var course = await lMSDbContext.Courses
     .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course != null)
            {
                ViewBag.CourseName = course.Title;
            }
            else
            {
                ViewBag.CourseName = "Unknown Course";
            }

            ViewBag.CourseId = courseId;

            return View(TestList);
        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> Create(Guid courseId)
        {
            var course = await lMSDbContext.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                return NotFound();
            }

            var vm = new CreateTestVM
            {
                CourseId = courseId
            };

            ViewBag.CourseName = course.Title;
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> Create(CreateTestVM model)
        {
            if (!ModelState.IsValid)
            {
                var course = await lMSDbContext.Courses.FirstOrDefaultAsync(c => c.Id == model.CourseId);
                ViewBag.CourseName = course?.Title;
                return View("Create",model);
            }

            var test = new TestDM
            {
                TestId = Guid.NewGuid(),
                Title = model.Title,
                TotalMarks = model.TotalMarks,
                CourseId = model.CourseId
            };

            lMSDbContext.Tests.Add(test);
            await lMSDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Test created successfully!";
            return RedirectToAction("Create", new { courseId = model.CourseId });
        }
        public async Task<IActionResult> DeleteTest(Guid testId)
        {
            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                return NotFound();
            }
            var courseId = test.CourseId;
            // Remove associated questions first
            lMSDbContext.Questions.RemoveRange(test.Questions);
            lMSDbContext.Tests.Remove(test);
            await lMSDbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Test and its questions deleted successfully!";
            return RedirectToAction("ListByCourse", new { courseId = courseId });
        }
        // GET: /Test/AddQuestion/{testId}
        [HttpGet]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> AddQuestion(Guid testId)
        {
            var test = await lMSDbContext.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                return NotFound();
            }

            var vm = new AddQuestionVM
            {
                TestId = testId
            };

            ViewBag.TestTitle = test.Title;
            return View(vm);
        }

        // POST: /Test/AddQuestion
        [HttpPost]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(AddQuestionVM model)
        {
            if (!ModelState.IsValid)
            {
                var test = await lMSDbContext.Tests.FirstOrDefaultAsync(t => t.TestId == model.TestId);
                ViewBag.TestTitle = test?.Title;
                return View(model);
            }

            var question = new QuestionDM
            {
                QuestionId = Guid.NewGuid(),
                TestId = model.TestId,
                QuestionText = model.QuestionText,
                OptionA = model.OptionA,
                OptionB = model.OptionB,
                OptionC = model.OptionC,
                OptionD = model.OptionD,
                CorrectAnswer = model.CorrectAnswer
            };

            lMSDbContext.Questions.Add(question);
            await lMSDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Question added successfully!";
            return RedirectToAction("ViewQuestions", new { testId = model.TestId });
        }
        // GET: /Test/ViewQuestions/{testId}
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> ViewQuestions(Guid testId)
        {
            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .Include(t => t.Course)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                return NotFound();
            }

            ViewBag.TestTitle = test.Title;

            var questions = test.Questions?
                .Select(q => new QuestionVM
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD
                }).ToList() ?? new List<QuestionVM>();
            ViewBag.CourseId = test.CourseId;
            return View(questions);
        }
        // POST: /Test/DeleteQuestion
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        [HttpPost]
      
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            var question = await lMSDbContext.Questions
                .Include(q => q.Test)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
            {
                return NotFound();
            }

            var testId = question.TestId;

            lMSDbContext.Questions.Remove(question);
            await lMSDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Question deleted successfully!";
            return RedirectToAction("ViewQuestions", new { testId = testId });
        }


    }
}
