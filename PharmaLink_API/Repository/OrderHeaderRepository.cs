using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using System.Linq.Expressions;

namespace PharmaLink_API.Repository
{
    public class OrderHeaderRepository : Repository<Order>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void UpdateStripePaymentID(int orderId, string sessionId, string paymentIntentId)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.SessionId = sessionId;
                order.PaymentIntentId = paymentIntentId;
                _db.SaveChanges(); 
            }
        }
        public async Task<List<Order>> GetAllWithDetailsAsync(
        Expression<Func<Order, bool>> filter = null,
        Func<IQueryable<Order>, IQueryable<Order>> include = null)
        {
            IQueryable<Order> query = _db.Orders;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }
    }
}
