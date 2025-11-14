namespace Home_furnishings.ViewModels
{
    public class HomeViewModel
    {
        public List<CategoryViewModel> Categories { get; set; }
        public List<ProductViewModel> FeaturedProducts { get; set; }
        public bool IsAuthenticated { get; internal set; }
        public int CartItemCount { get; internal set; }
        public string? UserName { get; internal set; }
    }
}