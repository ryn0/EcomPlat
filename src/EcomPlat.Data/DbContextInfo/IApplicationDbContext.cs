using EcomPlat.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Data.DbContextInfo
{
    public interface IApplicationDbContext : IDisposable
    {
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}