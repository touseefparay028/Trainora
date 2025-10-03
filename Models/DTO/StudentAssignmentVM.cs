using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.Marshalling;

namespace LearningManagementSystem.Models.DTO
{
    public class StudentAssignmentVM
    {
        public Guid Id { get; set; }= Guid.NewGuid();
        public string StudentName { get; set; }
      public Guid? assignmentDMId { get; set; }
        [Required]
        [NotMapped]
        public IFormFile File { get; set; }
    }
}
