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
        private readonly Context _context;

        public CartController(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager,
            Context context)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _context = context;
        }

        //public CartController(
        //    ICartRepository cartRepository,
        //    IProductRepository productRepository,
        //    UserManager<ApplicationUser> userManager)
        //{
        //    _cartRepository = cartRepository;
        //    _productRepository = productRepository;
        //    _userManager = userManager;
        //}

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

        // Add these methods to your CartController

        // GET: Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartRepository.GetCartWithProducts(user.Id);

            if (cart == null || cart.CartProducts == null || !cart.CartProducts.Any())
            {
                TempData["Message"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            var cartItems = cart.CartProducts.Select(cp => new CartItemViewModel
            {
                ProductId = cp.Product.ProductId,
                ProductName = cp.Product.Name,
                Price = cp.Product.Price,
                ImageUrl = cp.Product.ImageUrl,
                Quantity = cp.Quantity,
                IsInStock = cp.Product.IsActive && cp.Product.Quantity > 0,
                AvailableStock = cp.Product.Quantity
            }).ToList();

            float subtotal = cartItems.Sum(i => i.SubTotal);
            float shippingCost = 10.00f;
            float tax = subtotal * 0.1f; // 10% tax
            float grandTotal = subtotal + shippingCost + tax;

            var viewModel = new CheckoutViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CartItems = cartItems,
                TotalPrice = subtotal,
                TotalItems = cartItems.Sum(i => i.Quantity),
                ShippingCost = shippingCost,
                Tax = tax,
                GrandTotal = grandTotal
            };

            return View(viewModel);
        }

        // POST: Cart/ProcessCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = _cartRepository.GetCartWithProducts(user.Id);

                // Repopulate cart items for display
                model.CartItems = cart.CartProducts.Select(cp => new CartItemViewModel
                {
                    ProductId = cp.Product.ProductId,
                    ProductName = cp.Product.Name,
                    Price = cp.Product.Price,
                    ImageUrl = cp.Product.ImageUrl,
                    Quantity = cp.Quantity
                }).ToList();

                float subtotal = model.CartItems.Sum(i => i.SubTotal);
                model.TotalPrice = subtotal;
                model.TotalItems = model.CartItems.Sum(i => i.Quantity);
                model.ShippingCost = 10.00f;
                model.Tax = subtotal * 0.1f;
                model.GrandTotal = subtotal + model.ShippingCost + model.Tax;

                return View("Checkout", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var cart = _cartRepository.GetCartWithProducts(user.Id);

                if (cart == null || !cart.CartProducts.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty!";
                    return RedirectToAction("Index", "Cart");
                }

                // Create order with all form data
                var order = new Order
                {
                    UserId = user.Id.ToString(),
                    OrderDate = DateTime.Now,
                    TotalAmount = (decimal)model.GrandTotal,
                    OrderStatus = "Pending",
                    ShippingAddress = model.ShippingAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingPostalCode = model.PostalCode ?? "",
                    ShippingCountry = "USA", // You can add this to form if needed
                    PhoneNumber = model.PhoneNumber,
                    Notes = model.Notes ?? "",
                    ShippingCost = (decimal)model.ShippingCost,
                    Tax = (decimal)model.Tax,
                    OrderItems = new List<OrderItem>()
                };

                // Add order items from cart
                foreach (var cartProduct in cart.CartProducts)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = cartProduct.ProductId,
                        Quantity = cartProduct.Quantity,
                        UnitPrice = (decimal)cartProduct.Product.Price,
                        Discount = 0
                    };

                    order.OrderItems.Add(orderItem);
                }

                // Save order to database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create payment record
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = (float)order.TotalAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = model.PaymentMethod
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Clear the cart after successful order
                _cartRepository.ClearCart(user.Id.ToString());

                // Set success message
                TempData["SuccessMessage"] = $"🎉 Order #{order.OrderId} placed successfully! Thank you for your purchase.";
                TempData["OrderId"] = order.OrderId;

                // Redirect to Home
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in ProcessCheckout: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                ModelState.AddModelError("", "Error processing your order. Please try again.");

                // Repopulate cart items on error
                var user = await _userManager.GetUserAsync(User);
                var cart = _cartRepository.GetCartWithProducts(user.Id);

                model.CartItems = cart.CartProducts.Select(cp => new CartItemViewModel
                {
                    ProductId = cp.Product.ProductId,
                    ProductName = cp.Product.Name,
                    Price = cp.Product.Price,
                    ImageUrl = cp.Product.ImageUrl,
                    Quantity = cp.Quantity
                }).ToList();

                float subtotal = model.CartItems.Sum(i => i.SubTotal);
                model.TotalPrice = subtotal;
                model.TotalItems = model.CartItems.Sum(i => i.Quantity);
                model.ShippingCost = 10.00f;
                model.Tax = subtotal * 0.1f;
                model.GrandTotal = subtotal + model.ShippingCost + model.Tax;

                return View("Checkout", model);
            }
        }

        // GET: Cart/OrderConfirmation
        public IActionResult OrderConfirmation(int orderId)
        {
            TempData["OrderId"] = orderId;
            return View();
        }


    }
}