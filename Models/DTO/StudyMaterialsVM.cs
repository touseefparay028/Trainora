using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.DTO
{
    public class StudyMaterialsVM
    {
        public Guid Id { get; set; }= Guid.NewGuid();

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Upload File")]
        [Required(ErrorMessage = "Please upload a file")]
        [NotMapped]
        public IFormFile File { get; set; }

        public string UploadedBy { get; set; }

        public string? FilePath { get; set; }

        public DateTime UploadedOn { get; set; } = DateTime.Now;
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
    
}
