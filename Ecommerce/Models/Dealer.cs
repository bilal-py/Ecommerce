using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Dealer
    {
        [Key]
        public Guid DealerId { get; set; } = Guid.NewGuid();

        [Required]
        public string DealerName { get; set; }

        public string Location { get; set; }

        [Required]
        public string Email { get; set; }

        public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
    }


}
