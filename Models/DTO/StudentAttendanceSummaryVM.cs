namespace LearningManagementSystem.Models.DTO
{
    public class StudentAttendanceSummaryVM
    { // Student details
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        // Course details
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }

        // Attendance stats
        public int TotalClasses { get; set; }
        public int ClassesAttended { get; set; }

        // Computed field
        public double AttendancePercentage
        {
            get
            {
                if (TotalClasses == 0) return 0;
                return Math.Round(((double)ClassesAttended / TotalClasses) * 100, 2);
            }
        }

        // Optional: detailed attendance records (for charts or date-wise listing)
        public List<AttendanceDetailVM> AttendanceRecords { get; set; } = new();
    }

    // Nested model for date-wise attendance details
    public class AttendanceDetailVM
    {
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public string? Remark { get; set; }
    }
}
