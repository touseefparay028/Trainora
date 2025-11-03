using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace LearningManagementSystem.Models.Domains.TestDM
{
    public class QuestionDM
    {
        [Key]
        public Guid QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string OptionA { get; set; }

        [Required]
        public string OptionB { get; set; }

        [Required]
        public string OptionC { get; set; }

        [Required]
        public string OptionD { get; set; }

        [Required]
        public string CorrectAnswer { get; set; } // "A", "B", "C", or "D"

        [ForeignKey("Test")]
        public Guid TestId { get; set; }

        public TestDM? Test { get; set; }
    }
}
