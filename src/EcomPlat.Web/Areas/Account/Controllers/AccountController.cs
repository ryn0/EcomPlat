using EcomPlat.Data.Constants;
using EcomPlat.Data.Models;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(Constants.StringConstants.AccountArea)]
    [Route("account")]
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
        /// URL: /account/login
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        /// <summary>
        /// Processes the login form submission.
        /// URL: /account/login
        /// </summary>
        [HttpPost("login")]
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
                    return this.LocalRedirect(returnUrl ?? "/account/admin/index");
                }

                this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return this.View(model);
        }

        /// <summary>
        /// Displays the registration page.
        /// URL: /account/register
        /// </summary>
        [HttpGet("register")]
        public IActionResult Register()
        {
            return this.View();
        }

        /// <summary>
        /// Processes the registration form submission.
        /// URL: /account/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                bool isFirstUser = await this.userManager.Users.CountAsync() == 0;
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await this.userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (isFirstUser)
                    {
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
        /// URL: /account/logout
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return this.RedirectToAction("Login", "Account");
        }
    }
}
