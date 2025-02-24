using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Authorize]
    [Area("Account")]
    public class AdminController : Controller
    {
        // The Index view below serves as the landing page for product management.
        public IActionResult Index()
        {
            return this.View();
        }
    }
}