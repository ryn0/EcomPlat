using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Repositories.Interfaces;

namespace EcomPlat.Web.Extensions
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Adds all database repositories to the service collection.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        /// <returns>IServiceCollection as extension.</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register ApplicationDbContext as IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IConfigSettingRepository, ConfigSettingRepository>();

            return services;
        }
    }
}