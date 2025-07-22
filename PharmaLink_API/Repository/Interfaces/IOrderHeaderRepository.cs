using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface IOrderHeaderRepository : IRepository<Order>
    {
        void UpdateStripePaymentID(int orderId, string SESSIONid, string paymentIntentId);
    }
}
