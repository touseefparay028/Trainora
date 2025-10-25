using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.Domains
{
    public class ClassSessions
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseDM Course { get; set; }
        [Required]
        public DateTime SessionDate { get; set; } // Stores date of the class

        [Required]
        public TimeSpan SessionTime { get; set; } // Stores time of the class
    }
}
