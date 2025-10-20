using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.Domains
{
    public class ContactDM
    {
       public Guid id { get; set; }
        [Required]
            public string Name { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Message { get; set; }
     
    }
}
