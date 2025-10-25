using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.Domains
{
    public class TimeTableDM
    {
             [Key]
            public Guid Id { get; set; }

            [Required]
            public Guid CourseId { get; set; }

            [Required]
            public string Day { get; set; } // e.g. "Monday", "Wednesday"

            [Required]
            public TimeSpan StartTime { get; set; }

            [Required]
            public TimeSpan EndTime { get; set; }

            public string LabLocation { get; set; }

            [ForeignKey(nameof(CourseId))]
            public CourseDM Course { get; set; }
        
            
    }
}
