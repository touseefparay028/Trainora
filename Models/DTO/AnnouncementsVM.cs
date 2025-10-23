using LearningManagementSystem.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.DTO
{
    public class AnnouncementsVM
    {
        public Guid? Id { get; set; }= Guid.NewGuid();
        public string? FilePath { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 200 characters.")]
        public string? Description { get; set; }
       
        [NotMapped]
        [Required]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".pptx" })]
        public IFormFile? File { get; set; }
    }
}
