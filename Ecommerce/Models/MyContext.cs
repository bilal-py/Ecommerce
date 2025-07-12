using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Models
{
    public class MyContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=motoprotekt;Trusted_Connection=True;");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Warranty>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Warranties)
                .HasForeignKey(w => w.ProductId)
                .IsRequired();

            modelBuilder.Entity<Warranty>()
                .HasOne(w => w.Customer)
                .WithMany(c => c.Warranties)
                .HasForeignKey(w => w.CustomerId);

            modelBuilder.Entity<Warranty>()
                .HasOne(w => w.Dealer)
                .WithMany(d => d.Warranties)
                .HasForeignKey(w => w.DealerId);
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
    }
}
