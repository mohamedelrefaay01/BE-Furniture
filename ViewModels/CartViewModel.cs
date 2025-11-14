namespace Home_furnishings.ViewModels
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public List<CartItemViewModel> Items { get; set; }
        public float TotalPrice { get; set; }
        public int TotalItems { get; set; }
    }
}