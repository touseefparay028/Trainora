using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.ValidationAttributes
{
    public class FutureDateAttribute:ValidationAttribute
    {
        public FutureDateAttribute()
        {
            ErrorMessage = "Due time must be in the future.";
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true; // Let [Required] handle null case
            DateTime dateValue = (DateTime)value;
            return dateValue > DateTime.Now;
        }

    }
}
