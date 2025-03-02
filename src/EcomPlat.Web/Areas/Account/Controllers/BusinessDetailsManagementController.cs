using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(Constants.StringConstants.AccountArea)]
    [Authorize]
    public class BusinessDetailsManagementController : Controller
    {
        private readonly ApplicationDbContext context;

        public BusinessDetailsManagementController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /Account/BusinessDetailsManagement
        public async Task<IActionResult> Index()
        {
            var list = await this.context.BusinessDetails.ToListAsync();
            return this.View(list);
        }

        // GET: /Account/BusinessDetailsManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var business = await this.context.BusinessDetails
                .FirstOrDefaultAsync(b => b.BusinessDetailsId == id);
            if (business == null)
            {
                return this.NotFound();
            }
            return this.View(business);
        }

        // GET: /Account/BusinessDetailsManagement/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: /Account/BusinessDetailsManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessDetailsId,Name,AddressLine1,AddressLine2,City,StateRegion,PostalCode,CountryIso,Phone,Email,AddressType")] BusinessDetails businessDetails)
        {
            if (this.ModelState.IsValid)
            {
                this.context.Add(businessDetails);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }
            return this.View(businessDetails);
        }

        // GET: /Account/BusinessDetailsManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var businessDetails = await this.context.BusinessDetails.FindAsync(id);
            if (businessDetails == null)
            {
                return this.NotFound();
            }
            return this.View(businessDetails);
        }

        // POST: /Account/BusinessDetailsManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BusinessDetailsId,Name,AddressLine1,AddressLine2,City,StateRegion,PostalCode,CountryIso,Phone,Email,AddressType")] BusinessDetails businessDetails)
        {
            if (id != businessDetails.BusinessDetailsId)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    this.context.Update(businessDetails);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.BusinessDetailsExists(businessDetails.BusinessDetailsId))
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
            return this.View(businessDetails);
        }

        // GET: /Account/BusinessDetailsManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var businessDetails = await this.context.BusinessDetails
                .FirstOrDefaultAsync(b => b.BusinessDetailsId == id);
            if (businessDetails == null)
            {
                return this.NotFound();
            }
            return this.View(businessDetails);
        }

        // POST: /Account/BusinessDetailsManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var businessDetails = await this.context.BusinessDetails.FindAsync(id);
            this.context.BusinessDetails.Remove(businessDetails);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool BusinessDetailsExists(int id)
        {
            return this.context.BusinessDetails.Any(e => e.BusinessDetailsId == id);
        }
    }
}