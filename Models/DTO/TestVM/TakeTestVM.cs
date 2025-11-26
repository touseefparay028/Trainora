namespace LearningManagementSystem.Models.DTO.TestVM
{
   
        public class TakeTestVM
        {
            public Guid TestId { get; set; }
            public string TestTitle { get; set; }
        public Guid AttemptId { get; set; }


        public List<QuestionVM> Questions { get; set; } = new();
        }

        public class QuestionVM
        {
            public Guid QuestionId { get; set; }

            public string QuestionText { get; set; }

            public string OptionA { get; set; }

            public string OptionB { get; set; }

            public string OptionC { get; set; }

            public string OptionD { get; set; }
        }
    
}
