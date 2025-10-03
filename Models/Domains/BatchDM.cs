using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains
{
    public class BatchDM
    {
        [Key]
        public Guid? id { get; set; }
        public string Name { get; set; }
        public ICollection<ApplicationUser> ApplicationUser { get; set; }
        public ICollection<TeacherAssignmentDM> TeacherAssignments { get; set; }
    }
}
