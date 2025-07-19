using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public string Status { get; set; } = "Active";

        [NotMapped]
        public List<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "A", Text = "Prime" },
            new SelectListItem { Value = "B", Text = "Ultimate" },
            new SelectListItem { Value = "C", Text = "Ultimate Plus" }
        };
    }
}
