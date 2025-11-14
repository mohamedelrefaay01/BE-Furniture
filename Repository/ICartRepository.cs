using Home_furnishings.Models;

public interface ICartRepository
{
    Cart GetCartByUserId(int userId);
    Cart GetCartWithProducts(int userId);
    void AddProductToCart(int cartId, int productId);
    void AddProductToCartWithQuantity(int cartId, int productId, int quantity);
    void UpdateProductQuantityInCart(int cartId, int productId, int quantity);
    int GetProductQuantityInCart(int cartId, int productId);
    void RemoveProductFromCart(int cartId, int productId);
    void ClearCart(string userId);
    int GetCartItemCount(string userId);
}