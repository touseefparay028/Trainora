using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO
{
    public class AccountDeletionReason
    {
         public Guid id { get; set; }=Guid.NewGuid();
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 200 characters.")]
        public string Reason { get; set; }
        

    }
}
