﻿using System.Linq;
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
    public class SupplierManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public SupplierManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // GET: /Account/SupplierManagement/Index
        public async Task<IActionResult> Index()
        {
            var suppliers = await this.context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(suppliers);
        }

        // GET: /Account/SupplierManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await this.context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        // GET: /Account/SupplierManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Account/SupplierManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                this.context.Add(supplier);
                await this.context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: /Account/SupplierManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await this.context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        // POST: /Account/SupplierManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.SupplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    supplier.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    this.context.Update(supplier);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.SupplierId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: /Account/SupplierManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await this.context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        // POST: /Account/SupplierManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await this.context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                this.context.Suppliers.Remove(supplier);
                await this.context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(int id)
        {
            return this.context.Suppliers.Any(e => e.SupplierId == id);
        }
    }
}
