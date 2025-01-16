using HMS.Services;
using HMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Hotel.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel.Areas.Dashboard.ViewModels;

namespace Hotel.Controllers
{
    public class PublicBookingController : Controller
    {
        private readonly RoomService _roomService;
		private readonly BookingService _bookingService;
		private readonly GroupBookingService _groupBookingService;

        public PublicBookingController(BookingService bookingService, RoomService roomService, GroupBookingService groupBookingService)
        {
            _roomService = roomService;
            _groupBookingService = groupBookingService;
			_bookingService = bookingService;
		}

		/// <summary>
		/// Wyszukiwanie pokoi dla [checkIn, checkOut], X dorosłych, Y dzieci.
		/// Sprawdzamy status "Available" + ewentualnie brak kolizji.
		/// </summary>
		[HttpGet("SearchRooms")]
		public async Task<IActionResult> SearchRooms(
			DateTime? checkIn,
			DateTime? checkOut,
			int? adults,
			int? children)
		{
			if (!checkIn.HasValue || !checkOut.HasValue)
				return BadRequest("Brak wymaganych dat.");
			if (checkOut <= checkIn)
				return BadRequest("Data wyjazdu musi być po dacie przyjazdu.");

			int totalGuests = (adults ?? 1) + (children ?? 0);

			var allRooms = await _roomService.GetAllRoomsAsync();
			var candidateRooms = allRooms
				.Where(r => !r.IsBlocked)  // nieblokowane
				.Where(r => r.Accommodation.MaxGuests >= totalGuests)
				.ToList();

			var freeRooms = new List<Room>();
			foreach (var room in candidateRooms)
			{
				bool free = await _roomService.IsRoomAvailableAsync(
					room.ID,
					checkIn.Value,
					checkOut.Value
				);
				if (free) freeRooms.Add(room);
			}

			var model = new GroupSearchResultViewModel
			{
				FromDate = checkIn.Value,
				ToDate = checkOut.Value,
				AdultCount = adults ?? 1,
				ChildrenCount = children ?? 0,
				FoundRooms = freeRooms
			};

			// Zwrot widoku GroupSearchResults.cshtml
			return View("GroupSearchResults", model);
		}

        [HttpGet("CreateReservation")]
        public IActionResult CreateReservation(
              DateTime fromDate,
              DateTime toDate,
              int adults,
              int children,
              int roomId)
        {
            var model = new BookingActionModel
            {
                FromDate = fromDate,
                DateTo = toDate,
                AdultCount = adults,
                ChildrenCount = children,
                RoomID = roomId
            };
            return View("CreateReservation", model);
        }

        // 2) POST: /PublicBooking/CreateReservation
        [HttpPost("CreateReservation")]
        public async Task<IActionResult> CreateReservation(BookingActionModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateReservation", model);
            }

            // Tworzymy obiekt Reservation
            var newReservation = new Reservation
            {
                RoomID = (int)model.RoomID,
                DateFrom = model.FromDate,
                DateTo = model.DateTo,
                AdultCount = model.AdultCount,
                ChildrenCount = model.ChildrenCount,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            bool success = await _bookingService.CreateReservationAsync(newReservation);
            if (!success)
            {
                ModelState.AddModelError("", "Nie udało się utworzyć rezerwacji (pokój może być już zajęty).");
                return View("CreateReservation", model);
            }

            // Przekierowanie do potwierdzenia
            return RedirectToAction("ConfirmSingle", new { id = newReservation.ID });
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
		[HttpGet("ConfirmSingle")]
		public async Task<IActionResult> ConfirmSingle(int id)
		{
			var reservation = await _bookingService.GetReservationByIdAsync(id);
			if (reservation == null) return NotFound();

			return View("ConfirmSingle", reservation);
		}

		[HttpGet("ConfirmGroup")]
		public async Task<IActionResult> ConfirmGroup(int id)
		{
			var gr = await _groupBookingService.GetByIdAsync(id);
			if (gr == null) return NotFound();

			return View("ConfirmGroup", gr);
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
				FirstName = model.FirstName,
				LastName = model.LastName,
				ContactPhone = model.ContactPhone,
				ContactEmail = model.ContactEmail,
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
