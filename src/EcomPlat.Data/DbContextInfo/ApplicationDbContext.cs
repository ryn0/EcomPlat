using EcomPlat.Data.Models;
using EcomPlat.Data.Models.BaseModels;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Data.DbContextInfo
{
    public class ApplicationDbContext : ApplicationBaseContext<ApplicationDbContext>, IApplicationDbContext
    {
        private const string SizeOfPriceDecimal = "decimal(18,2)";
        private const string SizeOfWeightDecimal = "decimal(28,8)";
        private const string SizeOfReviewDecimal = "decimal(3,2)";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUser { get; set;  }
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set;  }
        public DbSet<BusinessDetails> BusinessDetails { get; set; }
        public DbSet<Category> Categories { get; set;  }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ConfigSetting> ConfigSettings { get; set;  }
        public DbSet<InternalUsage> InternalUsages { get; set; }
        public DbSet<OrderAddress> OrderAddresses { get; set;  }
        public DbSet<OrderItem> OrderItems { get; set;  }
        public DbSet<Order> Orders { get; set;  }
        public DbSet<ProductImage> ProductImages { get; set;  }
        public DbSet<ProductInventory> ProductInventories { get; set;  }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Product> Products { get; set;  }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set;  }
        public DbSet<ShoppingCart> ShoppingCarts { get; set;  }
        public DbSet<Subcategory> Subcategories { get; set;  }
        public DbSet<Supplier> Suppliers { get; set;  }
        public DbSet<Warehouse> Warehouses { get; set; }

        public override int SaveChanges()
        {
            this.SetDates();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            this.SetDates();

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Product and Subcategory.
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Subcategory)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SubcategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order decimal properties
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.OrderTotal)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.SalePrice)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.ShippingAmount)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.ShippingWeightOunces)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.SalePrice)
                .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.OutcomeAmount)
                .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.PaidAmount)
                .HasColumnType(SizeOfPriceDecimal);
 

            });

            // OrderItem decimal properties
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.TotalPrice)
                       .HasColumnType(SizeOfPriceDecimal);
            });


            // Product decimal properties
            modelBuilder.Entity<Product>(entity =>
            {

                entity.Property(e => e.Price)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.SalePrice)
                      .HasColumnType(SizeOfPriceDecimal);

                entity.Property(e => e.ProductWeightOunces)
                      .HasColumnType(SizeOfWeightDecimal);

                entity.Property(e => e.ShippingWeightOunces)
                      .HasColumnType(SizeOfWeightDecimal);

                entity.Property(e => e.HeightInches)
                      .HasColumnType(SizeOfWeightDecimal);

                entity.Property(e => e.WidthInches)
                      .HasColumnType(SizeOfWeightDecimal);

                entity.Property(e => e.LengthInches)
                      .HasColumnType(SizeOfWeightDecimal);

                entity.Property(e => e.ProductReview)
                    .HasColumnType(SizeOfReviewDecimal);
            });

            // ProductInventory decimal properties
            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.Property(e => e.PurchaseCost)
                      .HasColumnType(SizeOfPriceDecimal);
            });

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.DisplayOrder, pi.ImageGroupGuid })
                .IsUnique()
                .HasFilter("[Size] = 1"); // assuming ImageSize.Small is 1

            modelBuilder.Entity<Company>().ToTable("Companies");

            modelBuilder.Entity<Product>()
                        .HasIndex(p => p.ProductKey)
                        .IsUnique();

            modelBuilder.Entity<Company>()
              .HasIndex(p => p.Name)
              .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(p => p.CategoryKey)
                .IsUnique();

            modelBuilder.Entity<Subcategory>()
                .HasIndex(p => p.SubcategoryKey)
                .IsUnique();
        }

        private void SetDates()
        {
            foreach (var entry in this.ChangeTracker.Entries()
                .Where(x => (x.Entity is StateInfo) && x.State == EntityState.Added)
                .Select(x => (StateInfo)x.Entity))
            {
                if (entry.CreateDate == DateTime.MinValue)
                {
                    entry.CreateDate = DateTime.UtcNow;
                }
            }

            foreach (var entry in this.ChangeTracker.Entries()
                .Where(x => x.Entity is CreatedStateInfo && x.State == EntityState.Added)
                .Select(x => (CreatedStateInfo)x.Entity)
                .Where(x => x != null))
            {
                if (entry.CreateDate == DateTime.MinValue)
                {
                    entry.CreateDate = DateTime.UtcNow;
                }
            }

            foreach (var entry in this.ChangeTracker.Entries()
                .Where(x => x.Entity is StateInfo && x.State == EntityState.Modified)
                .Select(x => (StateInfo)x.Entity)
                .Where(x => x != null))
            {
                entry.UpdateDate = DateTime.UtcNow;
            }
        }
    }
}