using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO.TestVM
{
    public class SubmitTestVM
    {
        [Required(ErrorMessage = "Test ID is required.")]
        public Guid TestId { get; set; }
        public Guid AttemptId { get; set; }

        [Required(ErrorMessage = "Answers are required.")]
        [MinLength(1, ErrorMessage = "You must answer at least one question.")]
        public Dictionary<int, string> Answers { get; set; } = new();
    }
}
