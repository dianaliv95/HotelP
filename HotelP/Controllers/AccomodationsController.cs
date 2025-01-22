using HMS.Entities;
using HMS.Services;
using Hotel.ViewModels; 
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

        
        [HttpGet]
        public async Task<IActionResult> Index(int accommodationTypeID, int? accommodationPackageID)
        {

            var model = new AccommodationsViewModel();

            model.AccommodationType = _accommodationTypesService.GetAccommodationTypeByID(accommodationTypeID);

            var packages = await _accommodationPackagesService
                .GetAllAccommodationPackagesByAccommodationTypeAsync(accommodationTypeID);

            Console.WriteLine($"Ilość pobranych pakietów: {packages?.Count ?? 0}");
            model.AccommodationPackages = packages;

            if (model.AccommodationPackages == null || !model.AccommodationPackages.Any())
            {
                model.Accommodations = new List<Accommodation>();
                model.SelectedAccommodationPackageID = null; 
                return View(model);
            }

            model.SelectedAccommodationPackageID = accommodationPackageID.HasValue
                ? accommodationPackageID.Value
                : model.AccommodationPackages.First().ID;

           
            model.Accommodations = await _accommodationService.GetAllAccommodationsByPackageAsync(
                model.SelectedAccommodationPackageID.Value
            );

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
                Console.WriteLine($"ERROR: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(500, "Something went wrong in the server: " + ex.Message);
            }
        }


        [HttpGet]
		public async Task<IActionResult> AllAccommodations()
		{
			var accommodations = await _accommodationService.GetAllAccommodationsAsync();

			
			return View(accommodations);
		}

        
        [HttpPost]
        public IActionResult CheckAvailability(CheckAccommodationAvailabilityViewModel model)
        {
            
            try
            {
                string body =
                    $"Zapytanie o dostępność:\n\n" +
                    $"Data przyjazdu: {model.FromDate}\n" +
                    $"Liczba nocy: {model.Duration}\n" +
                    $"Dorośli: {model.NoOfAdults}\n" +
                    $"Dzieci: {model.NoOfChildren}\n" +
                    $"Imię gościa: {model.Name}\n" +
                    $"Email gościa: {model.Email}\n" +
                    $"Uwagi: {model.Notes}\n";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("dianaliv95@gmail.com", "HotelParadise"),
                    Subject = "Zapytanie o dostępność z formularza",
                    Body = body,
                    IsBodyHtml = false 
                };

                mailMessage.To.Add("dianaliv95@gmail.com");

                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("dianaliv95@gmail.com", "keofesmtvyqiqprf");
                    smtpClient.EnableSsl = true; 

                    smtpClient.Send(mailMessage);
                }

                return View("CheckAvailabilityResult", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Błąd podczas wysyłania maila: " + ex.Message);
            }
        }
        [HttpGet]
        public IActionResult CheckAvailabilityResult(CheckAccommodationAvailabilityViewModel model)
        {
            

            return View("CheckAvailabilityResult", model);
        }
    }
}
