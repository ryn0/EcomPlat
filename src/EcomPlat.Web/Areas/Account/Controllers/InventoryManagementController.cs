using EcomPlat.Data;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(Constants.StringConstants.AccountArea)]
    [Authorize]
    public class InventoryManagementController : Controller
    {
        private readonly ApplicationDbContext context;

        public InventoryManagementController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /Admin/Inventory
        public async Task<IActionResult> Index()
        {
            var inventoryItems = await this.context.ProductInventories
                .Include(pi => pi.Product)
                .Include(pi => pi.Supplier)
                .ToListAsync();
            return this.View(inventoryItems);
        }

        // GET: /Admin/Inventory/Details/5
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

        // GET: /Admin/Inventory/Create
        public async Task<IActionResult> Create()
        {
            this.ViewData["ProductId"] = new SelectList(await this.context.Products.ToListAsync(), "ProductId", "Name");
            this.ViewData["SupplierId"] = new SelectList(await this.context.Suppliers.ToListAsync(), "SupplierId", "Name");
            return this.View();
        }

        // POST: /Admin/Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInventory inventory)
        {
            if (this.ModelState.IsValid)
            {
                this.context.Add(inventory);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            this.ViewData["ProductId"] = new SelectList(await this.context.Products.ToListAsync(), "ProductId", "Name", inventory.ProductId);
            this.ViewData["SupplierId"] = new SelectList(await this.context.Suppliers.ToListAsync(), "SupplierId", "Name", inventory.SupplierId);
            return this.View(inventory);
        }

        // GET: /Admin/Inventory/Edit/5
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

            this.ViewData["ProductId"] = new SelectList(await this.context.Products.ToListAsync(), "ProductId", "Name", inventory.ProductId);
            this.ViewData["SupplierId"] = new SelectList(await this.context.Suppliers.ToListAsync(), "SupplierId", "Name", inventory.SupplierId);
            return this.View(inventory);
        }

        // POST: /Admin/Inventory/Edit/5
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

            this.ViewData["ProductId"] = new SelectList(await this.context.Products.ToListAsync(), "ProductId", "Name", inventory.ProductId);
            this.ViewData["SupplierId"] = new SelectList(await this.context.Suppliers.ToListAsync(), "SupplierId", "Name", inventory.SupplierId);
            return this.View(inventory);
        }

        // GET: /Admin/Inventory/Delete/5
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

        // POST: /Admin/Inventory/Delete/5
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
    }
}
