using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Warranty
    {
        [Key]
        public Guid WarrantyId { get; set; } = Guid.NewGuid();

        [Required]
        public string RollNumber { get; set; }

        public int Status { get; set; } // -1 = registered, 1 = pending, 0 = valid

        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }

        // FK to Product
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        // FK to Customer
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        // FK to Dealer
        public Guid DealerId { get; set; }

        [ForeignKey("DealerId")]
        public Dealer Dealer { get; set; }
    }
}
