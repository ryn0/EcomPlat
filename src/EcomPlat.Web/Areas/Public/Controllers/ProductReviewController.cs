using System.Drawing;
using System.Drawing.Imaging;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    public class ProductReviewController : Controller
    {
        private readonly ApplicationDbContext context;

        public ProductReviewController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(ProductReviewModel model)
        {
            var sessionCaptcha = this.HttpContext.Session.GetString(Constants.StringConstants.CacheKeyCaptcha);
            if (string.IsNullOrWhiteSpace(model.Captcha) ||
                !string.Equals(model.Captcha.Trim(), sessionCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                this.ModelState.AddModelError("Captcha", "Incorrect CAPTCHA. Please try again.");

                return this.View(model);
            }

            if (this.ModelState.IsValid)
            {
                this.context.ProductReviews.Add(new ProductReview()
                {
                    ProductId = model.ProductId,
                    Rating = model.Rating,
                    ReviewerName = model.ReviewerName,
                    Comment = model.Comment,
                    ReviewDate = DateTime.UtcNow,
                });
                await this.context.SaveChangesAsync();
                this.TempData["Success"] = "Review submitted for moderation.";
                return this.RedirectToAction("Success", "ProductReview");
            }
            else
            {
                var error = "There was a problem submitting your review.";
                this.TempData["Error"] = error;
                return this.Content(error);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var reviews = await this.context.ProductReviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
            return this.View(reviews);
        }

        [Route("productreview/success")]
        [HttpGet]
        public IActionResult Success()
        {
            return this.View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await this.context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await this.context.SaveChangesAsync();
            }
            return this.RedirectToAction("Manage");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await this.context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                this.context.ProductReviews.Remove(review);
                await this.context.SaveChangesAsync();
            }
            return this.RedirectToAction("Manage");
        }

        [Route("ProductReview/CaptchaImage")]
        [HttpGet]
        public IActionResult CaptchaImage()
        {
            var sessionId = this.HttpContext.Session.Id;
            string captchaText = CaptchaTextHelper.GenerateCaptchaText();
            this.HttpContext.Session.SetString(Constants.StringConstants.CacheKeyCaptcha, captchaText);
            using (var bitmap = new Bitmap(120, 30))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);
                    using (var font = new System.Drawing.Font("Arial", 20))
                    {
                        using (var brush = new SolidBrush(Color.Black))
                        {
                            graphics.DrawString(captchaText, font, brush, new PointF(10, 0));
                        }
                    }
                }
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return this.File(ms.ToArray(), "image/png");
                }
            }
        }
    }
}
