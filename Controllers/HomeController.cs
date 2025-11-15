// Controllers/HomeController.cs
using Home_furnishings.Models;
using Home_furnishings.Repository;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace Home_furnishings.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            ICartRepository cartRepository,
            UserManager<ApplicationUser> userManager)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var categories = _categoryRepository.GetActiveCategories();
            var featuredProducts = _productRepository.GetFeaturedProducts(6);
            var user = await _userManager.GetUserAsync(User);
            int cartCount = 0;
            if (user != null)
            {
                cartCount = _cartRepository.GetCartItemCount(user.Id.ToString());
            }
            var viewModel = new HomeViewModel
            {
                Categories = categories.Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    ProductCount = c.Products.Count(p => p.IsActive),
                    SampleImageUrl = c.Products.FirstOrDefault(p => p.IsActive)?.ImageUrl
                }).ToList(),
                FeaturedProducts = featuredProducts.Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Quantity = p.Quantity,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name
                }).ToList(),
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = user?.FullName ?? user?.UserName,
                CartItemCount = cartCount
            };
            return View(viewModel);
        }
    }
}