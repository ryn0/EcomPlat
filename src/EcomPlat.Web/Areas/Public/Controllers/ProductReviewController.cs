using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

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
                    ReviewerName = model.ReviewerName.Trim(),
                    Comment = model.Comment.Trim(),
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
            var sessionId = this.HttpContext.Session.Id;

            string captchaText = CaptchaTextHelper.GenerateCaptchaText();
            this.HttpContext.Session.SetString(Constants.StringConstants.CacheKeyCaptcha, captchaText);

            int width = 120;
            int height = 40;

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
            float fontSize = 20;

            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
            };

            using var font = new SKFont
            {
                Typeface = typeface,
                Size = fontSize
            };

            // Measure text width using glyphs
            var glyphs = font.GetGlyphs(captchaText);
            var widths = font.GetGlyphWidths(glyphs);
            float textWidth = widths.Sum();

            // Measure height using font metrics
            var metrics = font.Metrics;
            float textHeight = metrics.Descent - metrics.Ascent;

            // Calculate position
            float x = (width - textWidth) / 2; // center horizontally
            float y = ((height + textHeight) / 2) - metrics.Descent; // center vertically

            // Draw the text
            canvas.DrawText(
                   text: captchaText,
                   x: x,
                   y: y,
                   textAlign: SKTextAlign.Left,
                   font: font,
                   paint: paint);

            // Encode to PNG
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var captcha = data.ToArray();

            return this.File(data.ToArray(), "image/png");
        }
    }
}