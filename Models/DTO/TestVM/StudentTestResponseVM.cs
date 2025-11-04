namespace LearningManagementSystem.Models.DTO.TestVM
{
    public class StudentTestResponseVM
    {
        public Guid QuestionId { get; set; }

        public string QuestionText { get; set; }
        public Guid StudentId { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }

        public string CorrectAnswer { get; set; }
        public string StudentAnswer { get; set; }

        public bool IsCorrect { get; set; }
    }
}
