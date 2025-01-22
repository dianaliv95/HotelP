using Hotel.Areas.Dashboard.ViewModels;
using HMS.Entities;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin,admin,Recepcja")]
    public class BookingsController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly AccommodationService _accommodationService;
        private readonly RoomService _roomService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            BookingService bookingService,
            AccommodationService accommodationService,
            RoomService roomService,
            ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _accommodationService = accommodationService;
            _roomService = roomService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchTerm = "", int? accommodationId = null)
        {
            var reservations = await _bookingService.GetReservationsAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                reservations = reservations
                    .Where(r => r.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                             || r.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            if (accommodationId.HasValue && accommodationId > 0)
            {
                reservations = reservations
                    .Where(r => r.AccommodationID == accommodationId.Value)
                    .ToList();
            }

            foreach (var reservation in reservations)
            {
                reservation.TotalPrice = _bookingService.CalculateTotalPrice(reservation);
            }

            var model = new BookingsListingModel
            {
                Bookings = reservations,
                SearchTerm = searchTerm,
                SelectedAccommodationId = accommodationId,
                Accommodations = await _accommodationService.GetAllAccommodationsAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(int accommodationId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var rooms = await _bookingService.GetAvailableRoomsAsync(accommodationId, fromDate, toDate);
                return Json(rooms.Select(r => new { r.ID, r.Name }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available rooms.");
                return StatusCode(500, "Wystąpił błąd podczas pobierania dostępnych pokoi.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            if (id.HasValue)
            {
                // Edycja
                var existing = await _bookingService.GetReservationByIdAsync(id.Value);
                if (existing == null)
                    return NotFound("Rezerwacja nie znaleziona.");

                var rooms = await _roomService.GetAllRoomsAsync();

                var model = new BookingActionModel
                {
                    ID = existing.ID,
                    ReservationNumber = existing.ReservationNumber,
                    RoomID = existing.RoomID,
                    Rooms = rooms,

                    FirstName = existing.FirstName,
                    LastName = existing.LastName,
                    ContactPhone = existing.ContactPhone,
                    ContactEmail = existing.ContactEmail,
                    FromDate = existing.DateFrom,
                    DateTo = existing.DateTo,
                    AdultCount = existing.AdultCount,
                    ChildrenCount = existing.ChildrenCount,

                    BreakfastAdults = existing.BreakfastAdults,
                    BreakfastChildren = existing.BreakfastChildren,
                    LunchAdults = existing.LunchAdults,
                    LunchChildren = existing.LunchChildren,
                    DinnerAdults = existing.DinnerAdults,
                    DinnerChildren = existing.DinnerChildren,

                    IsPaid = existing.IsPaid,
                    PaymentMethod = existing.PaymentMethod,
                    Status = existing.Status
                };

                int duration = (model.DateTo - model.FromDate).Days;
                model.Duration = duration;

                var room = rooms.FirstOrDefault(r => r.ID == model.RoomID);
                var tempReservation = new Reservation
                {
                    BreakfastAdults = model.BreakfastAdults,
                    BreakfastChildren = model.BreakfastChildren,
                    LunchAdults = model.LunchAdults,
                    LunchChildren = model.LunchChildren,
                    DinnerAdults = model.DinnerAdults,
                    DinnerChildren = model.DinnerChildren
                };

                model.TotalPrice = _bookingService.CalculateTotalPrice(room, duration, tempReservation);
                model.AllowedStatuses = DetermineAllowedStatuses(existing);

                return PartialView("_BookingForm", model);
            }
            else
            {
                var rooms = await _roomService.GetFilteredRoomsAsync();

                var model = new BookingActionModel
                {
                    ReservationNumber = Guid.NewGuid().ToString().Substring(0, 8),
                    Rooms = rooms,
                    FromDate = DateTime.Now,
                    DateTo = DateTime.Now.AddDays(1),
                    AdultCount = 1,
                    ChildrenCount = 0
                };

                model.Duration = (model.DateTo - model.FromDate).Days;
                model.TotalPrice = 0m;
                model.AllowedStatuses = new List<ReservationStatus>
        {
            ReservationStatus.PreliminaryReservation,
            ReservationStatus.ConfirmedReservation,
            ReservationStatus.SettledStay
        };

                return PartialView("_BookingForm", model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrEdit(BookingActionModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    _logger.LogWarning("Pole: {0}, Błędy: {1}",
                                       kvp.Key,
                                       string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }


            if (!model.RoomID.HasValue)
            {
                return Json(new { success = false, message = "Nie wybrano pokoju." });
            }

            var room = await _roomService.GetRoomByIdAsync(model.RoomID.Value);
            if (room == null)
            {
                return Json(new { success = false, message = "Wybrany pokój nie istnieje w bazie." });
            }

            var accommodation = room.Accommodation;
            if (accommodation == null)
            {
                return Json(new { success = false, message = "Brak zakwaterowania w tym pokoju." });
            }

            if (model.AdultCount > accommodation.MaxAdults)
            {
                return Json(new
                {
                    success = false,
                    message = $"Zbyt wielu dorosłych. Maks = {accommodation.MaxAdults}."
                });
            }
            if (model.ChildrenCount > accommodation.MaxChildren)
            {
                return Json(new
                {
                    success = false,
                    message = $"Zbyt wiele dzieci. Maks = {accommodation.MaxChildren}."
                });
            }
            int totalGuests = model.AdultCount + model.ChildrenCount;
            if (totalGuests > accommodation.MaxGuests)
            {
                return Json(new
                {
                    success = false,
                    message = $"Łączna liczba osób ({totalGuests}) przekracza MaxGuests = {accommodation.MaxGuests}."
                });
            }

            int duration = (model.DateTo - model.FromDate).Days;
            if (duration < 0) duration = 0; 
            var tempReservation = new Reservation
            {
                BreakfastAdults = model.BreakfastAdults,
                BreakfastChildren = model.BreakfastChildren,
                LunchAdults = model.LunchAdults,
                LunchChildren = model.LunchChildren,
                DinnerAdults = model.DinnerAdults,
                DinnerChildren = model.DinnerChildren
            };
            decimal totalPrice = _bookingService.CalculateTotalPrice(room, duration, tempReservation);

            if (model.ID > 0)
            {
                var existing = await _bookingService.GetReservationByIdAsync(model.ID);
                if (existing == null)
                {
                    return Json(new { success = false, message = "Nie znaleziono rezerwacji do edycji." });
                }
                var allowed = DetermineAllowedStatuses(existing);
                if (!allowed.Contains(model.Status))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Wybrany status nie jest dozwolony w bieżącym momencie."
                    });
                }
                existing.RoomID = model.RoomID.Value;
                existing.AccommodationID = (await _roomService
                       .GetRoomByIdAsync(model.RoomID.Value)).AccommodationID;
                existing.FirstName = model.FirstName;
                existing.LastName = model.LastName;
                existing.ContactPhone = model.ContactPhone;
                existing.ContactEmail = model.ContactEmail;
                existing.DateFrom = model.FromDate;
                existing.DateTo = model.DateTo;
                existing.AdultCount = model.AdultCount;
                existing.ChildrenCount = model.ChildrenCount;

                existing.BreakfastAdults = model.BreakfastAdults;
                existing.BreakfastChildren = model.BreakfastChildren;
                existing.LunchAdults = model.LunchAdults;
                existing.LunchChildren = model.LunchChildren;
                existing.DinnerAdults = model.DinnerAdults;
                existing.DinnerChildren = model.DinnerChildren;

                existing.IsPaid = model.IsPaid;
                existing.PaymentMethod = model.PaymentMethod;
                existing.Status = model.Status;



                existing.UpdatedAt = DateTime.Now;



                await _bookingService.UpdateReservationAsync(existing);

                return Json(new { success = true, message = "Rezerwacja zaktualizowana.", totalPrice });
            }
            else
            {
                var newReservation = new Reservation
                {
                    ReservationNumber = model.ReservationNumber,
                    RoomID = model.RoomID.Value,
                    AccommodationID = room.AccommodationID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ContactPhone = model.ContactPhone,
                    ContactEmail = model.ContactEmail,
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

                    IsPaid = model.IsPaid,
                    PaymentMethod = model.PaymentMethod,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _bookingService.CreateReservationAsync(newReservation);

                return Json(new { success = true, message = "Rezerwacja utworzona.", totalPrice });
            }
        }

        private List<ReservationStatus> DetermineAllowedStatuses(Reservation existing)
        {
            var today = DateTime.Today;
            var allowed = new List<ReservationStatus>();

            var finalStatuses = new[]
            {
        ReservationStatus.NoShow,
        ReservationStatus.CompletedSettledStay,
        ReservationStatus.CompletedUnsettledStay
    };

            if (today < existing.DateFrom.Date)
            {
                allowed.Add(ReservationStatus.PreliminaryReservation);
                allowed.Add(ReservationStatus.ConfirmedReservation);
                allowed.Add(ReservationStatus.SettledStay);
            }
            else if (today >= existing.DateFrom.Date && today < existing.DateTo.Date)
            {
                
                allowed.Add(ReservationStatus.PreliminaryReservation);
                allowed.Add(ReservationStatus.ConfirmedReservation);
                allowed.Add(ReservationStatus.SettledStay);
            }
            else
            {
                allowed.Add(ReservationStatus.NoShow);
                allowed.Add(ReservationStatus.CompletedSettledStay);
                allowed.Add(ReservationStatus.CompletedUnsettledStay);
                allowed.Add(ReservationStatus.UnsettledStay);
            }

          
            return allowed.Distinct().ToList();
        }

        [HttpPost]
        public IActionResult CalculateTotalPriceAjax([FromBody] BookingActionModel model)
        {
            var room = _roomService.GetRoomByIdAsync(model.RoomID.Value).Result;
            if (room == null)
                return Json(new { success = false, message = "Pokój nie istnieje." });

            int nights = (model.DateTo - model.FromDate).Days;
            if (nights <= 0)
                return Json(new { success = true, totalPrice = 0 });

            var tempReservation = new Reservation
            {
                BreakfastAdults = model.BreakfastAdults,
                BreakfastChildren = model.BreakfastChildren,
                LunchAdults = model.LunchAdults,
                LunchChildren = model.LunchChildren,
                DinnerAdults = model.DinnerAdults,
                DinnerChildren = model.DinnerChildren
            };

            decimal totalPrice = _bookingService.CalculateTotalPrice(room, nights, tempReservation);

            return Json(new { success = true, totalPrice });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var reservation = await _bookingService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound("Nie znaleziono rezerwacji do usunięcia.");

            var model = new DeleteBookingViewModel
            {
                ID = reservation.ID,
                ReservationNumber = reservation.ReservationNumber,
                FirstName = reservation.FirstName,
                LastName = reservation.LastName
            };
            return PartialView("_DeleteBookingModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBookingConfirmed(int id)
        {
            var reservation = await _bookingService.GetReservationByIdAsync(id);
            if (reservation == null)
                return Json(new { success = false, message = "Rezerwacja nie została znaleziona." });

            bool result = await _bookingService.DeleteReservationAsync(id);
            if (!result)
                return Json(new { success = false, message = "Błąd przy usuwaniu rezerwacji." });

            bool updateResult = await _roomService.RoomisAvailavleAsync(reservation.RoomID);
            if (!updateResult)
            {
                return Json(new { success = false, message = "Rezerwacja usunięta, ale nie udało się zaktualizować statusu pokoju." });
            }

            _logger.LogInformation("Rezerwacja ID {0} usunięta.", reservation.ID);
            return Json(new { success = true, message = "Rezerwacja została usunięta." });
        }
    }
}
