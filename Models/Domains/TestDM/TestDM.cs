using LearningManagementSystem.Models.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.Domains.TestDM
{
    public class TestDM
    {
        [Key]
        public Guid TestId { get; set; }

        [Required]
        public string Title { get; set; }

        public Guid? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public CourseDM? Course { get; set; }

        public int? TotalMarks { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public ICollection<QuestionDM>? Questions { get; set; }
    }
}
