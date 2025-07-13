using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; } = Guid.NewGuid();

        [Required]
        public string CustomerName { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
    }
}
