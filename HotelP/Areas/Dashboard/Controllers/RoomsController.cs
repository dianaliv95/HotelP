using HMS.Data;
using HMS.Entities;
using HMS.Services;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class RoomsController : Controller
    {
        private readonly RoomService _roomService;
        private readonly HMSContext _context;
        private readonly AccommodationService _accommodationService;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(HMSContext context,
RoomService roomService, AccommodationService accommodationService, ILogger<RoomsController> logger)
        {
            _context = context; // Wstrzykujemy kontekst bazy danych

            _roomService = roomService;
            _accommodationService = accommodationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var accommodations = await _accommodationService.GetAllAccommodationsAsync();
            var model = new RoomViewModel
            {
                Accommodations = accommodations
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Accommodations = await _accommodationService.GetAllAccommodationsAsync();
                return View(model);
            }

            var room = new Room
            {
                Name = model.Name,
                AccommodationID = model.AccommodationID,
                PricePerNight = model.PricePerNight,
                IsReserved = false
            };

            bool success = await _roomService.AddRoomAsync(room);
            if (success)
            {
                TempData["SuccessMessage"] = "Pokój został dodany.";
                return RedirectToAction("Index", "Accommodations");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas dodawania pokoju.");
            model.Accommodations = await _accommodationService.GetAllAccommodationsAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRoomStatus(int roomId, string status, DateTime? availableFrom, DateTime? availableTo)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null)
                {
                    return NotFound("Pokój nie został znaleziony.");
                }

                room.Status = status;
                room.AvailableFrom = availableFrom;
                room.AvailableTo = availableTo;

                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Status pokoju został zaktualizowany." });
            }
            catch (Exception ex)
            {
                // Logowanie błędów
                return Json(new { success = false, message = "Wystąpił błąd podczas aktualizacji statusu pokoju." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> UpdateAvailableFrom([FromBody] UpdateRoomDateModel model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null) return NotFound();

            room.AvailableFrom = DateTime.Parse(model.AvailableFrom);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvailableTo([FromBody] UpdateRoomDateModel model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null) return NotFound();

            room.AvailableTo = DateTime.Parse(model.AvailableTo);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success = await _roomService.DeleteRoomAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Pokój został usunięty.";
                return RedirectToAction("Index", "Accommodations");
            }
            TempData["ErrorMessage"] = "Wystąpił błąd podczas usuwania pokoju.";
            return RedirectToAction("Index", "Accommodations");
        }
    }
}
