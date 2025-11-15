// Repository/ProductRepository.cs
using Home_furnishings.Models;
using Microsoft.EntityFrameworkCore;

namespace Home_furnishings.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly Context _context;

        public ProductRepository(Context context)
        {
            _context = context;
        }

        public List<Product> GetAll()
        {
            return _context.Products.ToList();
        }

        public Product GetById(int id)
        {
            return _context.Products.Find(id);
        }

        public void Insert(Product product)
        {
            System.Diagnostics.Debug.WriteLine($"=== REPOSITORY INSERT CALLED ===");
            System.Diagnostics.Debug.WriteLine($"Product Name: {product.Name}");
            System.Diagnostics.Debug.WriteLine($"Product Price: {product.Price}");
            System.Diagnostics.Debug.WriteLine($"CategoryId: {product.CategoryId}");

            _context.Products.Add(product);

            System.Diagnostics.Debug.WriteLine("Product added to context, calling SaveChanges...");

            var result = _context.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"SaveChanges returned: {result} (should be 1)");
            System.Diagnostics.Debug.WriteLine($"Product ProductId after save: {product.ProductId}");
        }
        public void Update(int id, Product product)
        {
            var entity = _context.Products.Find(id);
            if (entity != null)
            {
                entity.Name = product.Name;
                entity.Price = product.Price;
                entity.Description = product.Description;
                entity.ImageUrl = product.ImageUrl;
                entity.CategoryId = product.CategoryId;
                entity.Quantity = product.Quantity;
                entity.IsActive = product.IsActive;

                _context.SaveChanges();
            }
            else
            {
                throw new Exception($"Product with ID {id} not found");
            }
        }

        public void Delete(int id)
        {
            var entity = _context.Products.Find(id);
            if (entity != null)
            {
                _context.Products.Remove(entity);
                _context.SaveChanges();
            }
        }

        public List<Product> GetAllWithCategory()
        {
            return _context.Products
                .Include(p => p.Category)
                .ToList();
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            return _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToList();
        }

        public Product GetByIdWithCategory(int id)
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);
        }

        public List<Product> GetActiveProducts()
        {
            return _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Quantity > 0)
                .ToList();
        }

        public List<Product> GetActiveByCategoryId(int categoryId)
        {
            return _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive && p.Quantity > 0)
                .ToList();
        }

        public List<Product> GetFeaturedProducts(int count)
        {
            return _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Quantity > 0)
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        public bool UpdateStock(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);
            if (product != null && product.Quantity >= quantity)
            {
                product.Quantity -= quantity;
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}