using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<Order>
    {
        void UpdateStripePaymentID(int orderId, string SESSIONid, string paymentIntentId);
    }
}
