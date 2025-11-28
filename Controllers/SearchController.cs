// Controllers/SearchController.cs
using Home_furnishings.Models;
using Home_furnishings.Repository;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Home_furnishings.Controllers
{
    public class SearchController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(
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

        [HttpGet]
        public async Task<IActionResult> Index(string query, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortBy = "name", int page = 1, int pageSize = 12)
        {
            var user = await _userManager.GetUserAsync(User);
            int cartCount = 0;

            if (user != null)
            {
                cartCount = _cartRepository.GetCartItemCount(user.Id.ToString());
            }

            // Get all active products with categories - this returns List<Product>
            var allProductsList = _productRepository.GetAllWithCategory().Where(p => p.IsActive).ToList();

            // Apply search filters
            var searchResults = ApplySearchFilters(allProductsList, query, categoryId, minPrice, maxPrice);

            // Apply sorting
            searchResults = ApplySorting(searchResults, sortBy);

            // Calculate pagination
            var totalItems = searchResults.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedResults = searchResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Create view model
            var viewModel = new SearchViewModel
            {
                Query = query ?? string.Empty,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                Products = pagedResults.Select(p => new ProductViewModel
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
                Categories = _categoryRepository.GetActiveCategories().Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    ProductCount = c.Products.Count(p => p.IsActive)
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize,
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = user?.FullName ?? user?.UserName,
                CartItemCount = cartCount
            };

            return View(viewModel);
        }

        [HttpGet]
        public JsonResult Suggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            var suggestions = _productRepository.GetActiveProducts()
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (p.Description != null && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(5)
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price.ToString("C"),
                    imageUrl = p.ImageUrl,
                    categoryName = p.Category?.Name
                })
                .ToList();

            return Json(suggestions);
        }

        [HttpPost]
        public async Task<IActionResult> QuickSearch([FromBody] QuickSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Json(new { success = false, message = "Please enter a search term" });
            }

            var products = _productRepository.GetActiveProducts()
                .Where(p => p.Name.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                           (p.Description != null && p.Description.Contains(request.Query, StringComparison.OrdinalIgnoreCase)))
                .Take(10)
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price.ToString("C"),
                    imageUrl = p.ImageUrl,
                    categoryName = p.Category?.Name,
                    url = Url.Action("Details", "Product", new { id = p.ProductId })
                })
                .ToList();

            return Json(new { success = true, products });
        }

        private List<Product> ApplySearchFilters(List<Product> products, string query, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            // Text search
            if (!string.IsNullOrWhiteSpace(query))
            {
                products = products.Where(p =>
                    p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Category != null && p.Category.Name.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            // Price range filters
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= (float)minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= (float)maxPrice.Value).ToList();
            }

            return products;
        }

        private List<Product> ApplySorting(List<Product> products, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => products.OrderBy(p => p.Name).ToList(),
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                "price" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "newest" => products.OrderByDescending(p => p.ProductId).ToList(),
                _ => products.OrderBy(p => p.Name).ToList()
            };
        }
    }

    public class QuickSearchRequest
    {
        public string Query { get; set; } = string.Empty;
    }
}
