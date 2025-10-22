using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO
{
    public class BatchVM
    {
        public Guid id { get; set; }=Guid.NewGuid();
        [Required(ErrorMessage = "Name Can't be blank")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
        public ICollection<ApplicationUser> ApplicationUser { get; set; }
        public ICollection<TeacherAssignmentDM> TeacherAssignments { get; set; }
    }
}
