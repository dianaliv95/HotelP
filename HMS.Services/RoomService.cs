using HMS.Data;
using HMS.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
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

        // Dodawanie nowego pokoju
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

        // Pobranie wszystkich pokoi z ich zakwaterowaniami
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

        // Pobranie pokoju po ID z jego zakwaterowaniem
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

        // Pobranie pokoi przypisanych do konkretnego zakwaterowania
        public async Task<List<Room>> GetRoomsByAccommodationAsync(int accommodationId)
        {
            try
            {
                return await _context.Rooms
                    .Where(r => r.AccommodationID == accommodationId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania pokoi dla zakwaterowania o ID {AccommodationId}.", accommodationId);
                return new List<Room>();
            }
        }
        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime fromDate, DateTime toDate)
        {
            return !await _context.Reservations
                .AnyAsync(res => res.RoomID == roomId && res.DateFrom < toDate && res.DateTo > fromDate);
        }
        public async Task<List<Room>> GetFilteredRoomsAsync()
        {
            try
            {
                // Pobieranie pokoi z określonymi statusami
                var filteredRooms = await _context.Rooms
                    .Where(r =>
                        r.Status == "Available" ||
                        r.Status == "Completed" ||
                        r.Status == "CompletedSettledSt" ||
                        r.Status == "NoShow" ||
                        r.Status == "CompletedUnsettledStay")
                    .ToListAsync();

                return filteredRooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania pokoi z dozwolonymi statusami.");
                return new List<Room>();
            }
        }


        // Pobranie dostępnych pokoi w podanym zakresie dat
        public async Task<List<Room>> GetAvailableRoomsAsync(int accommodationId, DateTime fromDate, DateTime toDate)
        {
            var rooms = await _context.Rooms
                .Include(r => r.Accommodation)
                .ThenInclude(a => a.AccommodationPackage) // Jeśli potrzebujesz również pakietu
                .Where(r => r.AccommodationID == accommodationId
                            && r.Status == "Available"
                            && !r.Reservations.Any(res =>
                                (res.DateFrom < toDate && res.DateTo > fromDate)))
                .ToListAsync();

            // Logowanie informacji o pokojach
            foreach (var room in rooms)
            {
                _logger.LogInformation("Pokój ID: {RoomId}, MaxAdults: {MaxAdults}, MaxChildren: {MaxChildren}",
                    room.ID, room.Accommodation?.MaxAdults, room.Accommodation?.MaxChildren);
            }

            return rooms;
        }

        // Aktualizacja statusu pokoju
        public async Task<bool> UpdateRoomStatusAsync(int roomId, string status, DateTime? availableFrom = null, DateTime? availableTo = null)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null)
                {
                    _logger.LogWarning("Pokój o ID {RoomID} nie został znaleziony.", roomId);
                    return false;
                }

                room.Status = status;
                room.AvailableFrom = availableFrom;
                room.AvailableTo = availableTo;

                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Status pokoju o ID {RoomID} został zaktualizowany na {Status}.", roomId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji statusu pokoju o ID {RoomID}.", roomId);
                return false;
            }
        }

        // Oznaczenie pokoju jako zarezerwowanego
        public async Task<bool> MarkRoomAsReservedAsync(int roomId)
        {
            try
            {
                return await UpdateRoomStatusAsync(roomId, "Reserved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas oznaczania pokoju jako zarezerwowanego o ID {RoomID}.", roomId);
                return false;
            }
        }
        public async Task<bool> RoomisAvailavleAsync(int roomId)
        {
            try
            {
                return await UpdateRoomStatusAsync(roomId, "Available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas oznaczania pokoju jako zarezerwowanego o ID {RoomID}.", roomId);
                return false;
            }

        }

        // Usuwanie pokoju
        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            try
            {
                // Pobierz pokój na podstawie ID
                var room = await _context.Rooms
                    .Include(r => r.Reservations) // Załaduj powiązane rezerwacje, jeśli istnieją
                    .FirstOrDefaultAsync(r => r.ID == roomId);

                if (room == null)
                {
                    _logger.LogWarning("Próba usunięcia pokoju o ID {RoomID}, który nie istnieje.", roomId);
                    return false;
                }

                // Sprawdź, czy pokój nie ma powiązanych aktywnych rezerwacji
                if (room.Reservations != null && room.Reservations.Any())
                {
                    _logger.LogWarning("Nie można usunąć pokoju o ID {RoomID}, ponieważ ma powiązane rezerwacje.", roomId);
                    return false;
                }

                // Usuń pokój
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pokój o ID {RoomID} został pomyślnie usunięty.", roomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania pokoju o ID {RoomID}.", roomId);
                return false;
            }
        }
    }
}
