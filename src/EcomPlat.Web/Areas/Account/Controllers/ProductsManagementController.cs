using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.FileStorage.Repositories.Interfaces;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
            // Find all small images for the product.
            var images = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.Size == ImageSize.Small)
                .ToListAsync();

            foreach (var img in images)
            {
                img.IsMain = img.ProductImageId == imageId;
            }
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveImageUp(int imageId, int productId)
        {
            var image = await this.context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId && pi.ProductId == productId && pi.Size == ImageSize.Small);
            if (image == null)
            {
                return this.NotFound();
            }

            var imageAbove = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder < image.DisplayOrder && pi.Size == ImageSize.Small)
                .OrderByDescending(pi => pi.DisplayOrder)
                .FirstOrDefaultAsync();

            if (imageAbove != null)
            {
                int temp = image.DisplayOrder;
                // Set a temporary value to break the circular dependency.
                image.DisplayOrder = -1;
                await this.context.SaveChangesAsync();

                image.DisplayOrder = imageAbove.DisplayOrder;
                imageAbove.DisplayOrder = temp;
                await this.context.SaveChangesAsync();
            }
            return this.RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveImageDown(int imageId, int productId)
        {
            var image = await this.context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId && pi.ProductId == productId && pi.Size == ImageSize.Small);
            if (image == null)
            {
                return this.NotFound();
            }

            var imageBelow = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.DisplayOrder > image.DisplayOrder && pi.Size == ImageSize.Small)
                .OrderBy(pi => pi.DisplayOrder)
                .FirstOrDefaultAsync();

            if (imageBelow != null)
            {
                int temp = image.DisplayOrder;
                // Set a temporary value to break the circular dependency.
                image.DisplayOrder = -1;
                await this.context.SaveChangesAsync();

                image.DisplayOrder = imageBelow.DisplayOrder;
                imageBelow.DisplayOrder = temp;
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
        /// Processes all file uploads for a product, creating versions in different sizes.
        /// </summary>
        /// <param name="product">The product to associate images with.</param>
        /// <param name="productImages">The collection of uploaded files.</param>
        private async Task ProcessProductImageUploads(Product product, IFormFileCollection productImages)
        {
            if (productImages == null || productImages.Count == 0)
            {
                return;
            }

            // Determine the current maximum display order for this product.
            int currentMaxOrder = await this.GetCurrentMaxDisplayOrder(product.ProductId);
            string directory = $"products/{product.ProductId}/";

            foreach (var file in productImages)
            {
                if (file != null && file.Length > 0)
                {
                    // Generate a new group id for this file upload.
                    var groupId = Guid.NewGuid();

                    // Define the sizes to generate.
                    var sizes = new[] { ImageSize.Small, ImageSize.Medium, ImageSize.Large, ImageSize.Raw };

                    foreach (var size in sizes)
                    {
                        currentMaxOrder++;
                        var productImage = await this.ProcessFileUploadForSize(product, file, size, directory, currentMaxOrder);
                        // Set the same group id for each variant.
                        productImage.ImageGroupGuid = groupId;
                        product.Images.Add(productImage);
                    }
                }
            }
            await this.context.SaveChangesAsync();
        }


        /// <summary>
        /// Returns the current maximum DisplayOrder for a given product.
        /// </summary>
        /// <param name="productId">The product id.</param>
        /// <returns>An integer representing the current maximum display order.</returns>
        private async Task<int> GetCurrentMaxDisplayOrder(int productId)
        {
            int currentMaxOrder = 0;
            // Check images already loaded in the product.
            if (this.context.Products.Any(p => p.ProductId == productId))
            {
                var images = await this.context.ProductImages.Where(pi => pi.ProductId == productId).ToListAsync();
                if (images.Any())
                {
                    currentMaxOrder = images.Max(pi => pi.DisplayOrder);
                }
            }
            return currentMaxOrder;
        }

        /// <summary>
        /// Processes an individual file upload for a given image size.
        /// Resizes the image (if needed), uploads it, and returns a new ProductImage record.
        /// </summary>
        /// <param name="product">The product associated with the image.</param>
        /// <param name="file">The uploaded file.</param>
        /// <param name="size">The desired ImageSize.</param>
        /// <param name="directory">The directory path where the image will be stored.</param>
        /// <param name="displayOrder">The sequential display order for this image.</param>
        /// <returns>A new ProductImage record.</returns>
        private async Task<ProductImage> ProcessFileUploadForSize(Product product, IFormFile file, ImageSize size, string directory, int displayOrder)
        {
            // Get file name and extension.
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            string finalFileName;
            MemoryStream uploadStream;

            finalFileName = $"{fileNameWithoutExtension}_{size}{extension}".ToLower();

            if (size == ImageSize.Raw)
            {
                // For Raw, use the file as-is.
                uploadStream = new MemoryStream();
                await file.CopyToAsync(uploadStream);
                uploadStream.Position = 0;
            }
            else
            {
                // Resize image based on the size.
                uploadStream = await this.ResizeImageAsync(file, size);
            }

            // Upload the file.
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
        /// Resizes the uploaded file to a specific size.
        /// </summary>
        /// <param name="file">The uploaded IFormFile.</param>
        /// <param name="size">The target ImageSize (Small, Medium, or Large).</param>
        /// <returns>A MemoryStream containing the resized image.</returns>
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
                Size = new Size(maxDimension, maxDimension)
            };
            image.Mutate(x => x.Resize(resizeOptions));

            var resizedStream = new MemoryStream();
            await image.SaveAsJpegAsync(resizedStream);
            resizedStream.Position = 0;
            return resizedStream;
        }
    }
}
