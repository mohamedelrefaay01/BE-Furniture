using Home_furnishings.Models;
using Microsoft.EntityFrameworkCore;

namespace Home_furnishings.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly Context _context;

        public CartRepository(Context context)
        {
            _context = context;
        }

        public Cart GetCartByUserId(int userId)
        {
            var cart = _context.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return cart;
        }

        public Cart GetCartWithProducts(int userId)
        {
            return _context.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefault(c => c.UserId == userId);
        }

        public void AddProductToCart(int cartId, int productId)
        {
            AddProductToCartWithQuantity(cartId, productId, 1);
        }

        public void AddProductToCartWithQuantity(int cartId, int productId, int quantity)
        {
            var existingCartProduct = _context.CartProducts
                .FirstOrDefault(cp => cp.CartId == cartId && cp.ProductId == productId);

            if (existingCartProduct != null)
            {
                existingCartProduct.Quantity += quantity;
                _context.CartProducts.Update(existingCartProduct);
            }
            else
            {
                var cartProduct = new CartProduct
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedDate = DateTime.Now
                };
                _context.CartProducts.Add(cartProduct);
            }

            _context.SaveChanges();
        }

        public void UpdateProductQuantityInCart(int cartId, int productId, int quantity)
        {
            var cartProduct = _context.CartProducts
                .FirstOrDefault(cp => cp.CartId == cartId && cp.ProductId == productId);

            if (cartProduct != null)
            {
                cartProduct.Quantity = quantity;
                _context.CartProducts.Update(cartProduct);
                _context.SaveChanges();
            }
        }

        public int GetProductQuantityInCart(int cartId, int productId)
        {
            var cartProduct = _context.CartProducts
                .FirstOrDefault(cp => cp.CartId == cartId && cp.ProductId == productId);

            return cartProduct?.Quantity ?? 1;
        }

        public void RemoveProductFromCart(int cartId, int productId)
        {
            var cartProduct = _context.CartProducts
                .FirstOrDefault(cp => cp.CartId == cartId && cp.ProductId == productId);

            if (cartProduct != null)
            {
                _context.CartProducts.Remove(cartProduct);
                _context.SaveChanges();
            }
        }

        public void ClearCart(int userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart != null)
            {
                var cartProducts = _context.CartProducts
                    .Where(cp => cp.CartId == cart.CartId)
                    .ToList();

                _context.CartProducts.RemoveRange(cartProducts);
                _context.SaveChanges();
            }
        }

        public int GetCartItemCount(int userId)
        {
            var cart = GetCartByUserId(userId);
            if (cart == null) return 0;

            return _context.CartProducts
                .Where(cp => cp.CartId == cart.CartId)
                .Sum(cp => cp.Quantity);
        }

        // Add explicit implementations for the string overloads required by ICartRepository

        public void ClearCart(string userId)
        {
            if (int.TryParse(userId, out int id))
            {
                ClearCart(id);
            }
            // else: optionally handle invalid string input (e.g., throw exception or ignore)
        }

        public int GetCartItemCount(string userId)
        {
            if (int.TryParse(userId, out int id))
            {
                return GetCartItemCount(id);
            }
            // else: optionally handle invalid string input (e.g., return 0 or throw exception)
            return 0;
        }
    }
}
