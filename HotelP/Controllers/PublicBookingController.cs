using HMS.Services;
using HMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Hotel.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    public class PublicBookingController : Controller
    {
        private readonly RoomService _roomService;
        private readonly GroupBookingService _groupBookingService;

        public PublicBookingController(RoomService roomService, GroupBookingService groupBookingService)
        {
            _roomService = roomService;
            _groupBookingService = groupBookingService;
        }

        /// <summary>
        /// Wyszukiwanie pokoi dla [checkIn, checkOut], X dorosłych, Y dzieci.
        /// Sprawdzamy status "Available" + ewentualnie brak kolizji.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchRooms(DateTime? checkIn, DateTime? checkOut, int? adults, int? children)
        {
            if (!checkIn.HasValue || !checkOut.HasValue || checkIn < DateTime.Today || checkOut <= checkIn)
            {
                return BadRequest("Daty nieprawidłowe.");
            }

            int totalGuests = (adults ?? 1) + (children ?? 0);

            var allRooms = await _roomService.GetAllRoomsAsync();
            // Filtr wg. statusu "Available"
            // Tylko te pokoje, co status == "Available" i MaxGuests >= totalGuests
            var candidateRooms = allRooms
                .Where(r => r.Status == "Available")
                .Where(r => (r.Accommodation?.MaxGuests ?? 0) >= totalGuests)
                .ToList();


            // Wywołujemy backtracking, by znaleźć minimalny zestaw
            var minRoomsSet = _groupBookingService.FindMinRoomsForGuestsBacktracking(candidateRooms, totalGuests);

            var model = new GroupSearchResultViewModel
            {
                FromDate = checkIn.Value,
                ToDate = checkOut.Value,
                AdultCount = adults ?? 1,
                ChildrenCount = children ?? 0,
                FoundRooms = minRoomsSet
            };

            return View("GroupSearchResults", model);
            // stwórz widok GroupSearchResults
        }

        /// <summary>
        /// GET: formularz tworzenia rezerwacji grupowej (na podstawie roomIds).
        /// </summary>
        [HttpGet]
        public IActionResult CreateGroupReservation(DateTime fromDate, DateTime toDate, int adults, int children, List<int> roomIds)
        {
            var model = new CreateGroupReservationViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                AdultCount = adults,
                ChildrenCount = children,
                SelectedRoomIDs = roomIds
            };

            // Wyświetlasz formularz, w którym user wypełnia dane kontaktowe, 
            // liczbę posiłków itp.
            return View("CreateGroupReservation", model);
        }

        /// <summary>
        /// POST: Zapis rezerwacji grupowej z publicznego formularza.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateGroupReservation(CreateGroupReservationViewModel model)
        {
            // Prosta walidacja
            if (model.FromDate < DateTime.Today || model.ToDate <= model.FromDate)
            {
                ModelState.AddModelError("", "Daty są nieprawidłowe.");
            }
            if (!ModelState.IsValid)
            {
                return View("CreateGroupReservation", model);
            }

            // Numer rezerwacji
            var reservationNumber = Guid.NewGuid().ToString().Substring(0, 8);

            // Tworzymy GroupReservation:
            var groupRes = new GroupReservation
            {
                ReservationNumber = reservationNumber,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                AdultCount = model.AdultCount,
                ChildrenCount = model.ChildrenCount,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                // Pola logiczne np. "IsBreakfastIncluded" -> w tym przykładzie 
                // wolałabym tak: BreakfastAdults, BreakfastChildren...
                // Ale masz "IsBreakfastIncluded" – OK:
                

                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Tworzymy rezerwację
            bool saved = await _groupBookingService.CreateAsync(groupRes, model.SelectedRoomIDs);
            if (!saved)
            {
                ModelState.AddModelError("", "Błąd przy tworzeniu rezerwacji grupowej w bazie danych.");
                return View("CreateGroupReservation", model);
            }
            var savedRes = await _groupBookingService.GetByIdAsync(groupRes.ID);
            decimal totalPrice = _groupBookingService.CalculateTotalPrice(savedRes);
            // Potwierdzenie
            var confirmVM = new GroupReservationConfirmationViewModel
            {
                ReservationNumber = groupRes.ReservationNumber,
                FromDate = groupRes.FromDate,
                ToDate = groupRes.ToDate,
                AdultCount = groupRes.AdultCount,
                ChildrenCount = groupRes.ChildrenCount,
                TotalPrice = totalPrice


            };

            return View("GroupReservationConfirmation", confirmVM);
        }
    }
}
