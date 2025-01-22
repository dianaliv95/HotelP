using HMS.Entities;
using HMS.Services;
using Hotel.ViewModels; // tu znajduje się klasa AccomodationsViewModel (zmień, jeśli u Ciebie inaczej)
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Hotel.Controllers
{

    public class AccomodationsController : Controller
    {
        private readonly AccommodationService _accommodationService;
        private readonly AccommodationTypesService _accommodationTypesService;
        private readonly AccommodationPackagesService _accommodationPackagesService;

        public AccomodationsController(
            AccommodationService accommodationService,
            AccommodationTypesService accommodationTypesService,
            AccommodationPackagesService accommodationPackagesService)
        {
            _accommodationService = accommodationService;
            _accommodationTypesService = accommodationTypesService;
            _accommodationPackagesService = accommodationPackagesService;
        }

        /// <summary>
        /// Nowa wersja metody Index, wzorowana na starej.
        /// </summary>
        /// <param name="accommodationTypeID">ID typu zakwaterowania</param>
        /// <param name="accommodationPackageID">ID pakietu (opcjonalne)</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(int accommodationTypeID, int? accommodationPackageID)
        {

            // 1) Tworzymy model
            var model = new AccommodationsViewModel();

            // 2) Pobierz informacje o typie zakwaterowania (jeśli posiadasz taką metodę w serwisie):
            model.AccommodationType = _accommodationTypesService.GetAccommodationTypeByID(accommodationTypeID);

            // 3) Pobierz listę pakietów dla danego typu (asynchronicznie)
            var packages = await _accommodationPackagesService
                .GetAllAccommodationPackagesByAccommodationTypeAsync(accommodationTypeID);

            Console.WriteLine($"Ilość pobranych pakietów: {packages?.Count ?? 0}");
            model.AccommodationPackages = packages;

            // Jeśli nie znaleziono żadnych pakietów – zwracamy pusty model
            if (model.AccommodationPackages == null || !model.AccommodationPackages.Any())
            {
                model.Accommodations = new List<Accommodation>();
                model.SelectedAccommodationPackageID = null;  // Nie ma co wybrać
                return View(model);
            }

            // 4) Ustal wybrany pakiet (jeśli nie podano, weź pierwszy z listy)
            model.SelectedAccommodationPackageID = accommodationPackageID.HasValue
                ? accommodationPackageID.Value
                : model.AccommodationPackages.First().ID;

            // 5) Pobierz listę zakwaterowań (Accommodation) dla wybranego pakietu
            //    (załóżmy, że w AccommodationService posiadasz taką metodę).
            //    Musi to być np. public Task<List<Accommodation>> GetAllAccommodationsByPackageAsync(int packageId)
            model.Accommodations = await _accommodationService.GetAllAccommodationsByPackageAsync(
                model.SelectedAccommodationPackageID.Value
            );

            // 6) Zwracamy widok z modelem
            return View(model);
        }
        [HttpGet]
        public IActionResult DetailsPackage(int accomodationPackageID)
        {
            var package = _accommodationPackagesService.GetAccommodationPackageByID(accomodationPackageID);
            if (package == null) return NotFound();

            return View("DetailsPackage", new AccommodationPackageDetailsViewModel
            {
                AccommodationPackage = package
            });
        }


        [HttpGet("Accomodations/DetailsAccommodation/{id}")]
        public async Task<IActionResult> DetailsAccommodation(int id)
        {
            try
            {
                var accommodation = await _accommodationService.GetAccommodationByIdAsync(id);
                if (accommodation == null)
                {
                    return NotFound("No accommodation with this ID");
                }
                return View("DetailsAccommodation", accommodation);
            }
            catch (Exception ex)
            {
                // Złap błąd i wypisz do loga
                Console.WriteLine($"ERROR: {ex.Message} \n {ex.StackTrace}");
                // lub logger
                return StatusCode(500, "Something went wrong in the server: " + ex.Message);
            }
        }


        [HttpGet]
		public async Task<IActionResult> AllAccommodations()
		{
			// Przykład: pobierz wszystkie zakwaterowania (możesz użyć serwisu):
			var accommodations = await _accommodationService.GetAllAccommodationsAsync();

			// lub wersja synchroniczna:
			// var accommodations = _accommodationService.GetAllAccommodations();

			// Zwracamy widok:
			return View(accommodations);
		}

        /// <summary>
        /// Odpowiednik starej: public ActionResult CheckAvailability(CheckAccomodationAvailabilityViewModel model)
        /// </summary>
        [HttpPost]
        public IActionResult CheckAvailability(CheckAccommodationAvailabilityViewModel model)
        {
            
            try
            {
                // 1) Złóż tekst wiadomości (np. w oparciu o model).
                string body =
                    $"Zapytanie o dostępność:\n\n" +
                    $"Data przyjazdu: {model.FromDate}\n" +
                    $"Liczba nocy: {model.Duration}\n" +
                    $"Dorośli: {model.NoOfAdults}\n" +
                    $"Dzieci: {model.NoOfChildren}\n" +
                    $"Imię gościa: {model.Name}\n" +
                    $"Email gościa: {model.Email}\n" +
                    $"Uwagi: {model.Notes}\n";

                // 2) Zbuduj obiekt MailMessage
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("dianaliv95@gmail.com", "HotelParadise"),
                    Subject = "Zapytanie o dostępność z formularza",
                    Body = body,
                    IsBodyHtml = false // lub true, jeśli chcesz HTML
                };

                // Adres, na który wysyłasz
                mailMessage.To.Add("dianaliv95@gmail.com");

                // 3) Konfiguracja SMTP
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("dianaliv95@gmail.com", "keofesmtvyqiqprf");
                    smtpClient.EnableSsl = true; // zależy od serwera

                    // 4) Wyślij
                    smtpClient.Send(mailMessage);
                }

                // 5) Po wysyłce możesz np. zwrócić partial z wynikiem
                return View("CheckAvailabilityResult", model);
            }
            catch (Exception ex)
            {
                // Obsłuż błąd wysyłania, np. logowanie
                Console.WriteLine(ex.Message);
                // Możesz zwrócić inny partial z komunikatem błędu
                return StatusCode(500, "Błąd podczas wysyłania maila: " + ex.Message);
            }
        }
        [HttpGet]
        public IActionResult CheckAvailabilityResult(CheckAccommodationAvailabilityViewModel model)
        {
            // tu ewentualnie pobierasz dane (z TempData albo z bazy)
            // i zwracasz widok, np. "CheckAvailabilityResult.cshtml"

            return View("CheckAvailabilityResult", model);
        }
        // Dalsze akcje, np. Details, CheckAvailability itd., analogicznie
    }
}
