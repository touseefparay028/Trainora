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
        [StringLength(100,MinimumLength = 10, ErrorMessage ="Name should be at least 10 characters")]
        public string StudentName { get; set; }
      public Guid? assignmentDMId { get; set; }
        [Required]
        [NotMapped]
        [FileExtensions(Extensions = "pdf,docx,pptx", ErrorMessage = "Only .pdf, .docx, .pptx files are allowed.")]
        //[AllowedExtensions(new string[] { ".pdf", ".docx", ".pptx" })]
        [MaxFileSize(5 * 1024 * 1024)] // 5 MB
        public IFormFile File { get; set; }
        public string Path { get; set; }
    }
}
