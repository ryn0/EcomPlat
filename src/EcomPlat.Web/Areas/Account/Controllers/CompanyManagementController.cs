using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(Constants.StringConstants.AccountArea)]
    [Authorize]
    public class CompanyManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public CompanyManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
        }

        // GET: /Account/CompanyManagement/Index
        public async Task<IActionResult> Index()
        {
            var companies = await this.context.Companies.ToListAsync();
            return this.View(companies);
        }

        // GET: /Account/CompanyManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var company = await this.context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == id);
            if (company == null)
            {
                return this.NotFound();
            }
            return this.View(company);
        }

        // GET: /Account/CompanyManagement/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: /Account/CompanyManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (this.ModelState.IsValid)
            {
                company.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                this.context.Add(company);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }
            return this.View(company);
        }

        // GET: /Account/CompanyManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var company = await this.context.Companies.FindAsync(id);
            if (company == null)
            {
                return this.NotFound();
            }
            return this.View(company);
        }

        // POST: /Account/CompanyManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company)
        {
            if (id != company.CompanyId)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    company.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    this.context.Update(company);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.CompanyExists(company.CompanyId))
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
            return this.View(company);
        }

        // GET: /Account/CompanyManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var company = await this.context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == id);
            if (company == null)
            {
                return this.NotFound();
            }
            return this.View(company);
        }

        // POST: /Account/CompanyManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await this.context.Companies.FindAsync(id);
            this.context.Companies.Remove(company);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool CompanyExists(int id)
        {
            return this.context.Companies.Any(c => c.CompanyId == id);
        }
    }
}
