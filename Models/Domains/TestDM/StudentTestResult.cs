using LearningManagementSystem.Models.IdentityEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace LearningManagementSystem.Models.Domains.TestDM
{
    public class StudentTestResult
    {
        [Key]
        public Guid ResultId { get; set; }

        [Required]
        public Guid StudentId { get; set; }
        public ApplicationUser? Student {  get; set; }

        [ForeignKey("Test")]
        public Guid TestId { get; set; }

        public TestDM? Test { get; set; }

        public int Score { get; set; }
        public int? AssignedMarks { get; set; }

        public DateTime TakenAt { get; set; } = DateTime.Now;
    }
}
