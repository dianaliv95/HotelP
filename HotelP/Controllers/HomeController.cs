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
        private readonly AccommodationTypesService _accommodationTypesService;
        private readonly AccommodationPackagesService _accommodationPackagesService;

        // Konstruktor z wstrzykiwaniem us³ug
        public HomeController(
            ILogger<HomeController> logger,
            AccommodationTypesService accommodationTypesService,
            AccommodationPackagesService accommodationPackagesService)
        {
            _logger = logger;
            _accommodationTypesService = accommodationTypesService;
            _accommodationPackagesService = accommodationPackagesService;
        }

        // Oznaczamy metodê jako async i zmieniamy typ zwracany na Task<IActionResult>
        public async Task<IActionResult> Index()
        {
            // Tworzymy nasz model do widoku
            var model = new HomeViewModel();

            // Pobieramy listy z serwisów
            // - Synchronicznie:
            model.AccommodationTypes = _accommodationTypesService.GetAllAccommodationTypes();

            // - Asynchronicznie, wiêc u¿ywamy await
            model.AccommodationPackages = await _accommodationPackagesService.GetAllAccommodationPackagesAsync();

            // Przekazujemy do widoku
            return View(model);
        }
    }
}
