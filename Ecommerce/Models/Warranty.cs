using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Warranty
    {
        [Key]
        public Guid WarrantyId { get; set; } = Guid.NewGuid();

        public string RollNumber { get; set; }
        public int Status { get; set; }

        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }

        public string VehicleYear { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleVIN { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }

        public Guid DealerId { get; set; }
        public Dealer Dealer { get; set; }

        public bool BumpersFront { get; set; }
        public bool HoodLead { get; set; }
        public bool Mirrors { get; set; }
        public bool BumpersBack { get; set; }
        public bool EdgeGuard { get; set; }
        public bool Windshield { get; set; }
        public bool FendersLead { get; set; }
        public bool RoofFull { get; set; }
        public bool HoodFull { get; set; }
        public bool RoofLead { get; set; }
        public bool Headlamps { get; set; }
        public bool Trunk { get; set; }
    }
}
