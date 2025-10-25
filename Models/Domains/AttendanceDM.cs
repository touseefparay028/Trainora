using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.Domains
{
    public class AttendanceDM
    {
  

            public Guid Id { get; set; }

            // Foreign key to student
            public Guid StudentId { get; set; }
       
        public ApplicationUser? Student { get; set; }

            // Foreign key to batch
            public Guid BatchDMId { get; set; }
            public BatchDM? Batch { get; set; }

            // Foreign key to course
            public Guid CourseId { get; set; }
            public CourseDM? Course { get; set; }

            // Attendance details
            public DateTime Date { get; set; }
            public TimeSpan? JoinTime { get; set; }
            public bool IsPresent { get; set; }

            // Optional: reason for absence
            public string? Remark { get; set; }

       
    }
}

