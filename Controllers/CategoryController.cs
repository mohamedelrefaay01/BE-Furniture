// Controllers/CategoryController.cs
using Home_furnishings.Repository;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Home_furnishings.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        // GET: Category
        public IActionResult Index()
        {
            var categories = _categoryRepository.GetActiveCategories();

            var viewModel = categories.Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                ProductCount = c.Products.Count(p => p.IsActive),
                SampleImageUrl = c.Products.FirstOrDefault(p => p.IsActive)?.ImageUrl
            }).ToList();

            return View(viewModel);
        }

        // GET: Category/Curtains
        public IActionResult Curtains()
        {
            return GetCategoryProducts(1);
        }

        // GET: Category/Carpets
        public IActionResult Carpets()
        {
            return GetCategoryProducts(2);
        }

        // GET: Category/Products/1
        public IActionResult Products(int id)
        {
            return GetCategoryProducts(id);
        }

        private IActionResult GetCategoryProducts(int categoryId)
        {
            var category = _categoryRepository.GetById(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var products = _productRepository.GetActiveByCategoryId(categoryId);

            var viewModel = new CategoryProductsViewModel
            {
                CategoryId = categoryId,
                CategoryName = category.Name,
                TotalProducts = products.Count,
                Products = products.Select(p => new ProductViewModel
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
                }).ToList()
            };

            return View("Products", viewModel);
        }
    }
}
