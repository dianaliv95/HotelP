using HMS.Services;
using Hotel.Models;
using Hotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DishesService _dishesService;
        private readonly CategoryService _categoryService;


        private readonly AccommodationTypesService _accommodationTypesService;
        private readonly AccommodationPackagesService _accommodationPackagesService;

        // Konstruktor z wstrzykiwaniem us³ug
        public HomeController(
            ILogger<HomeController> logger,
            DishesService _dishesService,
            CategoryService _categoryService,
            AccommodationTypesService accommodationTypesService,
            AccommodationPackagesService accommodationPackagesService)
        {
            _logger = logger;
			this._dishesService = _dishesService;
			this._categoryService = _categoryService;
            _accommodationTypesService = accommodationTypesService;
            _accommodationPackagesService = accommodationPackagesService;
        }

        // Oznaczamy metodê jako async i zmieniamy typ zwracany na Task<IActionResult>
        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel();

            // 1) Dania: kategorie i same dania
            var categories = _categoryService.GetAllCategories();
            model.DishCategories = categories.ToList();

            var dishes = _dishesService.GetAllDishes();
            model.AllDishes = dishes;

            // 2) Zakwaterowania i pakiety
            model.AccommodationTypes = _accommodationTypesService.GetAllAccommodationTypes();
            model.AccommodationPackages = await _accommodationPackagesService.GetAllAccommodationPackagesAsync();

            return View(model);
        }
    }
}
