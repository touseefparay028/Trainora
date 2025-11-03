using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO.TestVM
{
    public class CreateTestVM
    {
        public Guid TestId { get; set; }
        [Required(ErrorMessage = "Test title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        [Display(Name = "Test Title")]
        public string Title { get; set; }

        [Display(Name = "Course")]
        public Guid? CourseId { get; set; }

        [Range(1, 500, ErrorMessage = "Total marks must be between 1 and 500.")]
        [Display(Name = "Total Marks")]
        public int? TotalMarks { get; set; }
    }
}
