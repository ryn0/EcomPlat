using EcomPlat.Data.Models;

namespace EcomPlat.Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        /// <summary>
        /// Creates a new order in the database.
        /// </summary>
        Task<Order> CreateAsync(Order order);

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        Task UpdateAsync(Order order);

        /// <summary>
        /// Deletes an existing order.
        /// </summary>
        Task DeleteAsync(Order order);

        /// <summary>
        /// Finds an order by its internal OrderId.
        /// </summary>
        Task<Order> FindByOrderIdAsync(int orderId);

        /// <summary>
        /// Finds an order by its customer-friendly CustomerOrderId.
        /// </summary>
        Task<Order> FindByCustomerOrderIdAsync(string customerOrderId);

        /// <summary>
        /// Returns a paged list of orders along with the total count.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A tuple of the orders and the total count.</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedOrdersAsync(int page, int pageSize);
    }
}