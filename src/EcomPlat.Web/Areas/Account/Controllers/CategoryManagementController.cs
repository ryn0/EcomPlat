using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(StringConstants.AccountArea)]
    [Authorize]
    public class CategoryManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public CategoryManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
        }

        // GET: /Admin/Category
        public async Task<IActionResult> Index()
        {
            var categories = await this.context.Categories.ToListAsync();
            return this.View(categories);
        }

        // GET: /Admin/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var category = await this.context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return this.NotFound();
            }
            return this.View(category);
        }

        // GET: /Admin/Category/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (this.ModelState.IsValid)
            {
                category.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                category = this.Clean(category);
                this.context.Add(category);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(category);
        }

        // GET: /Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var category = await this.context.Categories.FindAsync(id);
            if (category == null)
            {
                return this.NotFound();
            }
            return this.View(category);
        }

        // POST: /Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return this.NotFound();
            }
            if (this.ModelState.IsValid)
            {
                try
                {
                    category.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    category = this.Clean(category);
                    this.context.Update(category);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.CategoryExists(category.CategoryId))
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
            return this.View(category);
        }

        // GET: /Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var category = await this.context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return this.NotFound();
            }
            return this.View(category);
        }

        // POST: /Admin/Category/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await this.context.Categories.FindAsync(id);
            this.context.Categories.Remove(category);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool CategoryExists(int id)
        {
            return this.context.Categories.Any(c => c.CategoryId == id);
        }


        private Category Clean(Category category)
        {
            category.Name = category.Name.Trim();
            category.CategoryKey = StringHelpers.UrlKey(category.CategoryKey);
            return category;
        }

    }
}
