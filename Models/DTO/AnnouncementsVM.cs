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
        [FileExtensions(Extensions = "jpg,jpeg,png,pdf", ErrorMessage = "Only image or document files are allowed.")]
        public IFormFile? File { get; set; }
    }
}
