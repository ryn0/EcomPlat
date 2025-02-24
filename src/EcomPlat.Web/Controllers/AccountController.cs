using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcomPlat.Data.Constants;
using EcomPlat.Data.Models;
using EcomPlat.Web.Models;

namespace EcomPlat.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        /// <summary>
        /// Processes the login form submission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            if (this.ModelState.IsValid)
            {
                var result = await this.signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return this.LocalRedirect(returnUrl ?? "/Admin/Index");
                }

                this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return this.View(model);
        }

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return this.View();
        }

        /// <summary>
        /// Processes the registration form submission.
        /// The first user to register is automatically assigned the Administrator role.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                // Check if there are zero users in the system
                bool isFirstUser = (await this.userManager.Users.CountAsync()) == 0;
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await this.userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (isFirstUser)
                    {
                        // Create Administrator role if it does not exist, then assign it to the first user.
                        if (!await this.roleManager.RoleExistsAsync(StringConstants.Administrator))
                        {
                            await this.roleManager.CreateAsync(new IdentityRole(StringConstants.Administrator));
                        }

                        await this.userManager.AddToRoleAsync(user, StringConstants.Administrator);
                    }

                    await this.signInManager.SignInAsync(user, isPersistent: false);

                    return this.RedirectToAction("Index", "Admin");
                }

                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return this.View(model);
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return this.RedirectToAction("Index", "Home");
        }
    }
}