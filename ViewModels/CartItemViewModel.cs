namespace Home_furnishings.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public float SubTotal => Price * Quantity;
        public bool IsInStock { get; set; }
        public int AvailableStock { get; set; }
    }
}