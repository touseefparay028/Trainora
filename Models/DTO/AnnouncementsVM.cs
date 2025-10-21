using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.DTO
{
    public class AnnouncementsVM
    {
        public Guid? Id { get; set; }= Guid.NewGuid();

        [Required]
        public string? Description { get; set; }
        [NotMapped]
        public IFormFile? File { get; set; }
    }
}
