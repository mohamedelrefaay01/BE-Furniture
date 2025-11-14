using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_furnishings.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }  // Changed to string for Identity
        public ApplicationUser User { get; set; }

        // Use CartProduct join table instead of direct Products list
        public List<CartProduct> CartProducts { get; set; }

        // Helper property to get products
        [NotMapped]
        public List<Product> Products
        {
            get
            {
                return CartProducts?.Select(cp => cp.Product).ToList() ?? new List<Product>();
            }
        }
    }
}