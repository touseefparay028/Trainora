using LearningManagementSystem.Models.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTO
{
    public class RegisterDTO
    {
        public Guid Id { get; set; }= Guid.NewGuid();
        [Required(ErrorMessage = "Name Can't be blank")]
        [DataType(DataType.Text)]   
        public string? Name { get; set; }



        [Required(ErrorMessage = "Email Can't be blank")]
        [EmailAddress(ErrorMessage = "Email should be in Proper Format")]
        [DataType(DataType.EmailAddress)]
        [Remote(action: "IsEmailRegisteredAlready", controller: "Admin", ErrorMessage = "Already taken")]
        public string? Email { get; set; }




        [Required(ErrorMessage = "Phone Can't be blank")]
        [RegularExpression("^[0-9]*$" ,ErrorMessage ="Number Only")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10,ErrorMessage ="Minimum Length should be 10")]
        public string? Phone { get; set; }



        [Required(ErrorMessage = "Password Can't be blank")]
        [DataType(DataType.Password)]
        public string? Password {  get; set; }


        [Required(ErrorMessage = "ConfirmPassword Can't be blank")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Password And ComparePassword Doesn't Match")]
        public string? ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Address Can't be blank")]

        [DataType(DataType.Text)]
        public string? Address { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? gender { get; set; }
        [Required(ErrorMessage = "Please enter Date Of Birth")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(StudentDateOfBirth), nameof(StudentDateOfBirth.ValidateDateOfBirth))]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Enrollment number is required.")]
        
        public int? EnrollmentNumber { get; set; }

        [Required(ErrorMessage = "Please enter your course")]
        public string? Course { get; set; }
        public Guid? BatchDMId { get; set; }
        public IEnumerable<SelectListItem>? BatchList { get; set; }
    }
}
