using EcomPlat.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Data.DbContextInfo
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<ApplicationUser> ApplicationUser { get; set; }
        DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }
        DbSet<BusinessDetails> BusinessDetails { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Company> Companies { get; set; }
        DbSet<ConfigSetting> ConfigSettings { get; set; }
        DbSet<OrderAddress> OrderAddresses { get; set; }
        DbSet<OrderItem> OrderItems { get; set; }
        DbSet<Order> Orders { get; set; }
        DbSet<InternalUsage> InternalUsages { get; set; }
        DbSet<ProductImage> ProductImages { get; set; }
        DbSet<ProductInventory> ProductInventories { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        DbSet<ShoppingCart> ShoppingCarts { get; set; }
        DbSet<Subcategory> Subcategories { get; set; }
        DbSet<Supplier> Suppliers { get; set; }
        DbSet<Warehouse> Warehouses { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}