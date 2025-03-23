using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.FileStorage.Repositories.Interfaces;
using EcomPlat.Utilities.Helpers;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly ISiteFilesRepository siteFilesRepository;

        public ProductsManagementController(ApplicationDbContext context, ISiteFilesRepository siteFilesRepository, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
            this.siteFilesRepository = siteFilesRepository;
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1, int pageSize = IntegerConstants.PageSize)
        {
            IQueryable<Product> query = this.context.Products
                .Include(p => p.Images.Where(i => i.IsMain))
                .Include(p => p.Subcategory)
                .Include(p => p.Company)
                .OrderByDescending(p => p.CreateDate);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Check if input is a product URL and extract ProductKey
                var productKey = this.ExtractProductKeyFromUrl(searchQuery);
                if (!string.IsNullOrEmpty(productKey))
                {
                    query = query.Where(p => p.ProductKey == productKey);
                }
                else
                {
                    // Perform full-text search on Name & Description
                    query = query.Where(p =>
                        p.Name.Contains(searchQuery) ||
                        p.Description.Contains(searchQuery));
                }
            }

            int totalProducts = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            this.ViewData["CurrentPage"] = page;
            this.ViewData["PageSize"] = pageSize;
            this.ViewData["TotalProducts"] = totalProducts;
            this.ViewData["SearchQuery"] = searchQuery;

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
            await this.LoadDropDowns();

            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFileCollection productImages)
        {
            this.ValidateProduct(product);

            if (this.ModelState.IsValid)
            {
                product.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                product = this.Clean(product);
                this.context.Add(product);
                await this.context.SaveChangesAsync();

                await this.ProcessProductImageUploads(product, productImages);
                return this.RedirectToAction(nameof(this.Index));
            }

            await this.LoadDropDowns();
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

            await this.PopulateDropDowns(product);
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

            this.ValidateProduct(product);

            if (this.ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await this.context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return this.NotFound();
                    }

                    existingProduct.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                    this.UpdateProductFields(existingProduct, product);

                    this.context.Update(existingProduct);
                    await this.context.SaveChangesAsync();

                    await this.ProcessProductImageUploads(existingProduct, productImages);
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

            await this.PopulateDropDowns(product);
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

            // Retrieve all images for the product.
            var allImages = await this.context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            // Separate images into the chosen group and others.
            var mainGroup = allImages
                .Where(pi => pi.ImageGroupGuid == chosenGroupGuid)
                .OrderBy(pi => pi.DisplayOrder)
                .ToList();
            var otherGroup = allImages
                .Where(pi => pi.ImageGroupGuid != chosenGroupGuid)
                .OrderBy(pi => pi.DisplayOrder)
                .ToList();

            int order = 1;

            // Update images in the chosen group.
            foreach (var img in mainGroup)
            {
                img.DisplayOrder = order;
                img.IsMain = true;
                img.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                order++;
            }

            // Update images in the other group.
            foreach (var img in otherGroup)
            {
                img.DisplayOrder = order;
                img.IsMain = false;
                img.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                order++;
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
            {
                return this.NotFound();
            }

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
                img.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
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
            {
                return this.NotFound();
            }

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
                img.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
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

            var list = subcategories.Select(s => new SelectListItem
            {
                Value = s.SubcategoryId.ToString(),
                Text = s.Category.Name + " > " + s.Name
            }).ToList();

            // Insert default option at the top.
            list.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });
            this.ViewBag.SubCategoryId = new SelectList(list, "Value", "Text", selectedId);
        }

        private async Task PopulateCompanyDropDownList(object selectedId = null)
        {
            var companies = await this.context.Companies
                .OrderBy(c => c.Name)
                .ToListAsync();

            var list = companies.Select(c => new SelectListItem
            {
                Value = c.CompanyId.ToString(),
                Text = c.Name
            }).ToList();

            // Insert default option at the top.
            list.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });
            this.ViewBag.CompanyId = new SelectList(list, "Value", "Text", selectedId);
        }

        private async Task PopulateCountryDropDownList(object selectedId = null)
        {
            // Get the dictionary of countries from the helper.
            var countries = CountryHelper.GetCountries();

            // Build a list of SelectListItem from the dictionary.
            var list = countries.Select(c => new SelectListItem
            {
                Value = c.Key,
                Text = c.Value
            }).ToList();

            // Insert default option at the top.
            list.Insert(0, new SelectListItem { Value = "", Text = StringConstants.SelectText });
            this.ViewBag.CountryOrigin = new SelectList(list, "Value", "Text", selectedId);
            await Task.CompletedTask; // For async signature compliance.
        }

        /// <summary>
        /// Updates the fields on the existing product with values from the new product.
        /// </summary>
        /// <param name="existingProduct">The product loaded from the database.</param>
        /// <param name="newProduct">The new product values from the form.</param>
        private void UpdateProductFields(Product existingProduct, Product newProduct)
        {
            existingProduct.Name = newProduct.Name.Trim();
            existingProduct.ProductKey = StringHelpers.UrlKey(newProduct.Name);
            existingProduct.Description = newProduct.Description.Trim();
            existingProduct.Price = newProduct.Price;
            existingProduct.SalePrice = newProduct.SalePrice;
            existingProduct.IsAvailable = newProduct.IsAvailable;
            existingProduct.ProductWeightOunces = newProduct.ProductWeightOunces;
            existingProduct.ShippingWeightOunces = newProduct.ShippingWeightOunces;
            existingProduct.HeightInches = newProduct.HeightInches;
            existingProduct.WidthInches = newProduct.WidthInches;
            existingProduct.LengthInches = newProduct.LengthInches;
            existingProduct.StockQuantity = newProduct.StockQuantity;
            existingProduct.SubcategoryId = newProduct.SubcategoryId;
            existingProduct.CompanyId = newProduct.CompanyId;
            existingProduct.Upc = newProduct.Upc;
            existingProduct.Sku = newProduct.Sku;
            existingProduct.Notes = newProduct.Notes;
            existingProduct.CreateDate = existingProduct.CreateDate;
            existingProduct.CreatedByUserId = existingProduct.CreatedByUserId;
            existingProduct.CountryOfOrigin = newProduct.CountryOfOrigin;
            existingProduct.ProductReview = existingProduct.ProductReview;
            existingProduct.Company = existingProduct.Company;
            existingProduct.PriceCurrency = newProduct.PriceCurrency;
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
                        // Compute the final file name for this size.
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                        var extension = Path.GetExtension(file.FileName);
                        string finalFileName = $"{fileNameWithoutExtension}_{size}{extension}".ToLower();

                        // Look for an existing image with the same file name for this product and size.
                        var existingImage = product.Images.FirstOrDefault(i =>
                            i.Size == size &&
                            i.ImageUrl.EndsWith(finalFileName, StringComparison.OrdinalIgnoreCase));

                        if (existingImage != null)
                        {
                            // Process the file upload (this returns a new image with the same final file name).
                            var updatedImage = await this.ProcessFileUploadForSize(product, file, size, directory, groupOrder);
                            // Overwrite the existing record's URL and update the timestamp.
                            existingImage.ImageUrl = updatedImage.ImageUrl;
                            existingImage.UpdateDate = DateTime.UtcNow;
                            existingImage.UpdatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                        }
                        else
                        {
                            // No existing record: create a new one.
                            var productImage = await this.ProcessFileUploadForSize(product, file, size, directory, groupOrder);
                            productImage.CreatedByUserId = this.userManager.GetUserId(this.User) ?? string.Empty;
                            productImage.ImageGroupGuid = groupGuid;
                            productImage.IsMain = !hasMainImage;

                            // Add to both the context and the navigation collection.
                            this.context.ProductImages.Add(productImage);
                            product.Images.Add(productImage);
                        }
                    }

                    if (!hasMainImage)
                    {
                        hasMainImage = true;
                    }
                }
            }

            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Optionally log the error.
                throw; // or handle as needed
            }
        }

        private async Task PopulateDropDowns(Product product)
        {
            await this.PopulateSubcategoryDropDownList(product.SubcategoryId);
            await this.PopulateCompanyDropDownList(product.CompanyId);
            await this.PopulateCountryDropDownList(product.CountryOfOrigin);
        }

        private void ValidateProduct(Product product)
        {
            if (product.ProductWeightOunces > product.ShippingWeightOunces)
            {
                this.ModelState.AddModelError(string.Empty, "Product weight cannot be greater than shiping weight");
            }
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

        private string? ExtractProductKeyFromUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uri))
            {
                string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                int index = Array.IndexOf(segments, "product");
                if (index >= 0 && index + 1 < segments.Length)
                {
                    return segments[index + 1]; // Product Key from URL
                }
            }

            return null;
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

        private async Task LoadDropDowns()
        {
            await this.PopulateSubcategoryDropDownList();
            await this.PopulateCompanyDropDownList();
            await this.PopulateCountryDropDownList();
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

        private Product Clean(Product product)
        {
            product.ProductKey = StringHelpers.UrlKey(product.Name);
            product.Name = product.Name.Trim();
            product.Notes = product?.Notes?.Trim();

            return product;
        }
    }
}
