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
        private readonly BookingService _bookingService;         // <--- pojedyncze rezerwacje
        private readonly GroupBookingService _groupBookingService; // <--- grupowe rezerwacje
        private readonly ILogger<RoomsListController> _logger;

        public RoomsListController(
            RoomService roomService,
            BookingService bookingService,
            GroupBookingService groupBookingService,  // wstrzykujemy
            ILogger<RoomsListController> logger)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _groupBookingService = groupBookingService;
            _logger = logger;
        }

        // ========================================================
        // =========== (A) Lista pokoi z filtrowaniem =============
        // ========================================================
        [HttpGet]

        
        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo, string status)
        {
            var rooms = await _roomService.GetAllRoomsAsync();

            // DYNAMICZNE SPRAWDZANIE dostępności:
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                foreach (var r in rooms)
                {
                    bool isFree = await _roomService.IsRoomAvailableAsync(r.ID, dateFrom.Value, dateTo.Value);
                    if (!isFree)
                        r.Status = "Reserved";
                    else if (r.BlockedFrom != null && r.BlockedTo != null)
                        r.Status = "Blocked";
                    else
                        r.Status = "Available";
                }
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Available")
                    rooms = rooms.Where(r => r.Status == "Available").ToList();
                else if (status == "Blocked")
                    rooms = rooms.Where(r => r.Status == "Blocked").ToList();
                else if (status == "Reserved")
                    rooms = rooms.Where(r => r.Status == "Reserved").ToList();
            }

            var model = new RoomsListViewModel
            {
                Rooms = rooms,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SelectedStatus = status
            };
            return View("Index", model);
        }

        // Zmiana statusu pokoju
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int roomId, string newStatus)
        {
            var success = await _roomService.UpdateRoomStatusAsync(roomId, newStatus, null, null);
            if (success)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Nie udało się zaktualizować statusu pokoju." });
        }
        [HttpPost]
        public async Task<IActionResult> BlockRoom(int roomId, DateTime blockFrom, DateTime blockTo)
        {
            if (blockFrom >= blockTo)
            {
                return Json(new { success = false, message = "Data początkowa musi być wcześniejsza niż końcowa." });
            }

            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
                return Json(new { success = false, message = "Pokój nie istnieje." });

            room.BlockedFrom = blockFrom;
            room.BlockedTo = blockTo;
            room.IsBlocked = true;    // lub zrezygnuj z IsBlocked, jeżeli wystarczą daty
            room.Status = "Blocked";

            bool updated = await _roomService.UpdateRoomAsync(room);
            if (!updated)
                return Json(new { success = false, message = "Nie udało się zablokować pokoju." });

            return Json(new { success = true, message = $"Pokój zablokowany od {blockFrom:yyyy-MM-dd} do {blockTo:yyyy-MM-dd}." });
        }

        // (A4) Odblokowanie pokoju
        [HttpPost]
        public async Task<IActionResult> UnblockRoom(int roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
                return Json(new { success = false, message = "Pokój nie istnieje." });

            room.BlockedFrom = null;
            room.BlockedTo = null;
            room.IsBlocked = false;
            room.Status = "Available"; // lub "Available" z bazy

            bool updated = await _roomService.UpdateRoomAsync(room);
            if (!updated)
                return Json(new { success = false, message = "Nie udało się odblokować pokoju." });

            return Json(new { success = true, message = "Pokój został odblokowany." });
        }


        // ========================================================
        // ========== (B) POJEDYNCZA REZERWACJA (Booking) =========
        // ========================================================
        // GET: formularz do utworzenia rezerwacji pojedynczej
        [HttpGet]
        public async Task<IActionResult> CreateReservation(int roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null) return NotFound("Pokój nie znaleziony.");

            // Model do partiala
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

            // partial ~Areas/Dashboard/Views/Bookings/_BookingForm.cshtml
            return PartialView("~/Areas/Dashboard/Views/Bookings/_BookingForm.cshtml", model);
        }

        // POST: tworzenie rezerwacji pojedynczej
        [HttpPost]
        public async Task<IActionResult> CreateReservation(BookingActionModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                _logger.LogWarning("Wystąpiły błędy walidacji: {0}", string.Join("; ", errors));
                return Json(new { success = false, message = "Nieprawidłowe dane rezerwacji.", errors });
            }

            var room = await _roomService.GetRoomByIdAsync(model.RoomID.Value);
            if (room == null)
                return Json(new { success = false, message = "Pokój nie istnieje." });

            // Sprawdzenie limitów
            var accommodation = room.Accommodation;
            if (accommodation == null)
                return Json(new { success = false, message = "Brak zakwaterowania w pokoju." });

            if (model.AdultCount > accommodation.MaxAdults)
            {
                ModelState.AddModelError("AdultCount",
                    $"Przekroczono maksymalną liczbę dorosłych: {accommodation.MaxAdults}.");
            }
            if (model.ChildrenCount > accommodation.MaxChildren)
            {
                ModelState.AddModelError("ChildrenCount",
                    $"Przekroczono maksymalną liczbę dzieci: {accommodation.MaxChildren}.");
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new { success = false, message = "Walidacja nie powiodła się", errors });
            }

            // Tworzymy rezerwację
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

            // Zapis w bazie
            bool result = await _bookingService.CreateReservationAsync(newReservation);
            if (result)
            {
                // Ustawiamy status pokoju = "Reserved"
                await _roomService.UpdateRoomStatusAsync(newReservation.RoomID, "Reserved");
                return Json(new { success = true, message = $"Rezerwacja utworzona! Nr: {model.ReservationNumber}" });
            }
            else
            {
                return Json(new { success = false, message = "Nie udało się utworzyć rezerwacji (kolizja?)." });
            }
        }



        // ========================================================
        // ========= (C) REZERWACJA GRUPOWA (GroupBooking) ========
        // ========================================================
        //
        //  - metody analogiczne do GroupBookingsController:
        //    (1) Lista (opcjonalna, lub można w ogóle pominąć)
        //    (2) CreateOrEdit (GET)
        //    (3) CreateOrEdit (POST)
        //    (4) Delete (GET)
        //    (5) DeleteConfirmed (POST)
        //    …itd.
        // ========================================================

        /// <summary>
        /// (C1) Lista rezerwacji GRUPOWYCH – o ile chcesz w tym samym kontrolerze
        ///       (opcjonalnie).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GroupIndex(string searchTerm = "")
        {
            // Lista rezerwacji grupowych
            var groupReservations = await _groupBookingService.GetAllAsync();

            // Filtr
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                groupReservations = groupReservations
                    .Where(gr => gr.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                              || gr.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Obliczamy cenę (pokoje + posiłki)
            foreach (var gr in groupReservations)
            {
                gr.TotalPrice = _groupBookingService.CalculateTotalPrice(gr);
            }

            // (opcjonalnie) inny widok: /Areas/Dashboard/Views/RoomsList/GroupIndex.cshtml
            // lub /Areas/Dashboard/Views/GroupBookings/Index.cshtml
            return View("GroupIndex", groupReservations);
        }

        /// <summary>
        /// (C2) GET: formularz tworzenia / edycji rezerwacji GRUPOWEJ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateGroupReservation(int? id)
        {
            if (id.HasValue)
            {
                // EDYCJA
                var existing = await _groupBookingService.GetByIdAsync(id.Value);
                if (existing == null) return NotFound("Nie znaleziono rezerwacji grupowej do edycji.");

                var model = new GroupBookingActionModel
                {
                    ID = existing.ID,
                    ReservationNumber = existing.ReservationNumber,
                    FirstName = existing.FirstName,
                    LastName = existing.LastName,
                    FromDate = existing.FromDate,
                    ToDate = existing.ToDate,
                    AdultCount = existing.AdultCount,
                    ChildrenCount = existing.ChildrenCount,

                    // Posiłki
                    BreakfastAdults = existing.BreakfastAdults,
                    BreakfastChildren = existing.BreakfastChildren,
                    LunchAdults = existing.LunchAdults,
                    LunchChildren = existing.LunchChildren,
                    DinnerAdults = existing.DinnerAdults,
                    DinnerChildren = existing.DinnerChildren,

                    IsPaid = existing.IsPaid,
                    PaymentMethod = (PaymentsMethod?)existing.PaymentMethod,
                    RStatus = existing.RStatus,
                    ContactPhone = existing.ContactPhone,
                    ContactEmail = existing.ContactEmail,

                    // Pokoje w rezerwacji
                    SelectedRoomIDs = existing.GroupReservationRooms.Select(rr => rr.RoomID).ToList()
                };

                // Pokoje: te, które są Available lub już w rezerwacji
                var allRooms = await _roomService.GetAllRoomsAsync();
                var availableRooms = allRooms
                    .Where(r => r.Status == "Available" || model.SelectedRoomIDs.Contains(r.ID))
                    .ToList();

                model.AvailableRooms = availableRooms;

                // wstępne obliczenie ceny
                var tempGR = new GroupReservation
                {
                    FromDate = model.FromDate,
                    ToDate = model.ToDate,
                    BreakfastAdults = model.BreakfastAdults,
                    BreakfastChildren = model.BreakfastChildren,
                    LunchAdults = model.LunchAdults,
                    LunchChildren = model.LunchChildren,
                    DinnerAdults = model.DinnerAdults,
                    DinnerChildren = model.DinnerChildren,
                    GroupReservationRooms = availableRooms
                        .Where(r => model.SelectedRoomIDs.Contains(r.ID))
                        .Select(r => new GroupReservationRoom { Room = r })
                        .ToList()
                };
                model.TotalPrice = _groupBookingService.CalculateTotalPrice(tempGR);

                // partial _GroupBookingForm.cshtml
                return PartialView("~/Areas/Dashboard/Views/GroupBookings/_GroupBookingForm.cshtml", model);
            }
            else
            {
                // NOWA rezerwacja grupowa
                var allRooms = await _roomService.GetAllRoomsAsync();
                var availableRooms = allRooms.Where(r => r.Status == "Available").ToList();

                var model = new GroupBookingActionModel
                {
                    ReservationNumber = Guid.NewGuid().ToString().Substring(0, 8),
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today.AddDays(1),
                    AdultCount = 1,
                    ChildrenCount = 0,
                    AvailableRooms = availableRooms
                };

                return PartialView("~/Areas/Dashboard/Views/GroupBookings/_GroupBookingForm.cshtml", model);
            }
        }

        /// <summary>
        /// (C3) POST: tworzenie / edycja rezerwacji GRUPOWEJ
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateGroupReservation(GroupBookingActionModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Nieprawidłowe dane w formularzu rezerwacji grupowej." });

            if (model.SelectedRoomIDs == null || !model.SelectedRoomIDs.Any())
                return Json(new { success = false, message = "Wybierz przynajmniej jeden pokój dla rezerwacji grupowej." });

            // Budujemy obiekt
            var groupRes = new GroupReservation
            {
                ID = model.ID,
                ReservationNumber = model.ReservationNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                AdultCount = model.AdultCount,
                ChildrenCount = model.ChildrenCount,

                // Posiłki
                BreakfastAdults = model.BreakfastAdults,
                BreakfastChildren = model.BreakfastChildren,
                LunchAdults = model.LunchAdults,
                LunchChildren = model.LunchChildren,
                DinnerAdults = model.DinnerAdults,
                DinnerChildren = model.DinnerChildren,

                IsPaid = model.IsPaid,
                PaymentMethod = (PaymentMethod?)model.PaymentMethod,
                RStatus = model.RStatus,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                UpdatedAt = DateTime.Now
            };

            // Nowa czy edycja?
            if (model.ID > 0)
            {
                // Edycja
                bool ok = await _groupBookingService.UpdateAsync(groupRes, model.SelectedRoomIDs);
                if (!ok)
                    return Json(new { success = false, message = "Błąd podczas aktualizacji rezerwacji grupowej." });

                // Po zapisaniu -> liczymy cenę
                var updated = await _groupBookingService.GetByIdAsync(model.ID);
                decimal totalPrice = _groupBookingService.CalculateTotalPrice(updated);

                return Json(new { success = true, message = "Rezerwacja grupowa zaktualizowana pomyślnie.", totalPrice });
            }
            else
            {
                // Nowa
                groupRes.CreatedAt = DateTime.Now;
                groupRes.UpdatedAt = DateTime.Now;

                bool ok = await _groupBookingService.CreateAsync(groupRes, model.SelectedRoomIDs);
                if (!ok)
                    return Json(new { success = false, message = "Błąd przy tworzeniu nowej rezerwacji grupowej." });

                var created = await _groupBookingService.GetByIdAsync(groupRes.ID);
                decimal totalPrice = _groupBookingService.CalculateTotalPrice(created);

                return Json(new { success = true, message = "Nowa rezerwacja grupowa utworzona.", totalPrice });
            }
        }

        /// <summary>
        /// (C4) GET: potwierdzenie usunięcia rezerwacji grupowej
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteGroupReservation(int id)
        {
            var existing = await _groupBookingService.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Nie znaleziono rezerwacji grupowej do usunięcia.");

            var model = new GroupBookingActionModel
            {
                ID = existing.ID,
                FirstName = existing.FirstName,
                LastName = existing.LastName
            };
            // partial _DeleteGroupBookingModal.cshtml
            return PartialView("~/Areas/Dashboard/Views/GroupBookings/_DeleteGroupBookingModal.cshtml", model);
        }

        /// <summary>
        /// (C5) POST: usunięcie rezerwacji grupowej (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupReservationConfirmed(int id)
        {
            var existing = await _groupBookingService.GetByIdAsync(id);
            if (existing == null)
                return Json(new { success = false, message = "Nie znaleziono rezerwacji grupowej do usunięcia." });

            bool result = await _groupBookingService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Błąd przy usuwaniu rezerwacji grupowej." });

            return Json(new { success = true, message = "Rezerwacja grupowa została usunięta." });
        }


        // ========================================================
        // (D) Inne akcje lub metody pomocnicze...
        // ========================================================
       





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
