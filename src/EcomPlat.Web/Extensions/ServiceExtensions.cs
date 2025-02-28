using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using DirectoryManager.Web.Services.Implementations; // If needed
using DirectoryManager.Web.Services.Interfaces;
using EcomPlat.Data.Constants;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.FileStorage.Repositories.Implementations;
using EcomPlat.FileStorage.Repositories.Interfaces;
using EcomPlat.Shipping.Services.Implementaions;
using EcomPlat.Shipping.Services.Interfaces;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using any additional namespaces

namespace EcomPlat.Web.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // Register ApplicationDbContext (only once)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString(StringConstants.DefaultConnection)));

            // Register Identity (only once)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register common services
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddMvc();

            // Register custom repositories (ensure you have an extension method AddRepositories() defined)
            services.AddRepositories();

            // Register file storage repository (singleton)  
            services.AddSingleton<ISiteFilesRepository, SiteFilesRepository>();
            services.AddTransient<ICacheService, CacheService>();

            // Register BlobService singleton using async initialization (adjust as needed)
            services.AddSingleton<IBlobService>(provider =>
            {
                return Task.Run(async () =>
                {
                    using var scope = provider.CreateScope();
                    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    var azureStorageConnection = cacheService.GetSnippet(SiteConfigSetting.AzureStorageConnectionString);
                    var blobServiceClient = new BlobServiceClient(azureStorageConnection);
                    return await BlobService.CreateAsync(blobServiceClient);
                }).GetAwaiter().GetResult();
            });

            // Register BlobService singleton using async initialization (adjust as needed)
            services.AddTransient<IShippingCostCalculator>(provider =>
            {
                return Task.Run(() =>
                {
                    using var scope = provider.CreateScope();
                    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    var easyPostApiKey = cacheService.GetSnippet(SiteConfigSetting.EasyPostApiKey);

                    return new ShippingCostCalculator(easyPostApiKey);
                }).GetAwaiter().GetResult();
            });

            // Configure route options for lowercase URLs
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            return services;
        }
    }
}
