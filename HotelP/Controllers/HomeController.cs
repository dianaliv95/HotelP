using HMS.Services;
using Hotel.Models;
using Hotel.ViewModels;
using HotelP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
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
        public IActionResult About()
        {
            return View();
        }
		[HttpPost]
		public IActionResult ContactSubmit(ContactViewModel model)
		{
            if (!ModelState.IsValid)
            {
                // Zwróæ widok z powrotem i wyœwietl b³êdy
                return View("Contact", model);
            }

            try
            {
				// 1) Zbuduj treœæ maila
				string body =
					$"Nowa wiadomoœæ z formularza kontaktowego:\n\n" +
					$"Imiê: {model.Name}\n" +
					$"Email: {model.Email}\n" +
					$"Treœæ: {model.Message}\n";

				// 2) Tworzymy MailMessage
				var mailMessage = new MailMessage
				{
					From = new MailAddress("dianaliv95@gmail.com", "HotelParadise"),
					Subject = "Formularz kontaktowy – wiadomoœæ od u¿ytkownika",
					Body = body,
					IsBodyHtml = false
				};

				// Adres, na który wysy³asz
				mailMessage.To.Add("dianaliv95@gmail.com");

				// 3) Konfiguracja SMTP
				using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
				{
					smtpClient.Credentials = new NetworkCredential("dianaliv95@gmail.com", "keofesmtvyqiqprf");
					smtpClient.EnableSsl = true;

					// 4) Wyœlij
					smtpClient.Send(mailMessage);
				}

				// 5) Przechodzimy do nowego widoku z potwierdzeniem
				//    np. "ContactResult.cshtml", przekazuj¹c model
				return View("ContactResult", model);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return StatusCode(500, "B³¹d podczas wysy³ania maila: " + ex.Message);
			}
		}

		[HttpGet]
	public IActionResult Contact()
        {
            return View();
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
