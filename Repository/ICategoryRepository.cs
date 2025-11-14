using Home_furnishings.Models;

namespace Home_furnishings.Repository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        List<Category> GetAllWithProducts();
        Category GetByIdWithProducts(int id);
        List<Category> GetActiveCategories();
    }
}