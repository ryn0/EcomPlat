using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(StringConstants.AccountArea)]
    [Authorize]
    public class InventoryManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public InventoryManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
        }

        // GET: /Account/InventoryManagement/Index
        public async Task<IActionResult> Index()
        {
            var inventoryItems = await this.context.ProductInventories
                .Include(pi => pi.Product)
                .Include(pi => pi.Supplier)
                .OrderBy(pi => pi.Product.Name)
                .ToListAsync();
            return this.View(inventoryItems);
        }

        // GET: /Account/InventoryManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var inventory = await this.context.ProductInventories
                .Include(pi => pi.Product)
                .Include(pi => pi.Supplier)
                .FirstOrDefaultAsync(pi => pi.ProductInventoryId == id);
            if (inventory == null)
            {
                return this.NotFound();
            }
            return this.View(inventory);
        }

        // GET: /Account/InventoryManagement/Create
        public async Task<IActionResult> Create()
        {
            await this.LoadDropDowns();
            return this.View();
        }

        // POST: /Account/InventoryManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInventory inventory)
        {
            if (this.ModelState.IsValid)
            {
                inventory.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                this.context.Add(inventory);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            await this.LoadDropDowns(inventory);
            return this.View(inventory);
        }

        // GET: /Account/InventoryManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var inventory = await this.context.ProductInventories.FindAsync(id);
            if (inventory == null)
            {
                return this.NotFound();
            }

            await this.LoadDropDowns(inventory);
            return this.View(inventory);
        }

        // POST: /Account/InventoryManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductInventory inventory)
        {
            if (id != inventory.ProductInventoryId)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    inventory.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    this.context.Update(inventory);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.ProductInventoryExists(inventory.ProductInventoryId))
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

            await this.LoadDropDowns(inventory);
            return this.View(inventory);
        }

        // GET: /Account/InventoryManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var inventory = await this.context.ProductInventories
                .Include(pi => pi.Product)
                .Include(pi => pi.Supplier)
                .FirstOrDefaultAsync(pi => pi.ProductInventoryId == id);
            if (inventory == null)
            {
                return this.NotFound();
            }
            return this.View(inventory);
        }

        // POST: /Account/InventoryManagement/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await this.context.ProductInventories.FindAsync(id);
            this.context.ProductInventories.Remove(inventory);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool ProductInventoryExists(int id)
        {
            return this.context.ProductInventories.Any(e => e.ProductInventoryId == id);
        }

        // Helper method to load all dropdowns
        private async Task LoadDropDowns(ProductInventory inventory = null)
        {
            await this.PopulateProductDropDownList(inventory?.ProductId);
            await this.PopulateSupplierDropDownList(inventory?.SupplierId);
            await this.PopulateCountryDropDownList(inventory?.Product?.CountryOfOrigin);
        }

        private async Task PopulateProductDropDownList(object selectedId = null)
        {
            var products = await this.context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            var list = products.Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = p.Name
            }).ToList();

            list.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });
            this.ViewData["ProductId"] = new SelectList(list, "Value", "Text", selectedId);
        }

        private async Task PopulateSupplierDropDownList(object selectedId = null)
        {
            var suppliers = await this.context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();

            var list = suppliers.Select(s => new SelectListItem
            {
                Value = s.SupplierId.ToString(),
                Text = s.Name
            }).ToList();

            list.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });
            this.ViewData["SupplierId"] = new SelectList(list, "Value", "Text", selectedId);
        }

        private async Task PopulateCountryDropDownList(object selectedId = null)
        {
            var countries = EcomPlat.Utilities.Helpers.CountryHelper.GetCountries();
            var list = countries.Select(c => new SelectListItem
            {
                Value = c.Key,
                Text = c.Value
            }).ToList();

            list.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });
            this.ViewData["CountryOrigin"] = new SelectList(list, "Value", "Text", selectedId);
            await Task.CompletedTask;
        }
    }
}
