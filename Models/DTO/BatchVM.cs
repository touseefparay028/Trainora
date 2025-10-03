using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.IdentityEntities;

namespace LearningManagementSystem.Models.DTO
{
    public class BatchVM
    {
        public Guid id { get; set; }=Guid.NewGuid();
        public string Name { get; set; }
        public ICollection<ApplicationUser> ApplicationUser { get; set; }
        public ICollection<TeacherAssignmentDM> TeacherAssignments { get; set; }
    }
}
