// RoomService.cs
using HMS.Data;
using HMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Services
{
    public class RoomService
    {
        private readonly HMSContext _context;
        private readonly ILogger<RoomService> _logger;

        public RoomService(HMSContext context, ILogger<RoomService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- (1) Dodawanie nowego pokoju ---
        public async Task<bool> AddRoomAsync(Room room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            try
            {
                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Pokój został pomyślnie dodany: {RoomName}", room.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania pokoju: {RoomName}", room?.Name);
                return false;
            }
        }

        // --- (2) Ogólne pobranie wszystkich pokoi ---
        public async Task<List<Room>> GetAllRoomsAsync()
        {
            try
            {
                return await _context.Rooms
                    .Include(r => r.Accommodation)
                    .ThenInclude(a => a.AccommodationPackage)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich pokoi.");
                return new List<Room>();
            }
        }

        // --- (3) Pobranie 1 pokoju po ID ---
        public async Task<Room> GetRoomByIdAsync(int id)
        {
            try
            {
                return await _context.Rooms
                    .Include(r => r.Accommodation)
                    .FirstOrDefaultAsync(r => r.ID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania pokoju o ID {RoomID}.", id);
                return null;
            }
        }

        // --- (4) IsRoomAvailableAsync – sprawdza (a) blokadę, (b) rezerwacje ---
        public async Task<bool> IsRoomAvailableAsync(
            int roomId,
            DateTime fromDate,
            DateTime toDate,
            int? excludeSingleResId = null,
            int? excludeGroupResId = null)
        {
            // 1) Czy taki pokój istnieje?
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                _logger.LogWarning("RoomID={0} nie istnieje w bazie.", roomId);
                return false;
            }

            // 2) Sprawdź blokadę zakresową (BlockedFrom–BlockedTo).
            //    Kolizja => (room.BlockedFrom < toDate) && (room.BlockedTo > fromDate)
            if (room.BlockedFrom != null && room.BlockedTo != null)
            {
                bool overlap = room.BlockedFrom.Value < toDate
                               && room.BlockedTo.Value > fromDate;

                if (overlap)
                {
                    _logger.LogInformation(
                        "Pokój ID={0} jest zablokowany w [{1:d}, {2:d}) => niedostępny.",
                        roomId, room.BlockedFrom, room.BlockedTo
                    );
                    return false;
                }
            }

            // 3) Sprawdź rezerwacje pojedyncze
            bool collisionSingle = await _context.Reservations
                .Where(r => r.ID != (excludeSingleResId ?? 0))
                .Where(r => r.RoomID == roomId)
                .AnyAsync(r =>
                    r.DateFrom < toDate &&
                    r.DateTo > fromDate
                );
            if (collisionSingle)
            {
                // Kolizja z rezerwacją pojedynczą
                return false;
            }

            // 4) Sprawdź rezerwacje grupowe
            bool collisionGroup = await _context.GroupReservations
                .Where(gr => gr.ID != (excludeGroupResId ?? 0))
                .Where(gr => gr.FromDate < toDate && gr.ToDate > fromDate)
                .AnyAsync(gr => gr.GroupReservationRooms.Any(rr => rr.RoomID == roomId));
            if (collisionGroup)
            {
                return false;
            }

            // 5) Brak kolizji + brak blokady => OK
            return true;
        }

        // --- (5) Zmienianie statusu, AvailableFrom/To (opcjonalnie) ---
        public async Task<bool> UpdateRoomStatusAsync(int roomId, string status, DateTime? availableFrom = null, DateTime? availableTo = null)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null)
                {
                    _logger.LogWarning("Pokój o ID {0} nie został znaleziony.", roomId);
                    return false;
                }

                room.Status = status;
                room.AvailableFrom = availableFrom;
                room.AvailableTo = availableTo;

                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Status pokoju o ID {0} zaktualizowany na {1}.", roomId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy aktualizacji statusu pokoju ID={0}.", roomId);
                return false;
            }
        }

        // (Pomocnicza) Całościowe update dowolnych pól w Room
        public async Task<bool> UpdateRoomAsync(Room updatedRoom)
        {
            try
            {
                _context.Rooms.Update(updatedRoom);
                return (await _context.SaveChangesAsync() > 0);
              

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy aktualizacji pokoju (UpdateRoom).");
                return false;
            }
        }

        // Dalsze przykładowe metody:
        public async Task<bool> MarkRoomAsReservedAsync(int roomId)
        {
            return await UpdateRoomStatusAsync(roomId, "Reserved");
        }
        public async Task<bool> RoomisAvailavleAsync(int roomId)
        {
            return await UpdateRoomStatusAsync(roomId, "Available");
        }

        // (6) Usuwanie pokoju
        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            try
            {
                var room = await _context.Rooms
                    .Include(r => r.Reservations)
                    .FirstOrDefaultAsync(r => r.ID == roomId);

                if (room == null)
                {
                    _logger.LogWarning("Próba usunięcia pokoju ID={0}, który nie istnieje.", roomId);
                    return false;
                }

                // Sprawdź, czy pokój nie ma powiązanych rezerwacji aktywnych
                if (room.Reservations != null && room.Reservations.Any())
                {
                    _logger.LogWarning("Nie można usunąć pokoju ID={0}, ma powiązane rezerwacje.", roomId);
                    return false;
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pokój ID={0} został usunięty.", roomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy usuwaniu pokoju ID={0}.", roomId);
                return false;
            }
        }
        public async Task<List<Room>> GetFilteredRoomsAsync(string status = null)
        {
            try
            {
                IQueryable<Room> query = _context.Rooms;

                if (!string.IsNullOrEmpty(status))
                {
                    // Jeżeli ktoś podał status, to filtruj tylko po nim
                    query = query.Where(r => r.Status == status);
                }
                else
                {
                    // Jeżeli nie podano statusu, to używamy wbudowanej logiki
                    // dozwolonych statusów
                    query = query.Where(r =>
                        r.Status == "Available" ||
                        r.Status == "Completed" ||
                        r.Status == "CompletedSettledSt" ||
                        r.Status == "NoShow" ||
                        r.Status == "CompletedUnsettledStay"
                    );
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania przefiltrowanych pokoi.");
                return new List<Room>();
            }
        }


        // (7) Przykład getAvailableRoomsAsync – tu nie analizuje BlockedFrom/To
        // bo i tak w IsRoomAvailableAsync uwzględnisz to wewnętrznie.
        // (Możesz dodać analogicznie w pętli i sprawdzić.)
        public async Task<List<Room>> GetAvailableRoomsAsync(int accommodationId, DateTime fromDate, DateTime toDate)
        {
            var rooms = await _context.Rooms
                .Include(r => r.Accommodation)
                .ThenInclude(a => a.AccommodationPackage)
                .Where(r => r.AccommodationID == accommodationId)
                .ToListAsync();

            var result = new List<Room>();
            foreach (var r in rooms)
            {
                bool free = await IsRoomAvailableAsync(r.ID, fromDate, toDate);
                if (free) result.Add(r);
            }
            return result;
        }
    }
}
