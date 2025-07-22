using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;

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
    }
}
