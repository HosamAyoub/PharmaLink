using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.Interfaces
{
    public interface ICartRepository : IRepository<CartItem>
    {
        int IncrementCount(CartItem shoppingCart, int count);
        int DecrementCount(CartItem shoppingCart, int count);
        Task RemoveRangeAsync(IEnumerable<CartItem> cartItems);
    }
}
