using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; } = Guid.NewGuid();

        public string CustomerName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
    }

}
