using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains
{
    public class StudyMaterialsDM
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string? UploadedBy { get; set; }

        public DateTime UploadedOn { get; set; } = DateTime.Now;
        public Guid ApplicationUserId { get; set; }
        public ICollection<ApplicationUser>? ApplicationUser { get; set; }
    }
}
