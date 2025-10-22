using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO
{
    public class ContactVM
    {
        public Guid id { get; set; }
        [Required(ErrorMessage = "Name Can't be blank")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 500 characters.")]
        public string Message { get; set; }
    }
}
