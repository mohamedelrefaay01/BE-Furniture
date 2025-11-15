using System.ComponentModel.DataAnnotations;

namespace Home_furnishings.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string ShippingCity { get; set; }

        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; }

        public string Notes { get; set; }

        // Cart items for display
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public float TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public float ShippingCost { get; set; } = 10.00f;
        public float Tax { get; set; }
        public float GrandTotal { get; set; }
    }
}