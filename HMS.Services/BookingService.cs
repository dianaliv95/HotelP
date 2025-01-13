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
    public class BookingService
    {
        private readonly HMSContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(HMSContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Pobranie listy rezerwacji
        public async Task<List<Reservation>> GetReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .ThenInclude(room => room.Accommodation)
                .ToListAsync();
        }

        // Sprawdzenie dostępności pokoju w zadanym zakresie
        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime fromDate, DateTime toDate)
        {
            return !await _context.Reservations
                .AnyAsync(res =>
                    res.RoomID == roomId &&
                    res.DateFrom < toDate &&
                    res.DateTo > fromDate);
        }

        // Pobranie rezerwacji po ID
        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .ThenInclude(room => room.Accommodation)
                .FirstOrDefaultAsync(r => r.ID == id);
        }


        // Oblicz cenę – gdy mamy w pełni wczytaną Reservation
        public decimal CalculateTotalPrice(Reservation reservation)
        {
            if (reservation.Room == null)
            {
                reservation.Room = _context.Rooms
                    .FirstOrDefault(r => r.ID == reservation.RoomID);
                if (reservation.Room == null)
                    return 0m;
            }

            int nights = (reservation.DateTo - reservation.DateFrom).Days;
            if (nights <= 0) return 0m;

            decimal total = reservation.Room.PricePerNight * nights;

            total += 20 * reservation.BreakfastAdults * nights;
            total += 20 * reservation.BreakfastChildren * nights;
            total += 25 * reservation.LunchAdults * nights;
            total += 25 * reservation.LunchChildren * nights;
            total += 30 * reservation.DinnerAdults * nights;
            total += 30 * reservation.DinnerChildren * nights;

            return total;
        }

        // Oblicz cenę – gdy mamy Room, liczbę nocy i tymczasowy obiekt Reservation 
        // (np. w AJAX)
        public decimal CalculateTotalPrice(Room room, int numberOfNights, Reservation reservation)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (numberOfNights <= 0) return 0m;

            decimal total = room.PricePerNight * numberOfNights;

            total += 20 * reservation.BreakfastAdults * numberOfNights;
            total += 20 * reservation.BreakfastChildren * numberOfNights;
            total += 25 * reservation.LunchAdults * numberOfNights;
            total += 25 * reservation.LunchChildren * numberOfNights;
            total += 30 * reservation.DinnerAdults * numberOfNights;
            total += 30 * reservation.DinnerChildren * numberOfNights;

            return total;
        }

        // Tworzenie nowej rezerwacji
        public async Task<bool> CreateReservationAsync(Reservation reservation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Reservations.Add(reservation);

                var room = await _context.Rooms.FindAsync(reservation.RoomID);
                if (room != null)
                {
                    room.Status = "Reserved";
                    _context.Rooms.Update(room);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia rezerwacji.");
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Aktualizacja rezerwacji
        public async Task<bool> UpdateReservationAsync(Reservation reservation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(r => r.ID == reservation.ID);

                if (existing == null)
                {
                    _logger.LogWarning("Rezerwacja o ID {0} nie istnieje.", reservation.ID);
                    return false;
                }

                // Zmiana pokoju
                if (existing.RoomID != reservation.RoomID)
                {
                    var oldRoom = await _context.Rooms.FindAsync(existing.RoomID);
                    if (oldRoom != null)
                    {
                        oldRoom.Status = "Available";
                        _context.Rooms.Update(oldRoom);
                    }

                    var newRoom = await _context.Rooms.FindAsync(reservation.RoomID);
                    if (newRoom != null)
                    {
                        newRoom.Status = "Reserved";
                        _context.Rooms.Update(newRoom);
                    }
                }

                // Przepisanie pól
                existing.AccommodationID = reservation.AccommodationID;
                existing.RoomID = reservation.RoomID;
                existing.FirstName = reservation.FirstName;
                existing.LastName = reservation.LastName;
                existing.DateFrom = reservation.DateFrom;
                existing.DateTo = reservation.DateTo;
                existing.AdultCount = reservation.AdultCount;
                existing.ChildrenCount = reservation.ChildrenCount;
                existing.BreakfastAdults = reservation.BreakfastAdults;
                existing.BreakfastChildren = reservation.BreakfastChildren;
                existing.LunchAdults = reservation.LunchAdults;
                existing.LunchChildren = reservation.LunchChildren;
                existing.DinnerAdults = reservation.DinnerAdults;
                existing.DinnerChildren = reservation.DinnerChildren;
                existing.IsPaid = reservation.IsPaid;
                existing.PaymentMethod = reservation.PaymentMethod;
                existing.Status = reservation.Status;
                existing.UpdatedAt = DateTime.Now;



                // Gdy rezerwacja zakończona
                var finishedStatuses = new[]
                {
                    ReservationStatus.CompletedSettledStay,
                    ReservationStatus.CompletedUnsettledStay,
                    ReservationStatus.NoShow
                };
                if (finishedStatuses.Contains(reservation.Status))
                {
                    var room = await _context.Rooms.FindAsync(reservation.RoomID);
                    if (room != null)
                    {
                        room.Status = "Available";
                        _context.Rooms.Update(room);
                    }
                }

                _context.Reservations.Update(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji rezerwacji {0}", reservation.ID);
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Usuwanie rezerwacji
        public async Task<bool> DeleteReservationAsync(int reservationId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservation = await _context.Reservations.FindAsync(reservationId);
                if (reservation == null) return false;

                var room = await _context.Rooms.FindAsync(reservation.RoomID);
                if (room != null)
                {
                    room.Status = "Available";
                    _context.Rooms.Update(room);
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy usuwaniu rezerwacji.");
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Pobieranie dostępnych pokoi
        public async Task<List<Room>> GetAvailableRoomsAsync(int accommodationId, DateTime fromDate, DateTime toDate)
        {
            return await _context.Rooms
                .Where(r =>
                    r.AccommodationID == accommodationId
                    && r.Status == "Available"
                    && !_context.Reservations.Any(res =>
                        res.RoomID == r.ID
                        && res.DateFrom < toDate
                        && res.DateTo > fromDate))
                .ToListAsync();
        }
    }
}
