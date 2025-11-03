using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains
{
    public class TeacherAssignmentDM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Path { get; set; }
        public string? Subject { get; set; }

        [Required]
       
        public DateTime? DueTime { get; set; }
        public Guid ApplicationUserId { get; set; }
        public ICollection<ApplicationUser>? ApplicationUser { get; set; }
        //public int TeacherDMId { get; set;}
        //public TeacherDM TeacherDM { get; set; }
        public Guid BatchDMId { get; set; }
        public BatchDM? BatchDM { get; set; } 

    }
}
