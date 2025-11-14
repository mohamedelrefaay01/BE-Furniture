// Controllers/ProductController.cs
using Home_furnishings.Repository;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Home_furnishings.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }


        // GET: Product
        public IActionResult Index()
        {
            var products = _productRepository.GetActiveProducts();

            var viewModel = products.Select(p => new ProductViewModel
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
            }).ToList();

            return View(viewModel);
        }

        // GET: Product/Details/5
        public IActionResult Details(int id)
        {
            var product = _productRepository.GetByIdWithCategory(id);
            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = _productRepository.GetActiveByCategoryId(product.CategoryId)
                .Where(p => p.ProductId != id)
                .Take(4)
                .ToList();

            var viewModel = new ProductDetailsViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Quantity = product.Quantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                RelatedProducts = relatedProducts.Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category?.Name
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
