using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(StringConstants.AccountArea)]
    [Authorize]
    public class WarehouseManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public WarehouseManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // GET: /Account/WarehouseManagement/Index
        public async Task<IActionResult> Index()
        {
            var warehouses = await this.context.Warehouses
                .OrderBy(w => w.Name)
                .ToListAsync();
            return this.View(warehouses);
        }

        // GET: /Account/WarehouseManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var warehouse = await this.context.Warehouses
                .FirstOrDefaultAsync(w => w.WarehouseId == id);
            if (warehouse == null)
            {
                return this.NotFound();
            }

            return this.View(warehouse);
        }

        // GET: /Account/WarehouseManagement/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: /Account/WarehouseManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            if (this.ModelState.IsValid)
            {
                warehouse.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                this.context.Add(warehouse);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(warehouse);
        }

        // GET: /Account/WarehouseManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var warehouse = await this.context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return this.NotFound();
            }

            return this.View(warehouse);
        }

        // POST: /Account/WarehouseManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Warehouse warehouse)
        {
            if (id != warehouse.WarehouseId)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    warehouse.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    this.context.Update(warehouse);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.WarehouseExists(warehouse.WarehouseId))
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

            return this.View(warehouse);
        }

        // GET: /Account/WarehouseManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var warehouse = await this.context.Warehouses
                .FirstOrDefaultAsync(w => w.WarehouseId == id);
            if (warehouse == null)
            {
                return this.NotFound();
            }

            return this.View(warehouse);
        }

        // POST: /Account/WarehouseManagement/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await this.context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                this.context.Warehouses.Remove(warehouse);
                await this.context.SaveChangesAsync();
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        private bool WarehouseExists(int id)
        {
            return this.context.Warehouses.Any(w => w.WarehouseId == id);
        }
    }
}
