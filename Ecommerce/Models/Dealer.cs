using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Dealer
    {
        [Key]
        public Guid DealerId { get; set; } = Guid.NewGuid();

        [Required]
        public string DealerName { get; set; }
        [Phone]
        public string? Contact { get; set; }
        public string? Address { get; set; }
        public string? GSTNumber { get; set; }
        public string? FirmName { get; set; }

        [Required]
        public string Email { get; set; }

        public ICollection<Warranty>? Warranties { get; set; } = new List<Warranty>();
    }


}
