using Microsoft.AspNetCore.Mvc;

namespace Hotel.Areas.RestaurantManagement.Controllers
{
    public class RestaurantManagement : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
