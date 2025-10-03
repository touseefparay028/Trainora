namespace LearningManagementSystem.Models.DTO
{
    public class QuizQuestionVM
    {

            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public List<string> Options { get; set; }
            public string SelectedAnswer { get; set; }
     

    }
}
