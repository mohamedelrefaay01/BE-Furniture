
using Home_furnishings.Models;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Home_furnishings.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole<int>> roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<int>> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;

        }


        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser_ViewModel model)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var _user = new ApplicationUser
                {
                    UserName = model.UserName,
                    FullName = model.FullName,
                    Email = model.Email
                };

                var result = await userManager.CreateAsync(_user, model.Password);
                if (result.Succeeded)
                {

                    TempData["Email"] = model.Email;
                    TempData["Password"] = model.Password;
                    TempData["Success"] = "Registration successful! Please login.";


                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }












        // GET: /Account/Login

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var model = new LoginUser_ViewModel();

            if (TempData["Email"] != null)
                model.Email = TempData["Email"].ToString();
            if (TempData["Password"] != null)
                model.Password = TempData["Password"].ToString();

            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUser_ViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await userManager.FindByEmailAsync(model.Email); 
                if (user != null)
                {

                    var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);
                    if (passwordValid)
                    {
                        await signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                        TempData["Success"] = "Login successful!";
                        return RedirectToAction("Index", "Home");
                    }

                   
                }
            }
           
         ModelState.AddModelError("", "Invalid login attempt. Please check your email or password.");
            

            
            return View(model);
        }


        //logout
        //[HttpPost]
       [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");

        }


        public IActionResult AccessDenied()
        {
            return View();
        }




    }
}
