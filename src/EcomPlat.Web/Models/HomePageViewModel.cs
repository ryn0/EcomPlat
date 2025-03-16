using EcomPlat.Data.Models;
using System.Collections.Generic;

namespace EcomPlat.Web.Models
{
    public class HomePageViewModel
    {
        public IEnumerable<Product> RecentProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> RandomProducts { get; set; } = new List<Product>();
    }
}
