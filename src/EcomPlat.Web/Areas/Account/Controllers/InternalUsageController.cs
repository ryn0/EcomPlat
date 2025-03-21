using EcomPlat.Data.DbContextInfo;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(StringConstants.AccountArea)]
    public class InternalUsageController : Controller
    {
        private const int PageSize = 20;
        private readonly ApplicationDbContext context;

        public InternalUsageController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var totalCount = await this.context.InternalUsages.CountAsync();
            var usageRecords = await this.context.InternalUsages
                .Include(u => u.Product)
                .OrderByDescending(u => u.UsageDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            this.ViewData["CurrentPage"] = page;
            this.ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / PageSize);

            return this.View(usageRecords);
        }

        public async Task<IActionResult> Create()
        {
            await this.PopulateProductList();
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Data.Models.InternalUsage usage)
        {
            if (this.ModelState.IsValid)
            {
                this.context.InternalUsages.Add(usage);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }
            await this.PopulateProductList();
            return this.View(usage);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usage = await this.context.InternalUsages.FindAsync(id);
            if (usage == null)
            {
                return this.NotFound();
            }
            await this.PopulateProductList();
            return this.View(usage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Data.Models.InternalUsage usage)
        {
            if (id != usage.InternalUsageId)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                this.context.Update(usage);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            await this.PopulateProductList();
            return this.View(usage);
        }

        private async Task PopulateProductList()
        {
            var products = await this.context.Products.OrderBy(p => p.Name).ToListAsync();
            this.ViewBag.Products = new SelectList(products, "ProductId", "Name");
        }
    }
}
