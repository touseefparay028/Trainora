namespace LearningManagementSystem.Models.DTO.TestVM
{
    public class TestResponseVM
    {
        public Guid ResultId { get; set; }
        public string StudentName { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int? AssignedMarks { get; set; }
        public int TotalMarks { get; set; }
        public DateTime TakenAt { get; set; }
    }
}
