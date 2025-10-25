using LearningManagementSystem.Models.Domains;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.IdentityEntities
{
    public class ApplicationUser:IdentityUser<Guid>
    {

        public string? Name {  get; set; }
        public string? Address { get; set; }
        public string? gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? EnrollmentNumber { get; set; } 

        public string? Course { get; set; }

        public ICollection<TeacherAssignmentDM>? teacherAssignmentDMs { get; set; }
        public Guid? BatchDMId { get; set; }

        public ICollection<BatchDM>? BatchDM { get; set; }
       
    }
}
