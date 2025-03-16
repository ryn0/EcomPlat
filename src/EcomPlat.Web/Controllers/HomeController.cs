using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcomPlat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ApplicationDbContext context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            var recentProducts = await this.context.Products
                .Include(p => p.Images.Where(i => i.IsMain))
                .Where(p => p.IsAvailable)
                .OrderByDescending(p => p.CreateDate) // Sort by newest first
                .Take(8)
                .ToListAsync();

            var randomProducts = await this.context.Products
                .Include(p => p.Images.Where(i => i.IsMain))
                .Where(p => p.IsAvailable)
                .OrderBy(p => Guid.NewGuid()) // Random order
                .Take(8)
                .ToListAsync();

            var model = new HomePageViewModel
            {
                RecentProducts = recentProducts,
                RandomProducts = randomProducts
            };

            return this.View(model);
        }

        [HttpGet("/faq")]
        public async Task<IActionResult> FAQ()
        {
            var faqContent = await this.context.ConfigSettings
                .Where(cs => cs.SiteConfigSetting == SiteConfigSetting.FAQContentHtml)
                .Select(cs => cs.Content)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(faqContent))
            {
                faqContent = "<p>FAQ content is not available at this time.</p>";
            }

            this.ViewData["Title"] = "Frequently Asked Questions";
            return this.View("ContentPage", faqContent);
        }

        [HttpGet("/terms")]
        public async Task<IActionResult> Terms()
        {
            var termsContent = await this.context.ConfigSettings
                .Where(cs => cs.SiteConfigSetting == SiteConfigSetting.TermsContentHtml)
                .Select(cs => cs.Content)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(termsContent))
            {
                termsContent = "<p>Terms & Conditions content is not available at this time.</p>";
            }

            this.ViewData["Title"] = "Terms & Conditions";
            return this.View("ContentPage", termsContent);
        }

        [HttpGet("/contact")]
        public async Task<IActionResult> Contact()
        {
            var termsContent = await this.context.ConfigSettings
                .Where(cs => cs.SiteConfigSetting == SiteConfigSetting.ContactContentHtml)
                .Select(cs => cs.Content)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(termsContent))
            {
                termsContent = "<p>Contact content is not available at this time.</p>";
            }

            this.ViewData["Title"] = "Contact";
            return this.View("ContentPage", termsContent);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
