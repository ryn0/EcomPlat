using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Authorize]
    [Area(Constants.StringConstants.AccountArea)]
    public class ReviewManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private const int PageSize = 20;

        public ReviewManagementController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            var totalCount = await this.context.ProductReviews.CountAsync();
            var reviews = await this.context.ProductReviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var model = new ProductReviewManageViewModel
            {
                Reviews = reviews,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
            };

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await this.context.ProductReviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.ProductReviewId == id);

            if (review == null)
            {
                return this.NotFound();
            }

            return this.View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string reviewerName, string comment, int rating, bool isApproved)
        {
            var review = await this.context.ProductReviews.FindAsync(id);

            if (review == null)
            {
                return this.NotFound();
            }

            review.ReviewerName = reviewerName;
            review.Comment = comment;
            review.Rating = rating;
            review.IsApproved = isApproved;

            await this.context.SaveChangesAsync();
            await this.UpdateProductAverageRating(review.ProductId);

            this.TempData["Success"] = "Review updated successfully.";
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await this.context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await this.context.SaveChangesAsync();
                await this.UpdateProductAverageRating(review.ProductId);
                this.TempData["Success"] = "Review approved.";
            }
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await this.context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                int productId = review.ProductId;
                this.context.ProductReviews.Remove(review);
                await this.context.SaveChangesAsync();
                await this.UpdateProductAverageRating(productId);
                this.TempData["Success"] = "Review deleted.";
            }
            return this.RedirectToAction("Index");
        }

        private async Task UpdateProductAverageRating(int productId)
        {
            var approvedReviews = await this.context.ProductReviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .ToListAsync();

            if (approvedReviews.Any())
            {
                var average = approvedReviews.Average(r => r.Rating);
                var product = await this.context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.ProductReview = (decimal?)Math.Round(average, 2);
                    await this.context.SaveChangesAsync();
                }
            }
        }
    }
}
