using System.ComponentModel.DataAnnotations;

namespace Home_furnishings.ViewModels
{
    public class RegisterUser_ViewModel
    {

                                 //validation attributes
        [Required]
        [StringLength(50, ErrorMessage = "User name cannot exceed 50 characters.")]
        public string UserName { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; }


        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")] 
        public string Email { get; set; }



        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be at least 8 characters long.")]
       // [RegularExpression(
       //   @"^(?=.*[a-z]){4,}$",
       //    ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
       //)]
        // @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        public string Password { get; set; }



        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
