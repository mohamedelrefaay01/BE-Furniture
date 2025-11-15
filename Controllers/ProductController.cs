// Controllers/ProductController.cs
using Home_furnishings.Models;
using Home_furnishings.Repository;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home_furnishings.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly Context _context;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            Context context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
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

        // GET: Product/Create
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = _categoryRepository.GetActiveCategories();
            return View();
        }
        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public IActionResult Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var product = new Product
                    {
                        Name = model.Name,
                        Price = model.Price,
                        Description = model.Description,
                        ImageUrl = model.ImageUrl,
                        Quantity = model.Quantity,
                        IsActive = model.IsActive,
                        CategoryId = model.CategoryId
                    };

                    _productRepository.Insert(product);
                    TempData["SuccessMessage"] = "✓ Product created successfully!";
                    return RedirectToAction("Products", "Category", new { id = model.CategoryId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Categories = _categoryRepository.GetActiveCategories();
            return View(model);
        }
        // GET: Product/Edit/5
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "❌ Product not found!";
                return RedirectToAction("Index", "Product");
            }

            var viewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.ImageUrl ?? "",
                Quantity = product.Quantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId
            };

            ViewBag.Categories = _categoryRepository.GetActiveCategories();
            return View(viewModel);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, ProductViewModel model)
        {
            if (id != model.ProductId)
            {
                TempData["ErrorMessage"] = "❌ Product ID mismatch!";
                return RedirectToAction("Index", "Product");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = new Product
                    {
                        ProductId = id,
                        Name = model.Name,
                        Price = model.Price,
                        Description = model.Description,
                        ImageUrl = model.ImageUrl,
                        Quantity = model.Quantity,
                        IsActive = model.IsActive,
                        CategoryId = model.CategoryId
                    };

                    _productRepository.Update(id, product);
                    TempData["SuccessMessage"] = "✓ Product updated successfully!";
                    return RedirectToAction("Products", "Category", new { id = model.CategoryId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                }
            }

           
            ViewBag.Categories = _categoryRepository.GetActiveCategories();
            return View(model);
        }

        // POST: Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public IActionResult Delete(int id, int categoryId)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "❌ Product not found!";
                return RedirectToAction("Products", "Category", new { id = categoryId });
            }

            _productRepository.Delete(id);
            TempData["SuccessMessage"] = "✓ Product deleted successfully!";
            return RedirectToAction("Products", "Category", new { id = categoryId });
        }
    }
}