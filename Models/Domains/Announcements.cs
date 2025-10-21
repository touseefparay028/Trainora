using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains
{
    public class Announcements
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string? Description { get; set; }

        public string? FilePath { get; set; }  // optional uploaded file

        public  Guid CreatedBy { get; set; }  // store the user ID
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
