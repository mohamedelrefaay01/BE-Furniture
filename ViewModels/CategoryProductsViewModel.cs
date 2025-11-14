namespace Home_furnishings.ViewModels
{
    public class CategoryProductsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<ProductViewModel> Products { get; set; }
        public int TotalProducts { get; set; }
    }
}