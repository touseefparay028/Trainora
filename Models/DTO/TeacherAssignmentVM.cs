using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models.DTO
{
    public class TeacherAssignmentVM
    {
        public Guid Id { get; set; }= Guid.NewGuid();
        [Required(ErrorMessage ="Title can not be null")]
        public string Title { get; set; }
        
        [NotMapped]
        [Required(ErrorMessage ="File can not be null")]
        public IFormFile File { get; set; }
        public string? Path { get; set; }
        [Required(ErrorMessage = "Subject can not be null")]
        public string? Subject { get; set; }
        [Required(ErrorMessage = "Due Date and Time can not be null")]
        public DateTime DueTime { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public bool IsSubmitted { get; set; }
        [Required]
        public Guid BatchDMId { get; set; }
        public BatchDM? BatchDM { get; set; } 
        //public int TeacherDMId { get; set; }

        //public TeacherDM TeacherDM { get; set; }
        public IEnumerable<SelectListItem>? BatchList { get; set; }
    }
}
