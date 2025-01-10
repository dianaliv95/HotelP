using Hotel.Areas.Dashboard.ViewModels;
using HMS.Entities;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Areas.Dashboard.Controllers
{
	[Area("Dashboard")]
	public class AccommodationsController : Controller
	{
		private readonly AccommodationService _accommodationService;
		private readonly RoomService _roomService;

		private readonly AccommodationPackagesService _packageService;
		private readonly ILogger<AccommodationsController> _logger;

		public AccommodationsController(
			AccommodationService accommodationService,
			AccommodationPackagesService packageService,
			ILogger<AccommodationsController> logger, RoomService roomService)
		{
			_accommodationService = accommodationService;
			_packageService = packageService;
			_roomService = roomService;

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}


		[HttpGet]
		public async Task<IActionResult> ViewAvailableRooms(DateTime fromDate, DateTime toDate, int accommodationId)
		{
			_logger.LogInformation("Dostępne pokoje dla zakwaterowania ID: {AccommodationId} na zakres dat {FromDate} - {ToDate}", accommodationId, fromDate, toDate);

			var accommodation = await _accommodationService.GetAccommodationByIdAsync(accommodationId);

			if (accommodation == null)
			{
				_logger.LogWarning("Zakwaterowanie o ID {AccommodationId} nie znalezione.", accommodationId);
				return NotFound("Zakwaterowanie nie zostało znalezione.");
			}

			var availableRooms = await _roomService.GetAvailableRoomsAsync(accommodationId, fromDate, toDate);

			var model = new AvailableRoomsViewModel
			{
				Accommodation = accommodation,
				FromDate = fromDate,
				ToDate = toDate,
				AvailableRooms = availableRooms
			};

			return PartialView("_AvailableRooms", model);
		}


		[HttpPost("Dashboard/Accommodation/UpdateRoomStatus")]
		public async Task<IActionResult> UpdateRoomStatus(int roomId, string status, DateTime? availableFrom, DateTime? availableTo)
		{
			_logger.LogInformation("Otrzymano żądanie aktualizacji statusu pokoju ID: {RoomId}", roomId);

			var success = await _roomService.UpdateRoomStatusAsync(roomId, status, availableFrom, availableTo);
			if (success)
			{
				_logger.LogInformation("Status pokoju ID: {RoomId} został zaktualizowany.", roomId);
				return Json(new { success = true });
			}
			else
			{
				_logger.LogWarning("Nie udało się zaktualizować statusu pokoju ID: {RoomId}.", roomId);
				return Json(new { success = false, message = "Nie udało się zaktualizować statusu pokoju." });
			}
		}



		[HttpGet]
		public async Task<IActionResult> Index(string searchTerm, int? selectedPackageId)
		{
			var accommodations = selectedPackageId.HasValue
				? await _accommodationService.GetAccommodationsByPackageAsync(selectedPackageId.Value)
				: await _accommodationService.SearchAccommodationsAsync(searchTerm);

			var packages = await _packageService.GetAllAccommodationPackagesAsync();

			var model = new AccommodationsListingModel
			{
				Accommodations = accommodations,
				SearchTerm = searchTerm,
				SelectedPackageId = selectedPackageId,
				Packages = packages
			};

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Action(int? id)
		{
			var packages = await _packageService.GetAllAccommodationPackagesAsync();
			if (packages == null || !packages.Any())
			{
				_logger.LogWarning("Brak pakietów zakwaterowania w bazie danych.");
			}

			var model = new AccommodationActionModel
			{
				ID = id ?? 0,
				Name = string.Empty,
				Description = string.Empty,
				AccommodationPackageID = 0,
				Packages = packages,
				NumberOfRooms = 1,
				MaxGuests = 1,
				MaxAdults = 1,
				MaxChildren = 0
			};

			if (id.HasValue)
			{
				var accommodation = await _accommodationService.GetAccommodationByIdAsync(id.Value);
				if (accommodation != null)
				{
					model.ID = accommodation.ID;
					model.Name = accommodation.Name;
					model.Description = accommodation.Description;
					model.AccommodationPackageID = accommodation.AccommodationPackageID;
					model.MaxGuests = accommodation.MaxGuests;
					model.MaxAdults = accommodation.MaxAdults;
					model.MaxChildren = accommodation.MaxChildren;
				}
			}

			return PartialView("_Action", model);
		}

		[HttpPost]
		public async Task<IActionResult> Action(AccommodationActionModel model)
		{
			_logger.LogInformation("Rozpoczęcie obsługi żądania Action dla modelu: {@Model}", model);
			try
			{
				if (!ModelState.IsValid)
				{
					var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
					foreach (var error in errors)
					{
						_logger.LogWarning("Validation Error: {Error}", error);
					}
					return Json(new { success = false, message = "Nieprawidłowe dane wejściowe.", errors = errors });
				}

				if (model.ID > 0)
				{
					_logger.LogInformation("Aktualizacja istniejącego zakwaterowania o ID: {ID}", model.ID);
					var accommodation = await _accommodationService.GetAccommodationByIdAsync(model.ID);
					if (accommodation == null)
					{
						_logger.LogWarning("Zakwaterowanie o ID {ID} nie istnieje.", model.ID);
						return Json(new { success = false, message = "Zakwaterowanie nie istnieje." });
					}

					accommodation.Name = model.Name;
					accommodation.Description = model.Description;
					accommodation.AccommodationPackageID = model.AccommodationPackageID;
					accommodation.MaxGuests = model.MaxGuests;
					accommodation.MaxAdults = model.MaxAdults;
					accommodation.MaxChildren = model.MaxChildren;

					var updateResult = await _accommodationService.UpdateAccommodationAsync(accommodation);
					if (!updateResult)
					{
						_logger.LogError("Nie udało się zaktualizować zakwaterowania o ID: {ID}", model.ID);
						return Json(new { success = false, message = "Nie udało się zaktualizować zakwaterowania." });
					}
				}
				else
				{
					_logger.LogInformation("Tworzenie nowego zakwaterowania");
					var newAccommodation = new Accommodation
					{
						Name = model.Name,
						Description = model.Description,
						AccommodationPackageID = model.AccommodationPackageID,
						MaxGuests = model.MaxGuests,
						MaxAdults = model.MaxAdults,
						MaxChildren = model.MaxChildren
					};

					var saveResult = await _accommodationService.CreateAccommodationAsync(newAccommodation, model.NumberOfRooms);
					if (!saveResult)
					{
						_logger.LogError("Nie udało się zapisać zakwaterowania.");
						return Json(new { success = false, message = "Nie udało się zapisać zakwaterowania." });
					}
				}

				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Wystąpił błąd podczas obsługi żądania Action");
				return Json(new { success = false, message = "Wystąpił nieoczekiwany błąd." });
			}
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			var accommodation = await _accommodationService.GetAccommodationByIdAsync(id);

			if (accommodation == null)
			{
				return NotFound("Nie znaleziono zakwaterowania.");
			}

			var model = new AccommodationActionModel
			{
				ID = accommodation.ID,
				Name = accommodation.Name,
				Description = accommodation.Description,
				AccommodationPackageID = accommodation.AccommodationPackageID
			};

			return PartialView("_Delete", model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(AccommodationActionModel model)
		{
			if (model == null || model.ID <= 0)
			{
				return Json(new { success = false, message = "Nieprawidłowe dane wejściowe." });
			}

			var result = await _accommodationService.DeleteAccommodationAsync(model.ID);
			if (result)
			{
				return Json(new { success = true, message = "Zakwaterowanie zostało usunięte." });
			}

			return Json(new { success = false, message = "Błąd podczas usuwania zakwaterowania." });
		}
	}
}
