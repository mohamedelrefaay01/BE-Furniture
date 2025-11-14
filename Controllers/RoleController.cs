using Home_furnishings.Models;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Home_furnishings.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole<int>> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        //  show all roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

          
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        //  create new role
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Role name is required");
                return View();
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                ModelState.AddModelError("", "Role already exists");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{roleName}' created successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        //  delete role
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found!";
                return RedirectToAction("Index");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                TempData["Success"] = $"Role '{role.Name}' deleted successfully!";
            else
                TempData["Error"] = "Error deleting role!";

            return RedirectToAction("Index");
        }

        //  show all users for role management
        public IActionResult ManageUserRoles()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        //  show edit user roles
        [HttpGet]
        public async Task<IActionResult> EditUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.Id;

            var model = allRoles.Select(role => new RoleSelectionViewModel
            {
                RoleName = role,
                IsSelected = userRoles.Contains(role)
            }).ToList();

            return View(model);
        }

        //  save edited user roles
        [HttpPost]
        public async Task<IActionResult> EditUserRoles(string userId, List<RoleSelectionViewModel> model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = model.Where(x => x.IsSelected).Select(x => x.RoleName);
            var addRoles = selectedRoles.Except(userRoles);
            var removeRoles = userRoles.Except(selectedRoles);

            await _userManager.AddToRolesAsync(user, addRoles);
            await _userManager.RemoveFromRolesAsync(user, removeRoles);

            TempData["Success"] = "User roles updated successfully!";
            return RedirectToAction("ManageUserRoles");
        }
    }
}
