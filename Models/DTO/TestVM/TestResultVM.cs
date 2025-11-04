using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO.TestVM
{
    public class TestResultVM
    {

        [Display(Name = "Test Title")]
        public string TestTitle { get; set; }

        [Display(Name = "Score")]
        public int Score { get; set; }

        [Display(Name = "Attempted On")]
        public DateTime TakenAt { get; set; }
        public int? AssignedMarks { get; set; }
        public int TotalMarks { get; set; }
        public int TotalQuestions { get; set; }
    }
}
