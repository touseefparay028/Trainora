using System.ComponentModel.DataAnnotations;
namespace LearningManagementSystem.Models.DTO
{
    public class LoginDTO
    {
        public Guid Id { get; set; }
        //[Required(ErrorMessage = "Email Can't be blank")]
        //[EmailAddress(ErrorMessage = "Email should be in Proper Format")]
        //[DataType(DataType.EmailAddress)]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Password Can't be blank")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }=false;

    }
}
