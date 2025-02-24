using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.FileStorage.Repositories.Interfaces;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area("Account")]
    [Authorize]
    public class ProductsManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ISiteFilesRepository siteFilesRepository;

        public ProductsManagementController(ApplicationDbContext context, ISiteFilesRepository siteFilesRepository)
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

                await this.ProcessProductImageUploads(product, ProductImages);
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

                    await this.ProcessProductImageUploads(product, ProductImages);
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

        public async Task<IActionResult> Delete(int? id)
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

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await this.context.Products.FindAsync(id);
            this.context.Products.Remove(product);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var productImage = await this.context.ProductImages.FindAsync(imageId);
            if (productImage != null)
            {
                await this.siteFilesRepository.DeleteFileAsync(productImage.ImageUrl);
                this.context.ProductImages.Remove(productImage);
                await this.context.SaveChangesAsync();
            }
            return this.RedirectToAction("Edit", new { id = productId });
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

        /// <summary>
        /// Processes file uploads for a product and adds corresponding ProductImage records.
        /// Files are uploaded to the path: /products/{ProductId}/{filename}_{ImageSize}{extension}
        /// and a sequential DisplayOrder is assigned.
        /// </summary>
        /// <param name="product">The product to associate images with.</param>
        /// <param name="productImages">The collection of files to upload.</param>
        private async Task ProcessProductImageUploads(Product product, IFormFileCollection productImages)
        {
            if (productImages == null || productImages.Count == 0)
            {
                return;
            }

            // Determine the current maximum display order for this product.
            int currentMaxOrder = 0;
            if (product.Images != null && product.Images.Any())
            {
                currentMaxOrder = product.Images.Max(i => i.DisplayOrder);
            }
            else
            {
                // Optionally, check the database in case Images are not loaded.
                var existingImages = await this.context.ProductImages
                    .Where(pi => pi.ProductId == product.ProductId)
                    .ToListAsync();
                if (existingImages.Any())
                {
                    currentMaxOrder = existingImages.Max(pi => pi.DisplayOrder);
                }
            }

            foreach (var file in productImages)
            {
                if (file != null && file.Length > 0)
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    var sizeString = Data.Enums.ImageSize.Medium.ToString();
                    var finalFileName = $"{fileNameWithoutExtension}_{sizeString}{extension}";
                    var directory = $"products/{product.ProductId}/";

                    var imageUri = await this.siteFilesRepository.UploadAsync(file.OpenReadStream(), finalFileName, directory);
                    // Increment the display order
                    currentMaxOrder++;
                    var productImage = new ProductImage
                    {
                        ImageUrl = imageUri.ToString(),
                        Size = Data.Enums.ImageSize.Medium,
                        IsMain = false,
                        ProductId = product.ProductId,
                        DisplayOrder = currentMaxOrder
                    };

                    product.Images.Add(productImage);
                }
            }

            await this.context.SaveChangesAsync();
        }
    }
}
