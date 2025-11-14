using Home_furnishings.Models;

namespace Home_furnishings.Repository
{
    public interface IProductRepository : IRepository<Product>
    {
        List<Product> GetAllWithCategory();
        List<Product> GetProductsByCategory(int categoryId);
        Product GetByIdWithCategory(int id);
        List<Product> GetActiveProducts();
        List<Product> GetActiveByCategoryId(int categoryId);
        List<Product> GetFeaturedProducts(int count);
        bool UpdateStock(int productId, int quantity);
    }
}