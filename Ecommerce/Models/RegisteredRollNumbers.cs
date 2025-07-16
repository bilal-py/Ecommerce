using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class RegisteredRollNumbers
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Roll Number is required")]
        public string RollNumber { get; set; }
        public string Category { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; }
    }
}
