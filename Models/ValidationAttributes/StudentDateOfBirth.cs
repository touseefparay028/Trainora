using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.ValidationAttributes
{
    public class StudentDateOfBirth:ValidationAttribute
    {
        public static ValidationResult ValidateDateOfBirth(DateTime? dob, ValidationContext context)
        {
            if (dob == null)
                return new ValidationResult("Date of Birth is required.");

            if (dob > DateTime.Now)
                return new ValidationResult("Date of Birth cannot be in the future.");

            var age = DateTime.Now.Year - dob.Value.Year;
            if (dob.Value.Date > DateTime.Now.AddYears(-age)) age--;

            if (age < 5)
                return new ValidationResult("Student must be at least 5 years old.");

            return ValidationResult.Success;
        }
    }
}
