using Microsoft.EntityFrameworkCore;
using Ecommerce.Models;

namespace Ecommerce.Models
{
    public class MyContext : DbContext
    {
        // Constructor used by ASP.NET DI container
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Warranty>()
                .HasOne(w => w.Customer)
                .WithMany(c => c.Warranties)
                .HasForeignKey(w => w.CustomerId);

            modelBuilder.Entity<Warranty>()
                .HasOne(w => w.Dealer)
                .WithMany(d => d.Warranties)
                .HasForeignKey(w => w.DealerId);
            modelBuilder.Entity<Warranty>()
                .HasOne(w => w.RegisteredRollNumber)
                .WithMany()
                .HasForeignKey(w => w.RollNumber)
                .HasPrincipalKey(r => r.RollNumber); // important: this is NOT the PK but an alternate key

            modelBuilder.Entity<RegisteredRollNumbers>()
                .HasAlternateKey(r => r.RollNumber);

        }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<Ecommerce.Models.RegisteredRollNumbers> RegisteredRollNumber { get; set; } = default!;
        public DbSet<EmailLog> EmailLogs { get; set; }
    }
}
