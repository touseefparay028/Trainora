namespace LearningManagementSystem.Models.DTO
{
    public class AttendanceVM
    {
        public Guid id { get; set; }
        public string StudentName { get; set; }
        public string BatchName { get; set; }
        public string CourseName { get; set; }
        public Guid StudentId { get; set; }   // 🔹 Needed for manual attendance marking
        public Guid BatchDMId { get; set; }   // 🔹 Needed for manual attendance marking
        public Guid CourseId { get; set; }    // 🔹 Needed for manual attendance marking
        public DateTime Date { get; set; }
        public string JoinTime { get; set; }
        public bool IsPresent { get; set; }
    }
}
