using HMS.Entities;
using HMS.Services;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class GroupBookingsController : Controller
    {
        private readonly GroupBookingService _groupBookingService;
        private readonly RoomService _roomService;
        private readonly ILogger<GroupBookingsController> _logger;

        public GroupBookingsController(
            GroupBookingService groupBookingService,
            RoomService roomService,
            ILogger<GroupBookingsController> logger)
        {
            _groupBookingService = groupBookingService;
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// Lista rezerwacji grupowych + opcjonalny filtr po imieniu/nazwisku.
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var groupReservations = await _groupBookingService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Filtrowanie po imieniu/nazwisku
                groupReservations = groupReservations
                    .Where(r => r.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                             || r.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Doliczamy cenę (z posiłkami) do każdej rezerwacji
            foreach (var gr in groupReservations)
            {
                gr.TotalPrice = _groupBookingService.CalculateTotalPrice(gr);
            }

            return View(groupReservations);
            // Widok: /Areas/Dashboard/Views/GroupBookings/Index.cshtml
        }

        /// <summary>
        /// GET: formularz (partial) tworzenia lub edycji rezerwacji grupowej.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            if (id.HasValue)
            {
                // =============== Tryb edycji ===============
                var existing = await _groupBookingService.GetByIdAsync(id.Value);
                if (existing == null)
                    return NotFound("Nie znaleziono rezerwacji grupowej.");

                // Uzupełniamy model
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
                    SelectedRoomIDs = existing.GroupReservationRooms
                        .Select(x => x.RoomID)
                        .ToList()
                };

                // Pobierz listę wszystkich pokoi
                var allRooms = await _roomService.GetAllRoomsAsync();

                // Pokój jest widoczny, jeśli: status=="Available" LUB jest już w rezerwacji
                var availableRooms = allRooms
                    .Where(r => r.Status == "Available" || model.SelectedRoomIDs.Contains(r.ID))
                    .ToList();

                model.AvailableRooms = availableRooms;

                // Wstępna kalkulacja ceny do wyświetlenia
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

                // Ustalenie dozwolonych statusów
                model.AllowedGroupStatuses = DetermineAllowedGroupStatuses(existing);

                return PartialView("_GroupBookingForm", model);
            }
            else
            {
                // =============== Tryb tworzenia nowej rezerwacji ===============
                var rooms = await _roomService.GetAllRoomsAsync();
                var availableRooms = rooms.Where(r => r.Status == "Available").ToList();

                var model = new GroupBookingActionModel
                {
                    ReservationNumber = Guid.NewGuid().ToString().Substring(0, 8),
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today.AddDays(1),
                    AdultCount = 1,
                    ChildrenCount = 0,
                    BreakfastAdults = 0,
                    BreakfastChildren = 0,
                    LunchAdults = 0,
                    LunchChildren = 0,
                    DinnerAdults = 0,
                    DinnerChildren = 0,
                    AvailableRooms = availableRooms
                };

                // Dla nowej rezerwacji -> statusy wstępne
                model.AllowedGroupStatuses = new List<GroupReservationStatus>
        {
            GroupReservationStatus.PreliminaryReservation,
            GroupReservationStatus.ConfirmedReservation
        };

                return PartialView("_GroupBookingForm", model);
            }
        }


        /// <summary>
        /// POST: zapis tworzenia / edycji rezerwacji grupowej (AJAX).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrEdit(GroupBookingActionModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Nieprawidłowe dane formularza." });

            if (model.SelectedRoomIDs == null || !model.SelectedRoomIDs.Any())
                return Json(new { success = false, message = "Wybierz przynajmniej jeden pokój." });

            // Tworzymy tymczasowy obiekt GroupReservation do zapisu
            var groupReservation = new GroupReservation
            {
                ID = model.ID,
                ReservationNumber = model.ReservationNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                AdultCount = model.AdultCount,
                ChildrenCount = model.ChildrenCount,
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

            if (model.ID > 0)
            {
                // Edycja
                var existing = await _groupBookingService.GetByIdAsync(model.ID);
                if (existing == null)
                    return Json(new { success = false, message = "Rezerwacja nie znaleziona." });

                // (opcjonalnie) sprawdź, czy model.RStatus znajduje się w dozwolonych:
                var allowed = DetermineAllowedGroupStatuses(existing);
                if (!allowed.Contains(model.RStatus))
                {
                    return Json(new { success = false, message = "Ten status jest niedozwolony w obecnym momencie." });
                }

                bool ok = await _groupBookingService.UpdateAsync(groupReservation, model.SelectedRoomIDs);
                if (!ok)
                    return Json(new { success = false, message = "Błąd przy aktualizacji rezerwacji grupowej." });

                var updated = await _groupBookingService.GetByIdAsync(model.ID);
                decimal totalPrice = _groupBookingService.CalculateTotalPrice(updated);
                return Json(new { success = true, message = "Rezerwacja grupowa zaktualizowana.", totalPrice });
            }
            else
            {
                // Nowa
                groupReservation.CreatedAt = DateTime.Now;
                groupReservation.UpdatedAt = DateTime.Now;

                bool ok = await _groupBookingService.CreateAsync(groupReservation, model.SelectedRoomIDs);
                if (!ok)
                    return Json(new { success = false, message = "Błąd przy tworzeniu rezerwacji grupowej." });

                var created = await _groupBookingService.GetByIdAsync(groupReservation.ID);
                decimal totalPrice = _groupBookingService.CalculateTotalPrice(created);

                return Json(new { success = true, message = "Rezerwacja grupowa utworzona.", totalPrice });
            }
        }

        private List<GroupReservationStatus> DetermineAllowedGroupStatuses(GroupReservation gr)
        {
            var today = DateTime.Today;
            var allowed = new List<GroupReservationStatus>();

            // Wyróżnijmy finalne statusy
            var finalStatuses = new[]
            {
        GroupReservationStatus.NoShow,
        GroupReservationStatus.CompletedSettledStay,
        GroupReservationStatus.CompletedUnsettledStay
    };

            if (today < gr.FromDate.Date)
            {
                // Rezerwacja w przyszłości
                allowed.Add(GroupReservationStatus.PreliminaryReservation);
                allowed.Add(GroupReservationStatus.ConfirmedReservation);
            }
            else if (today >= gr.FromDate.Date && today < gr.ToDate.Date)
            {
                // Rezerwacja w trakcie
                allowed.Add(GroupReservationStatus.PreliminaryReservation);
                allowed.Add(GroupReservationStatus.ConfirmedReservation);
                allowed.Add(GroupReservationStatus.SettledStay);
                allowed.Add(GroupReservationStatus.UnsettledStay);
            }
            else
            {
                // Dzień wyjazdu lub po
                allowed.AddRange(finalStatuses);
                allowed.Add(GroupReservationStatus.UnsettledStay);
            }

            return allowed.Distinct().ToList();
        }
        /// <summary>
        /// GET: wyświetla partial z potwierdzeniem usunięcia.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteGroupBooking(int id)
        {
            var existing = await _groupBookingService.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Nie znaleziono rezerwacji do usunięcia.");

            var model = new GroupBookingActionModel
            {
                ID = existing.ID,
                FirstName = existing.FirstName,
                LastName = existing.LastName
            };
            return PartialView("_DeleteGroupBookingModal", model);
        }

        /// <summary>
        /// POST: potwierdzone usuwanie rezerwacji (AJAX).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupBookingConfirmed(int id)
        {
            var existing = await _groupBookingService.GetByIdAsync(id);
            if (existing == null)
                return Json(new { success = false, message = "Rezerwacja grupowa nie znaleziona." });

            bool result = await _groupBookingService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Błąd przy usuwaniu rezerwacji grupowej." });

            return Json(new { success = true, message = "Rezerwacja grupowa usunięta." });
        }
    }
}
