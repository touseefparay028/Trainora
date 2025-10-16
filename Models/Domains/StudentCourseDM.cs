using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.Domains
{
    public class StudentCourseDM
    {
      
            [Key]
            public Guid Id { get; set; }

            [Required]
            public Guid StudentId { get; set; }  // From Identity user

            [Required]
            public Guid CourseId { get; set; }

            public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

            [ForeignKey(nameof(StudentId))]
            public ApplicationUser Student { get; set; }

            [ForeignKey(nameof(CourseId))]
            public CourseDM Course { get; set; }
        
    }
}
