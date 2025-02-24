using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.FileStorage.Repositories.Interfaces;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Controllers.Admin
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ISiteFilesRepository siteFilesRepository;

        public ProductsController(ApplicationDbContext context, ISiteFilesRepository siteFilesRepository)
        {
            this.context = context;
            this.siteFilesRepository = siteFilesRepository;
        }
        public async Task<IActionResult> Index()
        {
            var products = await this.context.Products
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .ToListAsync();
            return this.View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var product = await this.context.Products
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return this.NotFound();
            }
            return this.View(product);
        }

        public async Task<IActionResult> Create()
        {
            await this.PopulateSubcategoryDropDownList();
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFileCollection ProductImages)
        {
            if (this.ModelState.IsValid)
            {
                product.ProductKey = StringHelpers.UrlKey(product.Name);
                this.context.Add(product);
                await this.context.SaveChangesAsync();

                if (ProductImages != null && ProductImages.Count > 0)
                {
                    foreach (var file in ProductImages)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var imageUri = await this.siteFilesRepository.UploadAsync(file.OpenReadStream(), file.FileName, product.ProductKey + "/");
                            var productImage = new ProductImage
                            {
                                ImageUrl = imageUri.ToString(),
                                Size = EcomPlat.Data.Enums.ImageSize.Medium,
                                IsMain = false,
                                ProductId = product.ProductId
                            };
                            product.Images.Add(productImage);
                        }
                    }
                    await this.context.SaveChangesAsync();
                }
                return this.RedirectToAction(nameof(this.Index));
            }
            await this.PopulateSubcategoryDropDownList(product.SubcategoryId);
            return this.View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var product = await this.context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return this.NotFound();
            }
            await this.PopulateSubcategoryDropDownList(product.SubcategoryId);
            return this.View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFileCollection ProductImages)
        {
            if (id != product.ProductId)
            {
                return this.NotFound();
            }
            if (this.ModelState.IsValid)
            {
                try
                {
                    this.context.Update(product);
                    await this.context.SaveChangesAsync();

                    if (ProductImages != null && ProductImages.Count > 0)
                    {
                        foreach (var file in ProductImages)
                        {
                            if (file != null && file.Length > 0)
                            {
                                var imageUri = await this.siteFilesRepository.UploadAsync(file.OpenReadStream(), file.FileName, product.ProductKey + "/");
                                var productImage = new ProductImage
                                {
                                    ImageUrl = imageUri.ToString(),
                                    Size = EcomPlat.Data.Enums.ImageSize.Medium,
                                    IsMain = false,
                                    ProductId = product.ProductId
                                };
                                product.Images.Add(productImage);
                            }
                        }
                        await this.context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.ProductExists(product.ProductId))
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
            await this.PopulateSubcategoryDropDownList(product.SubcategoryId);
            return this.View(product);
        }

        private bool ProductExists(int id)
        {
            return this.context.Products.Any(e => e.ProductId == id);
        }

        private async Task PopulateSubcategoryDropDownList(object selectedId = null)
        {
            var subcategories = await this.context.Subcategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();

            var list = subcategories.Select(s => new
            {
                s.SubcategoryId,
                DisplayText = s.Category.Name + " > " + s.Name
            });

            this.ViewBag.SubCategoryId = new SelectList(list, "SubcategoryId", "DisplayText", selectedId);
        }

    }
}
