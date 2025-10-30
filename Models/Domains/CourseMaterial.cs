using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains
{
    public class CourseMaterial
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedOn { get; set; } = DateTime.Now;
        public Guid CourseId { get; set; }
        public CourseDM? Course { get; set; }
        //public Guid ApplicationUserId { get; set; }
        public ICollection<ApplicationUser>? ApplicationUser { get; set; }
    }
}
