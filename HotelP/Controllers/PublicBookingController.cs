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

       
        [HttpGet("SearchRooms")]
        public async Task<IActionResult> SearchRooms(
    DateTime? checkIn,
    DateTime? checkOut,
    int? adults,
    int? children)
        {
            if (!checkIn.HasValue || !checkOut.HasValue)
                return BadRequest("Brak wymaganych dat przyjazdu/wyjazdu.");

            if (checkOut <= checkIn)
                return BadRequest("Data wyjazdu musi być późniejsza niż data przyjazdu.");

            int totalGuests = (adults ?? 1) + (children ?? 0);

            var allRooms = await _roomService.GetAllRoomsAsync();

            var candidateRooms = allRooms
                .Where(r => !r.IsBlocked)
                .ToList();

            var freeRooms = new List<Room>();
            foreach (var room in candidateRooms)
            {
                bool isFree = await _roomService.IsRoomAvailableAsync(
                    room.ID,
                    checkIn.Value,
                    checkOut.Value
                );
                if (isFree)
                    freeRooms.Add(room);
            }

           
            var model = new GroupSearchResultViewModel
            {
                FromDate = checkIn.Value,
                ToDate = checkOut.Value,
                AdultCount = adults ?? 1,
                ChildrenCount = children ?? 0,
                FoundRooms = freeRooms
            };

            return View("GroupSearchResults", model);
        }


       
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

		
		[HttpPost]
        public async Task<IActionResult> CreateGroupReservation(CreateGroupReservationViewModel model)
        {
            if (model.FromDate < DateTime.Today || model.ToDate <= model.FromDate)
            {
                ModelState.AddModelError("", "Daty są nieprawidłowe.");
            }
            if (!ModelState.IsValid)
            {
                return View("CreateGroupReservation", model);
            }

            var reservationNumber = Guid.NewGuid().ToString().Substring(0, 8);

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

			bool saved = await _groupBookingService.CreateAsync(groupRes, model.SelectedRoomIDs);
            if (!saved)
            {
                ModelState.AddModelError("", "Błąd przy tworzeniu rezerwacji grupowej w bazie danych.");
                return View("CreateGroupReservation", model);
            }
            var savedRes = await _groupBookingService.GetByIdAsync(groupRes.ID);
            decimal totalPrice = _groupBookingService.CalculateTotalPrice(savedRes);
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
