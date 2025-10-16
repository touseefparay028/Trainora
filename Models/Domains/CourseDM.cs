using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.Domains
{
    public class CourseDM
    {
      
            [Key]
            public Guid Id { get; set; }

            
            public string? Title { get; set; }

            public string? Description { get; set; }

            
            public Guid TeacherId { get; set; }  // From Identity user

            [ForeignKey(nameof(TeacherId))]
            public ApplicationUser? Teacher { get; set; }  // your Identity user model

            // Navigation
            public ICollection<StudentCourseDM> Enrollments { get; set; }
            public ICollection<TimeTableDM> TimeTables { get; set; }
        

    }
}
