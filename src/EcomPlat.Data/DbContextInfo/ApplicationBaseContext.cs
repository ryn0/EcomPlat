using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Data.DbContextInfo
{
    public class ApplicationBaseContext<TContext> : IdentityDbContext<ApplicationUser>
        where TContext : ApplicationBaseContext<TContext>
    {
        public ApplicationBaseContext(DbContextOptions<TContext> options)
            : base(options)
        {
        }
    }
}