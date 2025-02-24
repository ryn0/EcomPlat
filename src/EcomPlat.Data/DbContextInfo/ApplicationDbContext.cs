using EcomPlat.Data.Models;
using EcomPlat.Data.Models.BaseModels;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Data.DbContextInfo
{
    public class ApplicationDbContext : ApplicationBaseContext<ApplicationDbContext>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUser { get; set;  }
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set;  }
        public DbSet<Category> Categories { get; set;  }
        public DbSet<ConfigSetting> ConfigSettings { get; set;  }
        public DbSet<OrderAddress> OrderAddresses { get; set;  }
        public DbSet<OrderItem> OrderItems { get; set;  }
        public DbSet<Order> Orders { get; set;  }
        public DbSet<ProductImage> ProductImages { get; set;  }
        public DbSet<ProductInventory> ProductInventories { get; set;  }
        public DbSet<Product> Products { get; set;  }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set;  }
        public DbSet<ShoppingCart> ShoppingCarts { get; set;  }
        public DbSet<Subcategory> Subcategories { get; set;  }
        public DbSet<Supplier> Suppliers { get; set;  }

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
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SalePrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ShippingAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ShippingWeightOunces)
                      .HasColumnType("decimal(18,2)");
            });

            // OrderItem decimal properties
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice)
                      .HasColumnType("decimal(18,2)");
            });

            // Product decimal properties
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SalePrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ProductWeightOunces)
                      .HasColumnType("decimal(28,8)");

                entity.Property(e => e.ShippingWeightOunces)
                      .HasColumnType("decimal(28,8)");

                entity.Property(e => e.HeightInches)
                      .HasColumnType("decimal(28,8)");

                entity.Property(e => e.WidthInches)
                      .HasColumnType("decimal(28,8)");

                entity.Property(e => e.LengthInches)
                      .HasColumnType("decimal(28,8)");
            });

            // ProductInventory decimal properties
            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.Property(e => e.PurchaseCost)
                      .HasColumnType("decimal(18,2)");
            });
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