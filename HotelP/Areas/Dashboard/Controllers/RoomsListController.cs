using Hotel.Areas.Dashboard.ViewModels;
using HMS.Entities;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class RoomsListController : Controller
    {
        private readonly RoomService _roomService;
        private readonly BookingService _bookingService;
        private readonly ILogger<RoomsListController> _logger;

        public RoomsListController(
            RoomService roomService,
            BookingService bookingService,
            ILogger<RoomsListController> logger)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo, string status)
        {
            var rooms = await _roomService.GetAllRoomsAsync();

            // Filtry statusu
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Available")
                    rooms = rooms.Where(r => r.Status == "Available").ToList();
                else if (status == "Blocked")
                    rooms = rooms.Where(r => r.Status == "Blocked").ToList();
                else if (status == "Reserved")
                    rooms = rooms.Where(r => r.IsReserved).ToList();
            }

            // Filtry dat (jeśli w danym kontrolerze tak działają)
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                rooms = rooms.Where(r =>
                    (r.AvailableFrom == null || r.AvailableFrom <= dateFrom) &&
                    (r.AvailableTo == null || r.AvailableTo >= dateTo)
                ).ToList();
            }

            var model = new RoomsListViewModel
            {
                Rooms = rooms,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SelectedStatus = status
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int roomId, string newStatus)
        {
            var success = await _roomService.UpdateRoomStatusAsync(roomId, newStatus, null, null);
            if (success)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Nie udało się zaktualizować statusu pokoju." });
        }

        // GET: formularz rezerwacji (partial)
        [HttpGet]
        public async Task<IActionResult> CreateReservation(int roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null) return NotFound("Pokój nie znaleziony.");

            // Tworzymy model
            var model = new BookingActionModel
            {
                RoomID = room.ID,
                Rooms = new List<Room> { room },
                ReservationNumber = Guid.NewGuid().ToString().Substring(0, 8),
                FromDate = DateTime.Now.Date,
                DateTo = DateTime.Now.Date.AddDays(1),
                Duration = 1,
                AdultCount = 1,
                ChildrenCount = 0,
                Status = ReservationStatus.PreliminaryReservation
            };

            // partial `_BookingForm.cshtml` w folderze "Bookings":
            return PartialView("~/Areas/Dashboard/Views/Bookings/_BookingForm.cshtml", model);
        }

        // POST: tworzenie rezerwacji
        [HttpPost]
        public async Task<IActionResult> CreateReservation(BookingActionModel model)
        {
            // 1) Walidacja
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                foreach (var error in errors)
                {
                    _logger.LogWarning("ModelState error: {Error}", error);
                }
                return Json(new { success = false, message = "Nieprawidłowe dane rezerwacji.", errors });
            }

            // 2) Pobieramy pokój
            var room = await _roomService.GetRoomByIdAsync(model.RoomID.Value);
            if (room == null)
                return Json(new { success = false, message = "Pokój nie istnieje." });

            var accommodation = room.Accommodation;
            if (accommodation == null)
                return Json(new { success = false, message = "Brak informacji o zakwaterowaniu." });

            // 3) Walidacja max dorosłych/dzieci
            if (model.AdultCount > accommodation.MaxAdults)
            {
                ModelState.AddModelError("AdultCount",
                    $"Przekroczono dopuszczalną liczbę dorosłych ({accommodation.MaxAdults}).");
            }
            if (model.ChildrenCount > accommodation.MaxChildren)
            {
                ModelState.AddModelError("ChildrenCount",
                    $"Przekroczono dopuszczalną liczbę dzieci ({accommodation.MaxChildren}).");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                foreach (var error in errors)
                {
                    _logger.LogWarning("ModelState error: {Error}", error);
                }
                return Json(new { success = false, message = "Walidacja nie powiodła się", errors });
            }

            // 4) Tworzymy obiekt rezerwacji: zamiast bool Breakfast/Lunch/Dinner
            // używamy pol liczbowych BreakfastAdults, LunchAdults, DinnerAdults itd.
            var newReservation = new Reservation
            {
                ReservationNumber = model.ReservationNumber,
                RoomID = room.ID,
                AccommodationID = room.AccommodationID,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateFrom = model.FromDate,
                DateTo = model.DateTo,
                AdultCount = model.AdultCount,
                ChildrenCount = model.ChildrenCount,

                BreakfastAdults = model.BreakfastAdults,
                BreakfastChildren = model.BreakfastChildren,
                LunchAdults = model.LunchAdults,
                LunchChildren = model.LunchChildren,
                DinnerAdults = model.DinnerAdults,
                DinnerChildren = model.DinnerChildren,

                Status = model.Status,
                IsPaid = model.IsPaid,
                PaymentMethod = model.PaymentMethod,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 5) Zapis w bazie przez BookingService
            bool result = await _bookingService.CreateReservationAsync(newReservation);
            if (result)
            {
                // Aktualizacja statusu pokoju na "Reserved"
                await _roomService.UpdateRoomStatusAsync(newReservation.RoomID, "Reserved");

                return Json(new { success = true, message = $"Rezerwacja utworzona! Nr: {model.ReservationNumber}" });
            }
            else
            {
                return Json(new { success = false, message = "Nie udało się utworzyć rezerwacji." });
            }
        }

        [HttpGet]
        public IActionResult CreateAccommodation()
        {
            // Przykładowy partial
            var model = new
            {
                Rooms = new List<RoomViewModel>
                {
                    new RoomViewModel() // Domyślnie jeden pokój
                }
            };
            return PartialView("~/Areas/Dashboard/Views/RoomsList/_CreateAccommodationForm.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(int accommodationId, DateTime fromDate, DateTime toDate)
        {
            if (accommodationId <= 0)
            {
                return BadRequest("Nieprawidłowe ID zakwaterowania.");
            }

            var rooms = await _roomService.GetAvailableRoomsAsync(accommodationId, fromDate, toDate);
            var roomList = rooms.Select(r => new {
                r.ID,
                r.Name,
                PricePerNight = r.PricePerNight.ToString("C", new CultureInfo("pl-PL"))
            });

            return Json(roomList);
        }
    }
}
