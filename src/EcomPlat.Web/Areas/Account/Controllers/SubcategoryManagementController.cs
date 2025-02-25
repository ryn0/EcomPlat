using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(Constants.StringConstants.AccountArea)]
    [Authorize]
    public class SubcategoryManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public SubcategoryManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
        }

        // GET: /Admin/Subcategory
        public async Task<IActionResult> Index()
        {
            var subcategories = await this.context.Subcategories
                .Include(s => s.Category)
                .ToListAsync();
            return this.View(subcategories);
        }

        // GET: /Admin/Subcategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var subcategory = await this.context.Subcategories
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.SubcategoryId == id);
            if (subcategory == null)
            {
                return this.NotFound();
            }
            return this.View(subcategory);
        }

        // GET: /Admin/Subcategory/Create
        public async Task<IActionResult> Create()
        {
            this.ViewData["CategoryId"] = new SelectList(await this.context.Categories.ToListAsync(), "CategoryId", "Name");
            return this.View();
        }

        // POST: /Admin/Subcategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subcategory subcategory)
        {
            if (this.ModelState.IsValid)
            {
                subcategory.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                subcategory = this.Clean(subcategory);
                this.context.Add(subcategory);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            this.ViewData["CategoryId"] = new SelectList(await this.context.Categories.ToListAsync(), "CategoryId", "Name", subcategory.CategoryId);
            return this.View(subcategory);
        }

        // GET: /Admin/Subcategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var subcategory = await this.context.Subcategories.FindAsync(id);
            if (subcategory == null)
            {
                return this.NotFound();
            }
            this.ViewData["CategoryId"] = new SelectList(await this.context.Categories.ToListAsync(), "CategoryId", "Name", subcategory.CategoryId);
            return this.View(subcategory);
        }

        // POST: /Admin/Subcategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subcategory subcategory)
        {
            if (id != subcategory.SubcategoryId)
            {
                return this.NotFound();
            }
            if (this.ModelState.IsValid)
            {
                try
                {
                    subcategory.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    subcategory = this.Clean(subcategory);
                    this.context.Update(subcategory);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.SubcategoryExists(subcategory.SubcategoryId))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return this.RedirectToAction(nameof(this.Index));
            }
            this.ViewData["CategoryId"] = new SelectList(await this.context.Categories.ToListAsync(), "CategoryId", "Name", subcategory.CategoryId);
            return this.View(subcategory);
        }

        // GET: /Admin/Subcategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var subcategory = await this.context.Subcategories
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.SubcategoryId == id);
            if (subcategory == null)
            {
                return this.NotFound();
            }
            return this.View(subcategory);
        }

        // POST: /Admin/Subcategory/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subcategory = await this.context.Subcategories.FindAsync(id);
            this.context.Subcategories.Remove(subcategory);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool SubcategoryExists(int id)
        {
            return this.context.Subcategories.Any(s => s.SubcategoryId == id);
        }

        private Subcategory Clean(Subcategory subcategory)
        {
            subcategory.Name = subcategory.Name.Trim();
            subcategory.SubcategoryKey = StringHelpers.UrlKey(subcategory.Name);
            return subcategory;
        }

    }
}
