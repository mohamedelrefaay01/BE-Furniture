using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Home_furnishings.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        public string FullName { get; set; }
        public List<Order> Orders { get; set; }
        // Changed to single Cart (one cart per user)
        public Cart Cart { get; set; }
    }
}