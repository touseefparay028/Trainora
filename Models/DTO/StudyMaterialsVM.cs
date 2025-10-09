using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.DTO
{
    public class StudyMaterialsVM
    {
        public Guid Id { get; set; }= Guid.NewGuid();

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s.,'-]+$", ErrorMessage = "Title can only contain letters, numbers, spaces, and basic punctuation.")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s.,'""!?\-()]+$", ErrorMessage = "Description can only contain letters, numbers, spaces, and basic punctuation.")]
        public string Description { get; set; }

        [Display(Name = "Upload File")]
        [Required(ErrorMessage = "Please upload a file")]
        [NotMapped]
        [AllowedExtensions(new string[] { ".pdf", ".docx", ".pptx" })]
        [MaxFileSize(5 * 1024 * 1024)] // 5 MB
        public IFormFile File { get; set; }

        public string? UploadedBy { get; set; }

        public string? FilePath { get; set; }

        public DateTime UploadedOn { get; set; } = DateTime.Now;
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
    
}
