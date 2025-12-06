using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains.TestDM;
using LearningManagementSystem.Models.DTO.TestVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;

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
                CourseId = model.CourseId,
                DurationMinutes = model.DurationMinutes
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
            // To get the test (title, total marks, existing questions)
            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == model.TestId);

            if (test == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
               
                ViewBag.TestTitle = test?.Title;
                return View("AddQuestion",model);
            }
            if (model.Marks <= 0)
            {
                ModelState.AddModelError(nameof(model.Marks), "Marks must be greater than 0.");
                return View(model);
            }

            // 🔹 Calculate existing total marks of questions for this test
            int existingMarks = test.Questions?.Sum(q => q.Marks) ?? 0;
            int newTotalMarks = existingMarks + model.Marks;

            // 🔹 If Test.TotalMarks is set, enforce sum(question marks) ≤ TotalMarks
            if (test.TotalMarks.HasValue && newTotalMarks > test.TotalMarks.Value)
            {
                int remaining = test.TotalMarks.Value - existingMarks;

                ModelState.AddModelError(nameof(model.Marks),
                    $"You can assign only {remaining} marks more to this test. " +
                    $"Current total of questions = {existingMarks}, Test total = {test.TotalMarks}.");

                // Optional: show remaining marks in the view
                ViewBag.RemainingMarks = remaining;

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
                CorrectAnswer = model.CorrectAnswer,
                Marks = model.Marks
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
        [HttpPost]
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> UploadQuestions(Guid testId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("AddQuestion", new { testId = testId });
            }

            // Get the test with existing questions for marks validation
            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                TempData["Error"] = "Test not found.";
                return RedirectToAction("AddQuestion", new { testId = testId });
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet.Dimension == null)
            {
                TempData["Error"] = "Excel file seems to be empty.";
                return RedirectToAction("AddQuestion", new { testId = testId });
            }

            int rowCount = worksheet.Dimension.Rows;

            List<QuestionDM> questions = new();
            int newQuestionsTotalMarks = 0;

            for (int row = 2; row <= rowCount; row++) // Row 1 = header
            {
                // Skip completely empty rows
                var qText = worksheet.Cells[row, 1].Value?.ToString();
                var optionA = worksheet.Cells[row, 2].Value?.ToString();
                var optionB = worksheet.Cells[row, 3].Value?.ToString();
                var optionC = worksheet.Cells[row, 4].Value?.ToString();
                var optionD = worksheet.Cells[row, 5].Value?.ToString();
                var correct = worksheet.Cells[row, 6].Value?.ToString();
                var marksCell = worksheet.Cells[row, 7].Value?.ToString();

                bool isRowEmpty = string.IsNullOrWhiteSpace(qText)
                                  && string.IsNullOrWhiteSpace(optionA)
                                  && string.IsNullOrWhiteSpace(optionB)
                                  && string.IsNullOrWhiteSpace(optionC)
                                  && string.IsNullOrWhiteSpace(optionD)
                                  && string.IsNullOrWhiteSpace(correct)
                                  && string.IsNullOrWhiteSpace(marksCell);

                if (isRowEmpty)
                    continue;

                // Basic required checks
                if (string.IsNullOrWhiteSpace(qText)
                    || string.IsNullOrWhiteSpace(optionA)
                    || string.IsNullOrWhiteSpace(optionB)
                    || string.IsNullOrWhiteSpace(optionC)
                    || string.IsNullOrWhiteSpace(optionD)
                    || string.IsNullOrWhiteSpace(correct))
                {
                    TempData["Error"] = $"Invalid data at row {row}. Please ensure question text, options, and correct answer are filled.";
                    return RedirectToAction("AddQuestion", new { testId = testId });
                }

                // 🔹 Marks validation for this row
                if (string.IsNullOrWhiteSpace(marksCell) || !int.TryParse(marksCell, out int marks) || marks <= 0)
                {
                    TempData["Error"] = $"Invalid marks at row {row}. Marks must be a positive integer.";
                    return RedirectToAction("AddQuestion", new { testId = testId });
                }

                newQuestionsTotalMarks += marks;

                var question = new QuestionDM
                {
                    QuestionId = Guid.NewGuid(),
                    TestId = testId,
                    QuestionText = qText,
                    OptionA = optionA,
                    OptionB = optionB,
                    OptionC = optionC,
                    OptionD = optionD,
                    CorrectAnswer = correct,
                    Marks = marks
                };

                questions.Add(question);
            }

            if (!questions.Any())
            {
                TempData["Error"] = "No valid questions found in the Excel file.";
                return RedirectToAction("AddQuestion", new { testId = testId });
            }

            // 🔹 Validate against Test.TotalMarks
            if (test.TotalMarks.HasValue)
            {
                int existingMarks = test.Questions?.Sum(q => q.Marks) ?? 0;
                int remaining = test.TotalMarks.Value - existingMarks;

                if (newQuestionsTotalMarks > remaining)
                {
                    TempData["Error"] =
                        $"Uploaded questions total marks ({newQuestionsTotalMarks}) exceed the remaining allowed marks ({remaining}). " +
                        $"Existing questions total = {existingMarks}, Test total = {test.TotalMarks}.";
                    return RedirectToAction("AddQuestion", new { testId = testId });
                }
            }

            // ✅ All good – save to DB
            lMSDbContext.Questions.AddRange(questions);
            await lMSDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{questions.Count} questions uploaded successfully!";
            return RedirectToAction("AddQuestion", new { testId = testId });
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

        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public async Task<IActionResult> AvailableTests(Guid courseId)
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var course = await lMSDbContext.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.CourseName = course.Title;

            // Fetch all tests for this course
            var tests = await lMSDbContext.Tests
                .Where(t => t.CourseId == courseId)
                .Select(t => new CreateTestVM
                {
                    TestId = t.TestId,
                    Title = t.Title,
                    CourseId = t.CourseId,
                    TotalMarks = t.TotalMarks
                })
                .ToListAsync();

            // Get all test IDs already attempted by the student
            var takenTestIds = await lMSDbContext.StudentTestResults
                .Where(r => r.StudentId == studentId)
                .Select(r => r.TestId)
                .ToListAsync();

            // Pass both lists to ViewBag
            ViewBag.TakenTestIds = takenTestIds;

            return View(tests);
        }

        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> TakeTest(Guid testId)
        {
            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                return NotFound();
            }
            if (test.DurationMinutes <= 0)
            {
                TempData["ErrorMessage"] = "Test duration is not set.";
                return RedirectToAction("AvailableTests", new { courseId = test.CourseId });
            }
            var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentIdClaim == null) return Unauthorized();
            
            var studentId = Guid.Parse(studentIdClaim);
            
            var nowUtc = DateTime.UtcNow;

            // 🔹 Find or create attempt
            var attempt = await lMSDbContext.TestAttempts
                .FirstOrDefaultAsync(a => a.TestId == testId && a.StudentId == studentId);
            if (attempt == null)
            {
                attempt = new TestAttemptDM
                {
                    AttemptId = Guid.NewGuid(),
                    TestId = testId,
                    StudentId = studentId,
                    StartTimeUtc = nowUtc,
                    Status = "InProgress"
                };

                lMSDbContext.TestAttempts.Add(attempt);
                await lMSDbContext.SaveChangesAsync();
            }
            var expireAt = attempt.StartTimeUtc.AddMinutes(test.DurationMinutes);
            var remainingSeconds = (int)(expireAt - nowUtc).TotalSeconds;
            if (remainingSeconds <= 0)
            {
                attempt.Status = "Expired";
                attempt.EndTimeUtc = nowUtc;
                await lMSDbContext.SaveChangesAsync();

                TempData["ErrorMessage"] = "Your test time has expired.";
                return RedirectToAction("AvailableTests", new { courseId = test.CourseId });
            }

            var vm = new TakeTestVM
            {
                TestId = test.TestId,
                TestTitle = test.Title,
                Questions = test.Questions.Select(q => new QuestionVM
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD
                }).ToList(),
                AttemptId = attempt.AttemptId
            };

            ViewBag.RemainingSeconds = remainingSeconds;
            return View(vm);
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(SubmitTestVM model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("TakeTest", new { testId = model.TestId });
            }
            var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentIdClaim == null) return Unauthorized();
            var studentId = Guid.Parse(studentIdClaim);

            // 🔹 1. Load attempt with test (for duration)
            var attempt = await lMSDbContext.TestAttempts
                .Include(a => a.Test)
                .FirstOrDefaultAsync(a => a.AttemptId == model.AttemptId && a.StudentId == studentId);

            if (attempt == null)
            {
                TempData["SuccessMessage"] = "Test attempt not found.";
                ModelState.AddModelError(string.Empty, "Test attempt not found.");
                return RedirectToAction("TakeTest", new { testId = model.TestId });
            }

            var nowUtc = DateTime.UtcNow;
            var expireAt = attempt.StartTimeUtc.AddMinutes(attempt.Test.DurationMinutes);

            if (nowUtc > expireAt)
            {
                attempt.Status = "AutoSubmitted";
            }
            else
            {
                attempt.Status = "Submitted";
            }
            attempt.EndTimeUtc = nowUtc;



            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == model.TestId);

            if (test == null)
            {
                ModelState.AddModelError(string.Empty, "Test not found");
                return RedirectToAction("TakeTest", new { testId = model.TestId });
            }

            int score = 0;
            int MarksObtained = 0;


            // Save responses for each question
            var studentAnswers = new List<StudentAnswer>();

            foreach (var q in test.Questions)
            {
                if (model.Answers.TryGetValue(q.QuestionId.GetHashCode(), out string givenAnswer))
                {
                    bool isCorrect = string.Equals(givenAnswer, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                    if (isCorrect)
                    {
                        score++;
                        MarksObtained += q.Marks;
                    }

                    studentAnswers.Add(new StudentAnswer
                    {
                        StudentId = studentId,
                        TestId = test.TestId,
                        QuestionId = q.QuestionId,
                        SelectedOption = givenAnswer,
                        IsCorrect = isCorrect
                    });
                }
            }

            await lMSDbContext.StudentAnswers.AddRangeAsync(studentAnswers);

            // Save main result
            var result = new StudentTestResult
            {
                ResultId = Guid.NewGuid(),
                StudentId = studentId,
                TestId = model.TestId,
                Score = score,
                AssignedMarks = MarksObtained,
                TakenAt = DateTime.Now
            };

            lMSDbContext.StudentTestResults.Add(result);
            await lMSDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You scored {score} out of {test.Questions.Count}!";
            return RedirectToAction("AvailableTests", new { courseId = test.CourseId });
        }

        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        public async Task<IActionResult> ViewResponses(Guid testId)
        {
            var test = await lMSDbContext.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
                return NotFound();

            ViewBag.TestTitle = test.Title;
            int totalQuestions = test.Questions.Count;

            var responses = await lMSDbContext.StudentTestResults
                .Where(r => r.TestId == testId)
                .Include(r => r.Test)
                .Include(r => r.Student)
                .Select(r => new TestResponseVM
                {
                    ResultId = r.ResultId,
                    StudentName = r.Student.Name,
                    Score = r.Score,
                    TotalQuestions = totalQuestions,
                    AssignedMarks = r.AssignedMarks, // if you have separate marks column, map that instead
                    TotalMarks = test.TotalMarks ?? 100,
                    TakenAt = r.TakenAt
                })
                .ToListAsync();
            ViewBag.CourseId = test.CourseId;
            return View(responses);
        }
        [Authorize(AuthenticationSchemes = "TeacherAuth", Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> SaveMarksForStudent(Guid ResultId, int Marks)
        {
            var result = await lMSDbContext.StudentTestResults.FindAsync(ResultId);
            if (result == null)
            {
                TempData["ErrorMessage"] = "Student test result not found.";
                return RedirectToAction("ListByCourse");
            }

            result.AssignedMarks = Marks;
            await lMSDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Marks saved successfully!";
            return RedirectToAction("ViewResponses", new { testId = result.TestId });
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public async Task<IActionResult> TestResult(Guid testId)
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var result = await lMSDbContext.StudentTestResults
                .Include(r => r.Test)
                .Where(r => r.TestId == testId && r.StudentId == studentId)
                .Select(r => new TestResultVM
                {
                    TestTitle = r.Test.Title,
                    Score = r.Score,
                    AssignedMarks = r.AssignedMarks,
                    TotalMarks = r.Test.TotalMarks ?? 0,
                    TotalQuestions = r.Test.Questions.Count,
                    TakenAt = r.TakenAt
                })
                .FirstOrDefaultAsync();
            ViewBag.CourseId = (await lMSDbContext.Tests.FindAsync(testId))?.CourseId;
            if (result == null)
            {
                TempData["ErrorMessage"] = "No result found for this test.";
                return RedirectToAction("AvailableTests", new { courseId = ViewBag.CourseId });
            }
            ViewBag.TestId = testId;
            return View(result);
        }
        [Authorize(AuthenticationSchemes = "StudentAuth", Roles = "Student")]
        public async Task<IActionResult> ViewTestResponses(Guid testId)
        {
            var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentIdClaim == null) return Unauthorized();

            var studentId = Guid.Parse(studentIdClaim);

            var responses = await lMSDbContext.StudentAnswers
                .Where(a => a.StudentId == studentId && a.TestId == testId)
                .Join(lMSDbContext.Questions,
                      answer => answer.QuestionId,
                      question => question.QuestionId,
                      (answer, question) => new StudentTestResponseVM
                      {
                          QuestionId = question.QuestionId,
                          QuestionText = question.QuestionText,
                          OptionA = question.OptionA,
                          OptionB = question.OptionB,
                          OptionC = question.OptionC,
                          OptionD = question.OptionD,
                          CorrectAnswer = question.CorrectAnswer,
                          StudentAnswer = answer.SelectedOption,
                          IsCorrect = answer.IsCorrect
                      })
                .ToListAsync();

            if (responses == null || !responses.Any())
            {
                ViewBag.Message = "No responses found for this test.";
                return View("TestResult");
            }

            return View(responses);
        }


    }
}
