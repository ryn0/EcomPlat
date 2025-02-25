using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.FileStorage.Repositories.Interfaces;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(Constants.StringConstants.AccountArea)]
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
                .Include(p => p.Company)
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
                .Include(p => p.Company)
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
            await this.PopulateCompanyDropDownList();
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
            await this.PopulateCompanyDropDownList(product.CompanyId);
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
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return this.NotFound();
            }

            await this.PopulateSubcategoryDropDownList(product.SubcategoryId);
            await this.PopulateCompanyDropDownList(product.CompanyId);
            return this.View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFileCollection productImages)
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

                    await this.ProcessProductImageUploads(product, productImages);
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
            await this.PopulateCompanyDropDownList(product.CompanyId);
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
                .Include(p => p.Company)
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
                Guid groupGuid = productImage.ImageGroupGuid;
                var imagesToDelete = await this.context.ProductImages
                    .Where(pi => pi.ProductId == productId && pi.ImageGroupGuid == groupGuid)
                    .ToListAsync();
                foreach (var img in imagesToDelete)
                {
                    await this.siteFilesRepository.DeleteFileAsync(img.ImageUrl);
                    this.context.ProductImages.Remove(img);
                }
                await this.context.SaveChangesAsync();
            }
            return this.RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int imageId, int productId)
        {
            // Find the chosen image.
            var chosenImage = await this.context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId && pi.ProductId == productId);
            if (chosenImage == null)
            {
                return this.NotFound();
            }

            // Get the group Guid of the chosen image.
            Guid chosenGroupGuid = chosenImage.ImageGroupGuid;

            // For this product, update all images:
            // Mark as main if they belong to the chosen group; otherwise, mark as not main.
            var allImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
            foreach (var img in allImages)
            {
                img.IsMain = img.ImageGroupGuid == chosenGroupGuid;
            }
            await this.context.SaveChangesAsync();

            return this.RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveImageUp(int imageId, int productId)
        {
            // (Move up logic unchanged from previous implementation)
            var image = await this.context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId && pi.ProductId == productId && pi.Size == ImageSize.Small);
            if (image == null)
                return this.NotFound();

            int currentGroupOrder = image.DisplayOrder;
            var lowerGroupOrder = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder < currentGroupOrder)
                .Select(pi => pi.DisplayOrder)
                .Distinct()
                .OrderByDescending(o => o)
                .FirstOrDefaultAsync();

            if (lowerGroupOrder == 0)
            {
                return this.RedirectToAction("Edit", new { id = productId });
            }

            var currentGroupImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder == currentGroupOrder)
                .ToListAsync();
            var groupAboveImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder == lowerGroupOrder)
                .ToListAsync();

            foreach (var img in currentGroupImages)
            {
                img.DisplayOrder = lowerGroupOrder;
            }
            foreach (var img in groupAboveImages)
            {
                img.DisplayOrder = currentGroupOrder;
            }
            await this.context.SaveChangesAsync();

            return this.RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveImageDown(int imageId, int productId)
        {
            var image = await this.context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId && pi.ProductId == productId && pi.Size == ImageSize.Small);
            if (image == null)
                return this.NotFound();

            int currentGroupOrder = image.DisplayOrder;
            var higherGroupOrder = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder > currentGroupOrder)
                .Select(pi => pi.DisplayOrder)
                .Distinct()
                .OrderBy(o => o)
                .FirstOrDefaultAsync();

            if (higherGroupOrder == 0)
            {
                return this.RedirectToAction("Edit", new { id = productId });
            }

            var currentGroupImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder == currentGroupOrder)
                .ToListAsync();
            var groupBelowImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder == higherGroupOrder)
                .ToListAsync();

            foreach (var img in currentGroupImages)
            {
                img.DisplayOrder = higherGroupOrder;
            }
            foreach (var img in groupBelowImages)
            {
                img.DisplayOrder = currentGroupOrder;
            }
            await this.context.SaveChangesAsync();

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

        private async Task PopulateCompanyDropDownList(object selectedId = null)
        {
            var companies = await this.context.Companies
                .OrderBy(c => c.Name)
                .ToListAsync();

            var list = companies.Select(c => new
            {
                c.CompanyId,
                c.Name
            });

            this.ViewBag.CompanyId = new SelectList(list, "CompanyId", "Name", selectedId);
        }

        /// <summary>
        /// Processes all file uploads for a product by creating multiple image versions.
        /// Each file upload is treated as a group: all variants share the same DisplayOrder and ImageGroupGuid.
        /// Files are uploaded to: /products/{ProductId}/{filename}_{ImageSize}{extension}.
        /// </summary>
        /// <summary>
        /// Processes all file uploads for a product by creating multiple image versions.
        /// Each file upload is treated as a group: all variants share the same DisplayOrder and ImageGroupGuid.
        /// Additionally, if no main image exists, the small variant of the first uploaded file is marked as main.
        /// Files are uploaded to: /products/{ProductId}/{filename}_{ImageSize}{extension}.
        /// </summary>
        /// <summary>
        /// Processes all file uploads for a product by creating multiple image versions.
        /// Each file upload is treated as a group: all variants share the same DisplayOrder and ImageGroupGuid.
        /// If the product currently has no main image, all variants in the group will be marked as main.
        /// Files are uploaded to: /products/{ProductId}/{filename}_{ImageSize}{extension}.
        /// </summary>
        private async Task ProcessProductImageUploads(Product product, IFormFileCollection productImages)
        {
            if (productImages == null || productImages.Count == 0)
            {
                return;
            }

            int currentMaxOrder = await this.GetCurrentMaxDisplayOrder(product.ProductId);
            string directory = $"products/{product.ProductId}/";

            // Check if a main image already exists.
            bool hasMainImage = product.Images.Any(pi => pi.IsMain);

            foreach (var file in productImages)
            {
                if (file != null && file.Length > 0)
                {
                    currentMaxOrder++;
                    Guid groupGuid = Guid.NewGuid();
                    int groupOrder = currentMaxOrder;

                    // Define the sizes to generate.
                    var sizes = new[] { ImageSize.Small, ImageSize.Medium, ImageSize.Large, ImageSize.Raw };

                    foreach (var size in sizes)
                    {
                        var productImage = await this.ProcessFileUploadForSize(product, file, size, directory, groupOrder);
                        productImage.ImageGroupGuid = groupGuid;
                        // If no main image exists, mark all variants in this group as main.
                        productImage.IsMain = !hasMainImage;
                        product.Images.Add(productImage);
                    }

                    // If we just marked this group as main, update the flag so that subsequent uploads won't override it.
                    if (!hasMainImage)
                    {
                        hasMainImage = true;
                    }
                }
            }
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the current maximum DisplayOrder for a given product.
        /// </summary>
        private async Task<int> GetCurrentMaxDisplayOrder(int productId)
        {
            int currentMaxOrder = 0;
            var existingImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
            if (existingImages.Any())
            {
                currentMaxOrder = existingImages.Max(pi => pi.DisplayOrder);
            }
            return currentMaxOrder;
        }

        /// <summary>
        /// Processes an individual file upload for a given image size.
        /// Resizes the image (if needed), uploads it, and returns a new ProductImage record.
        /// </summary>
        private async Task<ProductImage> ProcessFileUploadForSize(Product product, IFormFile file, ImageSize size, string directory, int displayOrder)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            string finalFileName;
            MemoryStream uploadStream;

            finalFileName = $"{fileNameWithoutExtension}_{size}{extension}".ToLower();

            if (size == ImageSize.Raw)
            {
                uploadStream = new MemoryStream();
                await file.CopyToAsync(uploadStream);
                uploadStream.Position = 0;
            }
            else
            {
                uploadStream = await this.ResizeImageAsync(file, size);
            }

            var imageUri = await this.siteFilesRepository.UploadAsync(uploadStream, finalFileName, directory);
            uploadStream.Dispose();

            return new ProductImage
            {
                ImageUrl = imageUri.ToString(),
                Size = size,
                IsMain = false,
                ProductId = product.ProductId,
                DisplayOrder = displayOrder
            };
        }

        /// <summary>
        /// Resizes the uploaded file to a specific size using ImageSharp.
        /// </summary>
        private async Task<MemoryStream> ResizeImageAsync(IFormFile file, ImageSize size)
        {
            int maxDimension = size switch
            {
                ImageSize.Small => 100,
                ImageSize.Medium => 300,
                ImageSize.Large => 800,
                _ => throw new System.ArgumentException("Invalid size for resizing", nameof(size))
            };

            using var originalMs = new MemoryStream();
            await file.CopyToAsync(originalMs);
            originalMs.Position = 0;

            using var image = SixLabors.ImageSharp.Image.Load(originalMs);
            var resizeOptions = new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(maxDimension, maxDimension)
            };
            image.Mutate(x => x.Resize(resizeOptions));

            var resizedStream = new MemoryStream();
            await image.SaveAsJpegAsync(resizedStream);
            resizedStream.Position = 0;
            return resizedStream;
        }
    }
}
