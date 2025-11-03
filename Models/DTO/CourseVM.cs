using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.DTO
{
    public class CourseVM
    {
        [Key]
        public Guid Id { get; set; }= Guid.NewGuid();

        [Required(ErrorMessage ="Title is required")]
        [StringLength(50, MinimumLength =5, ErrorMessage ="Title must be between 5 and 50 characters")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Description { get; set; }

        
        public Guid TeacherId { get; set; }  // From Identity user
        public ApplicationUser? Teacher { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public string? TeacherName { get; set; }
        public List<RegisterDTO>? Students { get; set; }
        public int EnrolledStudentsCount { get; set; }
        [Required(ErrorMessage = "Please select a batch.")]
        public Guid? BatchId { get; set; }  // For course associated with a batch
        public BatchDM? Batch { get; set; }
        // Navigation
        public ICollection<StudentCourseDM>? Enrollments { get; set; }
        public ICollection<TimeTableDM>? TimeTables { get; set; }
        public IEnumerable<SelectListItem>? TeacherList { get; set; }
        public IEnumerable<SelectListItem>? BatchList { get; set; } // for course creation with batch
    }
}
