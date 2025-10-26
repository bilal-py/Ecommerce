using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ToEmail { get; set; }

       //store them as a comma or semicolon-separated string.
        [MaxLength(500)]
        public string? Cc { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; }

        public string Body { get; set; }

        public bool EmailSent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? WarrantyId { get; set; }
    }
}
