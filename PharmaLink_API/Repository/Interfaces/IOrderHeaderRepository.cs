using PharmaLink_API.Models;
using System.Linq.Expressions;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IOrderHeaderRepository : IRepository<Order>
    {
        void UpdateStripePaymentID(int orderId, string SESSIONid, string paymentIntentId);
        Task<List<Order>> GetAllWithDetailsAsync(
        Expression<Func<Order, bool>> filter = null,
        Func<IQueryable<Order>, IQueryable<Order>> include = null);
    }
}
