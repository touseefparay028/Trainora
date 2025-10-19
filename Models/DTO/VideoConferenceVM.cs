using LearningManagementSystem.Models.Domains;

namespace LearningManagementSystem.Models.DTO
{
    public class VideoConferenceVM
    {
        public Guid Id { get; set; }
        public string MeetingLink { get; set; }  // The Jitsi meeting URL
        public Guid TeacherId { get; set; }
        public Guid BatchId { get; set; }
        public BatchDM batch {  get; set; }
        public string BatchName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
