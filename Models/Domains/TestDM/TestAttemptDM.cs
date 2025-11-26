using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains.TestDM
{
    public class TestAttemptDM
    {
        [Key]
        public Guid AttemptId { get; set; }

        [Required]
        public Guid TestId { get; set; }
        public TestDM Test { get; set; }

        [Required]
        public Guid StudentId { get; set; }   // your Student Guid

        [Required]
        public DateTime StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        [Required]
        public string Status { get; set; } = "InProgress";
        // InProgress, Submitted, AutoSubmitted, Expired
    }
}
