using EcomPlat.Data.DbContextInfo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.ViewComponents
{
    public class CartItemCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext context;

        public CartItemCountViewComponent(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Use the session ID to get the current cart.
            string sessionId = this.HttpContext.Session.Id;
            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            // Sum up the total quantity of items.
            int itemCount = cart?.Items.Sum(i => i.Quantity) ?? 0;
            return this.View("Default", itemCount);
        }
    }
}
