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
                .Include(pi => pi.Warehouse) // make sure Warehouse is included
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
                .Include(pi => pi.Warehouse)
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
                .Include(pi => pi.Warehouse)
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
            if (inventory != null)
            {
                this.context.ProductInventories.Remove(inventory);
                await this.context.SaveChangesAsync();
            }
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool ProductInventoryExists(int id)
        {
            return this.context.ProductInventories.Any(e => e.ProductInventoryId == id);
        }

        /// <summary>
        /// Loads dropdown lists for Products, Suppliers, and Warehouses.
        /// Inserts a default option "-- Select --" at the top.
        /// </summary>
        /// <param name="inventory">An optional ProductInventory instance to select default values.</param>
        private async Task LoadDropDowns(ProductInventory inventory = null)
        {
            var products = await this.context.Products.OrderBy(p => p.Name).ToListAsync();
            var suppliers = await this.context.Suppliers.OrderBy(s => s.Name).ToListAsync();
            var warehouses = await this.context.Warehouses.OrderBy(w => w.Name).ToListAsync();

            var productList = products.Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = p.Name
            }).ToList();
            productList.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });

            var supplierList = suppliers.Select(s => new SelectListItem
            {
                Value = s.SupplierId.ToString(),
                Text = s.Name
            }).ToList();
            supplierList.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });

            var warehouseList = warehouses.Select(w => new SelectListItem
            {
                Value = w.WarehouseId.ToString(),
                Text = w.Name
            }).ToList();
            warehouseList.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });

            this.ViewBag.ProductId = new SelectList(productList, "Value", "Text", inventory?.ProductId);
            this.ViewBag.SupplierId = new SelectList(supplierList, "Value", "Text", inventory?.SupplierId);
            this.ViewBag.WarehouseId = new SelectList(warehouseList, "Value", "Text", inventory?.WarehouseId);
        }
    }
}
