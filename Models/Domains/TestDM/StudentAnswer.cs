using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains.TestDM
{
    public class StudentAnswer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid TestId { get; set; }

        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public string SelectedOption { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
