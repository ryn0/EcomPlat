using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using EcomPlat.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Data.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext context;

        public OrderRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            this.context.Orders.Add(order);
            await this.context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            this.context.Orders.Update(order);
            await this.context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Order order)
        {
            this.context.Orders.Remove(order);
            await this.context.SaveChangesAsync();
        }

        public async Task<Order> FindByOrderIdAsync(int orderId)
        {
            return await this.context.Orders
                .Include(o => o.OrderItems) // Include order items if needed.
                .Include(o => o.Addresses)  // Include addresses if needed.
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order> FindByCustomerOrderIdAsync(string customerOrderId)
        {
            return await this.context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Addresses)
                .FirstOrDefaultAsync(o => o.CustomerOrderId == customerOrderId);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedOrdersAsync(int page, int pageSize)
        {
            int totalCount = await this.context.Orders.CountAsync();
            var orders = await this.context.Orders
                .OrderBy(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (orders, totalCount);
        }
    }
}
