using LearningManagementSystem.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO
{
    public class UpdateAssignmentVM
    {
        public Guid Id { get; set; }         // StudentAssignmentDM Id
        [Required(ErrorMessage = "Please upload a file.")]
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".pptx" })]
        [MaxFileSize(5 * 1024 * 1024)] // 5 MB
        public IFormFile NewFile { get; set; }
    }

}
