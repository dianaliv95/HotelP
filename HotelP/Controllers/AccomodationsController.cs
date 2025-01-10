using HMS.Entities;
using HMS.Services;
using Hotel.ViewModels; // tu znajduje się klasa AccomodationsViewModel (zmień, jeśli u Ciebie inaczej)
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public async Task<IActionResult> Details(int accommodationPackageID)
        {
			// Tworzymy model. Zakładamy, że w nowej wersji jest to: AccommodationPackageDetailsViewModel
			var model = new AccommodationPackageDetailsViewModel
			{
				// Jeśli posiadasz metodę asynchroniczną:
				AccommodationPackage = _accommodationPackagesService.GetAccommodationPackageByID(accommodationPackageID)
			};

            // (lub w wersji synchronicznej:)
            // model.AccommodationPackage = _accommodationPackagesService.GetAccommodationPackageByID(accommodationPackageID);

            if (model.AccommodationPackage == null)
            {
                // Możesz zwrócić np. NotFound() lub redirect
                return NotFound("Package not found");
            }

            return View(model);
        }

        /// <summary>
        /// Odpowiednik starej: public ActionResult CheckAvailability(CheckAccomodationAvailabilityViewModel model)
        /// </summary>
        [HttpPost]
        public IActionResult CheckAvailability(CheckAccommodationAvailabilityViewModel model)
        {
            // Logika sprawdzania
            // ...
            // Zwracamy np. partial view z wynikiem:
            return PartialView("_CheckAvailabilityResult", model);
        }
        // Dalsze akcje, np. Details, CheckAvailability itd., analogicznie
    }
}
