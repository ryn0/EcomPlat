using System.Drawing;
using System.Drawing.Imaging;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Mvc;

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
                return this.BadRequest("Incorrect CAPTCHA.Please try again.");
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
                return this.RedirectToAction("Success");
            }

            return this.BadRequest("There was a problem submitting your review.");
        }

        [Route("productreview/success")]
        [HttpGet]
        public IActionResult Success()
        {
            return this.View();
        }

        [Route("ProductReview/CaptchaImage")]
        [HttpGet]
        public IActionResult CaptchaImage()
        {
            var captchaText = CaptchaTextHelper.GenerateCaptchaText();
            this.HttpContext.Session.SetString(Constants.StringConstants.CacheKeyCaptcha, captchaText);

            using var bitmap = new Bitmap(120, 30);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            using var font = new Font("Arial", 20);
            using var brush = new SolidBrush(Color.Black);
            graphics.DrawString(captchaText, font, brush, new PointF(10, 0));
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return this.File(ms.ToArray(), "image/png");
        }
    }
}