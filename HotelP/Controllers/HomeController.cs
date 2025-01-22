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

        // Konstruktor z wstrzykiwaniem us�ug
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
                // Zwr�� widok z powrotem i wy�wietl b��dy
                return View("Contact", model);
            }

            try
            {
				// 1) Zbuduj tre�� maila
				string body =
					$"Nowa wiadomo�� z formularza kontaktowego:\n\n" +
					$"Imi�: {model.Name}\n" +
					$"Email: {model.Email}\n" +
					$"Tre��: {model.Message}\n";

				// 2) Tworzymy MailMessage
				var mailMessage = new MailMessage
				{
					From = new MailAddress("dianaliv95@gmail.com", "HotelParadise"),
					Subject = "Formularz kontaktowy � wiadomo�� od u�ytkownika",
					Body = body,
					IsBodyHtml = false
				};

				// Adres, na kt�ry wysy�asz
				mailMessage.To.Add("dianaliv95@gmail.com");

				// 3) Konfiguracja SMTP
				using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
				{
					smtpClient.Credentials = new NetworkCredential("dianaliv95@gmail.com", "keofesmtvyqiqprf");
					smtpClient.EnableSsl = true;

					// 4) Wy�lij
					smtpClient.Send(mailMessage);
				}

				// 5) Przechodzimy do nowego widoku z potwierdzeniem
				//    np. "ContactResult.cshtml", przekazuj�c model
				return View("ContactResult", model);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return StatusCode(500, "B��d podczas wysy�ania maila: " + ex.Message);
			}
		}

		[HttpGet]
	public IActionResult Contact()
        {
            return View();
        }


        // Oznaczamy metod� jako async i zmieniamy typ zwracany na Task<IActionResult>
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
