using Microsoft.EntityFrameworkCore;

namespace chapter1
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().ToTable("Items");
            modelBuilder.Entity<Item>().HasKey(p => p.Barcode);

            modelBuilder.Entity<Item>().HasData(new Item { Barcode = "123", Title = "Headphone", SellingPrice = 50 });
            modelBuilder.Entity<Item>().HasData(new Item { Barcode = "456", Title = "Keyboard", SellingPrice = 40 });
            modelBuilder.Entity<Item>().HasData(new Item { Barcode = "789", Title = "Monitor", SellingPrice = 100 });


            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Item> Items { get; set; }
    }
}
