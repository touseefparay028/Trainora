using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO.TestVM
{
    public class AddQuestionVM
    {
        [Required(ErrorMessage = "Test reference is required.")]
        public Guid TestId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(500, ErrorMessage = "Question cannot exceed 500 characters.")]
        [Display(Name = "Question")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Option A is required.")]
        [StringLength(200, ErrorMessage = "Option cannot exceed 200 characters.")]
        [Display(Name = "Option A")]
        public string OptionA { get; set; }

        [Required(ErrorMessage = "Option B is required.")]
        [StringLength(200, ErrorMessage = "Option cannot exceed 200 characters.")]
        [Display(Name = "Option B")]
        public string OptionB { get; set; }

        [Required(ErrorMessage = "Option C is required.")]
        [StringLength(200, ErrorMessage = "Option cannot exceed 200 characters.")]
        [Display(Name = "Option C")]
        public string OptionC { get; set; }

        [Required(ErrorMessage = "Option D is required.")]
        [StringLength(200, ErrorMessage = "Option cannot exceed 200 characters.")]
        [Display(Name = "Option D")]
        public string OptionD { get; set; }

        [Required(ErrorMessage = "You must select the correct answer.")]
        [RegularExpression("^(A|B|C|D)$", ErrorMessage = "Correct answer must be A, B, C, or D.")]
        [Display(Name = "Correct Answer (A/B/C/D)")]
        public string CorrectAnswer { get; set; }
        [Required(ErrorMessage = "Marks is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Marks must be at least 1.")]
        public int Marks { get; set; }

    }
}
