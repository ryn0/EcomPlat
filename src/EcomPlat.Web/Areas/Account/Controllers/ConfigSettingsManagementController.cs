using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area("Account")]
    [Authorize]
    public class ConfigSettingsManagementController : Controller
    {
        private readonly ApplicationDbContext context;

        public ConfigSettingsManagementController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /Admin/ConfigSettings
        public async Task<IActionResult> Index()
        {
            var settings = await this.context.ConfigSettings.ToListAsync();
            return this.View(settings);
        }

        // GET: /Admin/ConfigSettings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var configSetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(c => c.ConfigSettingId == id);
            if (configSetting == null)
            {
                return this.NotFound();
            }
            return this.View(configSetting);
        }

        // GET: /Admin/ConfigSettings/Create
        public IActionResult Create()
        {
            this.PopulateSiteConfigSettingDropDownList();
            return this.View();
        }

        // POST: /Admin/ConfigSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfigSetting configSetting)
        {
            if (this.ModelState.IsValid)
            {
                this.context.Add(configSetting);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }
            this.PopulateSiteConfigSettingDropDownList(configSetting.SiteConfigSetting);
            return this.View(configSetting);
        }

        // GET: /Admin/ConfigSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var configSetting = await this.context.ConfigSettings.FindAsync(id);
            if (configSetting == null)
            {
                return this.NotFound();
            }
            this.PopulateSiteConfigSettingDropDownList(configSetting.SiteConfigSetting);
            return this.View(configSetting);
        }

        // POST: /Admin/ConfigSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConfigSetting configSetting)
        {
            if (id != configSetting.ConfigSettingId)
            {
                return this.NotFound();
            }
            if (this.ModelState.IsValid)
            {
                try
                {
                    this.context.Update(configSetting);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.ConfigSettingExists(configSetting.ConfigSettingId))
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
            this.PopulateSiteConfigSettingDropDownList(configSetting.SiteConfigSetting);
            return this.View(configSetting);
        }

        // GET: /Admin/ConfigSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var configSetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(c => c.ConfigSettingId == id);
            if (configSetting == null)
            {
                return this.NotFound();
            }
            return this.View(configSetting);
        }

        // POST: /Admin/ConfigSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var configSetting = await this.context.ConfigSettings.FindAsync(id);
            this.context.ConfigSettings.Remove(configSetting);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool ConfigSettingExists(int id)
        {
            return this.context.ConfigSettings.Any(e => e.ConfigSettingId == id);
        }

        /// <summary>
        /// Populates the ViewBag with a SelectList of SiteConfigSetting values.
        /// </summary>
        /// <param name="selectedSetting">The selected setting, if any.</param>
        private void PopulateSiteConfigSettingDropDownList(object selectedSetting = null)
        {
            var values = Enum.GetValues(typeof(SiteConfigSetting))
                .Cast<SiteConfigSetting>()
                .Select(s => new
                {
                    Id = s,
                    Name = s.ToString()
                })
                .ToList();

            this.ViewBag.SiteConfigSetting = new SelectList(values, "Id", "Name", selectedSetting);
        }
    }
}
