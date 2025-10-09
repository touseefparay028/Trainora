using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.ValidationAttributes
{
    public class MaxFileSizeAttribute:ValidationAttribute
    {

        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
            ErrorMessage = $"Maximum allowed file size is {maxFileSize / (1024 * 1024)} MB.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}
