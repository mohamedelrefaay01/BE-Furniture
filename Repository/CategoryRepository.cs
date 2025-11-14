// Repository/CategoryRepository.cs
using Home_furnishings.Models;
using Microsoft.EntityFrameworkCore;

namespace Home_furnishings.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly Context _context;

        public CategoryRepository(Context context)
        {
            _context = context;
        }

        public List<Category> GetAll()
        {
            return _context.Categories.ToList();
        }

        public Category GetById(int id)
        {
            return _context.Categories.Find(id);
        }

        public void Insert(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void Update(int id, Category category)
        {
            var entity = _context.Categories.Find(id);
            if (entity != null)
            {
                entity.Name = category.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.Categories.Find(id);
            if (entity != null)
            {
                _context.Categories.Remove(entity);
                _context.SaveChanges();
            }
        }

        public List<Category> GetAllWithProducts()
        {
            return _context.Categories
                .Include(c => c.Products)
                .ToList();
        }

        public Category GetByIdWithProducts(int id)
        {
            return _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);
        }

        public List<Category> GetActiveCategories()
        {
            return _context.Categories
                .Include(c => c.Products)
                .Where(c => c.Products.Any(p => p.IsActive))
                .ToList();
        }
    }
}