using LearningManagementSystem.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.Marshalling;

namespace LearningManagementSystem.Models.DTO
{
    public class StudentAssignmentVM
    {
        public Guid Id { get; set; }= Guid.NewGuid();
        [Required(ErrorMessage ="Student name is required")]
        public string StudentName { get; set; }
      public Guid? assignmentDMId { get; set; }
        [Required]
        [NotMapped]
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".pptx" })]
        [MaxFileSize(5 * 1024 * 1024)] // 5 MB
        public IFormFile File { get; set; }
        public string Path { get; set; }
    }
}
