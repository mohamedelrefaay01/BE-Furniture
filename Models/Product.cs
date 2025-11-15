using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_furnishings.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public float Price { get; set; }

        public string? Description { get; set; }  // Nullable

        public string? ImageUrl { get; set; }  // Nullable

        public int Quantity { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }  // Nullable navigation property

        // Initialize collection
        public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();
    }
}