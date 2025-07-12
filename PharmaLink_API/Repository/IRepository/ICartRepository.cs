using PharmaLink_API.Models;

namespace PharmaLink_API.Repository.IRepository
{
    public interface ICartRepository : IRepository<CartItem>
    {
        int IncrementCount(CartItem shoppingCart, int count);
        int DecrementCount(CartItem shoppingCart, int count);
    }    
}
