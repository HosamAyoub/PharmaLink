using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class CartRepository : Repository<CartItem>, ICartRepository
    {
        private readonly ApplicationDbContext _db;
        public CartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public int DecrementCount(CartItem shoppingCart, int count)
        {
            shoppingCart.Quantity -= count;
            return shoppingCart.Quantity;
        }

        public int IncrementCount(CartItem shoppingCart, int count)
        {
            shoppingCart.Quantity += count;
            return shoppingCart.Quantity;
        }
    }
}
