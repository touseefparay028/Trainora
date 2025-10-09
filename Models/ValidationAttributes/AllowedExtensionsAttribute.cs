using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.ValidationAttributes
{
    public class AllowedExtensionsAttribute:ValidationAttribute
    {
         private readonly string[] _extensions;
    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
        ErrorMessage = $"Only the following file types are allowed: {string.Join(", ", extensions)}";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult(ErrorMessage);
            }
        }
        return ValidationResult.Success;
    }
    }
}
