using Home_furnishings.Models;
using Home_furnishings.Repository;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Home_furnishings.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartRepository.GetCartWithProducts(user.Id);

            if (cart == null || cart.CartProducts == null || !cart.CartProducts.Any())
            {
                return View(new CartViewModel
                {
                    Items = new List<CartItemViewModel>(),
                    TotalPrice = 0,
                    TotalItems = 0
                });
            }

            var viewModel = new CartViewModel
            {
                CartId = cart.CartId,
                Items = cart.CartProducts.Select(cp => new CartItemViewModel
                {
                    ProductId = cp.Product.ProductId,
                    ProductName = cp.Product.Name,
                    Price = cp.Product.Price,
                    ImageUrl = cp.Product.ImageUrl,
                    Quantity = cp.Quantity,
                    IsInStock = cp.Product.IsActive && cp.Product.Quantity > 0,
                    AvailableStock = cp.Product.Quantity
                }).ToList()
            };

            viewModel.TotalItems = viewModel.Items.Sum(i => i.Quantity);
            viewModel.TotalPrice = viewModel.Items.Sum(i => i.SubTotal);

            return View(viewModel);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Please login first", redirectToLogin = true });
            }

            var product = _productRepository.GetById(model.ProductId);
            if (product == null || !product.IsActive)
            {
                return Json(new { success = false, message = "Product not available" });
            }

            if (product.Quantity < model.Quantity)
            {
                return Json(new { success = false, message = $"Only {product.Quantity} items available in stock" });
            }

            var cart = _cartRepository.GetCartByUserId(user.Id);

            // Check if product already in cart
            if (cart.CartProducts.Any(cp => cp.ProductId == model.ProductId))
            {
                // Update quantity instead of adding duplicate
                _cartRepository.UpdateProductQuantityInCart(cart.CartId, model.ProductId, model.Quantity);
            }
            else
            {
                // Add new product with quantity
                _cartRepository.AddProductToCartWithQuantity(cart.CartId, model.ProductId, model.Quantity);
            }

            int cartCount = _cartRepository.GetCartItemCount(user.Id.ToString());
            return Json(new { success = true, message = "Product added to cart", cartCount });
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (quantity < 1)
            {
                return Json(new { success = false, message = "Quantity must be at least 1" });
            }

            var product = _productRepository.GetById(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (quantity > product.Quantity)
            {
                return Json(new { success = false, message = $"Only {product.Quantity} items available" });
            }

            var cart = _cartRepository.GetCartByUserId(user.Id);
            _cartRepository.UpdateProductQuantityInCart(cart.CartId, productId, quantity);

            var updatedCart = _cartRepository.GetCartWithProducts(user.Id);

            // Recalculate totals
            float totalPrice = updatedCart.CartProducts.Sum(cp => cp.Product.Price * cp.Quantity);
            float itemSubTotal = product.Price * quantity;

            return Json(new
            {
                success = true,
                totalPrice,
                itemSubTotal,
                quantity
            });
        }

        // POST: Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            var cart = _cartRepository.GetCartByUserId(user.Id);
            _cartRepository.RemoveProductFromCart(cart.CartId, productId);

            int cartCount = _cartRepository.GetCartItemCount(user.Id.ToString());
            var updatedCart = _cartRepository.GetCartWithProducts(user.Id);

            float totalPrice = updatedCart?.CartProducts.Sum(cp => cp.Product.Price * cp.Quantity) ?? 0;

            return Json(new { success = true, cartCount, totalPrice });
        }

        // POST: Cart/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            _cartRepository.ClearCart(user.Id.ToString());

            return Json(new { success = true });
        }

        // GET: Cart/GetCartCount
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            int count = _cartRepository.GetCartItemCount(user.Id.ToString());
            return Json(new { count });
        }
    }
}