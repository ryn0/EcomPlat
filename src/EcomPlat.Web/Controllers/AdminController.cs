using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomPlat.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        // The Index view below serves as the landing page for product management.
        public IActionResult Index()
        {
            return this.View();
        }
    }
}