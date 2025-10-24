namespace LearningManagementSystem.Models.DTO
{
    public class AttendanceVM
    {
        public string StudentName { get; set; }
        public string BatchName { get; set; }
        public string CourseName { get; set; }
        public DateTime Date { get; set; }
        public string JoinTime { get; set; }
        public bool IsPresent { get; set; }
    }
}
