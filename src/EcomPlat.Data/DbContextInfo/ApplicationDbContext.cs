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

        public DbSet<ApplicationUser> ApplicationUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DbSet<ApplicationUserRole> ApplicationUserRole { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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