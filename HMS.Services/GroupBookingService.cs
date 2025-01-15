using HMS.Data;
using HMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GroupBookingService
{
    private readonly HMSContext _context;
    private readonly ILogger<GroupBookingService> _logger;

    public GroupBookingService(HMSContext context, ILogger<GroupBookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sprawdza, czy dany pokój (roomId) jest wolny w półotwartym przedziale [checkIn, checkOut),
    /// pomijając rezerwację o ID = excludeResId (np. przy edycji).
    /// Kolizja => (checkIn < existing.ToDate) && (checkOut > existing.FromDate).
    /// </summary>
    private async Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime checkIn,
        DateTime checkOut,
        int? excludeResId = null)
    {
        // Pobieramy rezerwacje, w których występuje ten pokój (poza excludeResId).
        var all = await _context.GroupReservations
            .Include(gr => gr.GroupReservationRooms)
            .Where(gr => gr.ID != excludeResId) // pomijamy rezerwację, którą właśnie edytujemy (jeśli jest).
            .ToListAsync();

        foreach (var gr in all)
        {
            bool hasThatRoom = gr.GroupReservationRooms.Any(rr => rr.RoomID == roomId);
            if (!hasThatRoom)
                continue; // ta rezerwacja nie dotyczy naszego pokoju

            // Konwencja [gr.FromDate, gr.ToDate).
            // Kolizja => (checkIn < gr.ToDate) && (checkOut > gr.FromDate)
            if (checkIn < gr.ToDate && checkOut > gr.FromDate)
            {
                // Koliduje => niedostępne
                return false;
            }
        }

        // Brak kolizji => wolne
        return true;
    }

    /// <summary>
    /// Pobiera wszystkie rezerwacje grupowe (bez auto-zwalniania).
    /// </summary>
    public async Task<List<GroupReservation>> GetAllAsync()
    {
        return await _context.GroupReservations
            .Include(gr => gr.GroupReservationRooms)
                .ThenInclude(rr => rr.Room)
            .ToListAsync();
    }

    /// <summary>
    /// Pobiera jedną rezerwację grupową (bez auto-zwalniania).
    /// </summary>
    public async Task<GroupReservation> GetByIdAsync(int id)
    {
        return await _context.GroupReservations
            .Include(gr => gr.GroupReservationRooms)
                .ThenInclude(rr => rr.Room)
            .FirstOrDefaultAsync(gr => gr.ID == id);
    }

    /// <summary>
    /// Tworzy nową rezerwację grupową, sprawdza dostępność pokoi (kolizje),
    /// ustawia pokoje w status "Reserved" i zapisuje w bazie.
    /// </summary>
    public async Task<bool> CreateAsync(GroupReservation groupRes, List<int> roomIDs)
    {
        using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1) Sprawdzenie kolizji
            foreach (var rid in roomIDs)
            {
                bool free = await IsRoomAvailableAsync(rid, groupRes.FromDate, groupRes.ToDate, null);
                if (!free)
                {
                    _logger.LogWarning(
                        "Pokój {RoomID} jest już zajęty w [ {From}, {To} ).",
                        rid, groupRes.FromDate, groupRes.ToDate
                    );
                    return false;
                }
            }

            // 2) Dodajemy rezerwację
            _context.GroupReservations.Add(groupRes);
            await _context.SaveChangesAsync();

            // 3) Tworzymy powiązania + ustawiamy status pokoju "Reserved"
            foreach (var rid in roomIDs)
            {
                var grr = new GroupReservationRoom
                {
                    GroupReservationID = groupRes.ID,
                    RoomID = rid
                };
                _context.GroupReservationRooms.Add(grr);

                var room = await _context.Rooms.FindAsync(rid);
                if (room != null)
                {
                    room.Status = "Reserved";
                    _context.Rooms.Update(room);
                }
            }

            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd przy tworzeniu rezerwacji grupowej.");
            await trans.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// Edytuje rezerwację: zwalnia usunięte pokoje, rezerwuje nowe.
    /// Jeśli status = Completed..., to zwalnia wszystkie pokoje w tej rezerwacji.
    /// </summary>
    public async Task<bool> UpdateAsync(GroupReservation updated, List<int> newRoomIDs)
    {
        using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1) Pobierz istniejącą rezerwację grupową wraz z pokojami
            var existing = await _context.GroupReservations
                .Include(gr => gr.GroupReservationRooms)
                    .ThenInclude(rr => rr.Room)
                .FirstOrDefaultAsync(gr => gr.ID == updated.ID);

            if (existing == null)
            {
                _logger.LogWarning("Nie znaleziono rezerwacji grupowej o ID={0}.", updated.ID);
                return false;
            }

            // 2) (opcjonalnie) Sprawdź kolizje dla nowych pokoi
            //    - np. IsRoomAvailableAsync(...)

            // 3) Zwalniamy pokoje, które zostały usunięte z listy
            var oldRooms = existing.GroupReservationRooms.ToList();
            foreach (var oldGrr in oldRooms)
            {
                if (!newRoomIDs.Contains(oldGrr.RoomID))
                {
                    var oldRoom = await _context.Rooms.FindAsync(oldGrr.RoomID);
                    if (oldRoom != null && oldRoom.Status == "Reserved")
                    {
                        oldRoom.Status = "Available";
                        _context.Rooms.Update(oldRoom);
                    }
                }
            }

            // 4) Usuwamy stare powiązania
            _context.GroupReservationRooms.RemoveRange(oldRooms);
            await _context.SaveChangesAsync();

            // 5) Dodajemy nowe powiązania i ustawiamy status = "Reserved"
            foreach (var roomId in newRoomIDs)
            {
                var grr = new GroupReservationRoom
                {
                    GroupReservationID = existing.ID,
                    RoomID = roomId
                };
                _context.GroupReservationRooms.Add(grr);

                var newRoom = await _context.Rooms.FindAsync(roomId);
                if (newRoom != null)
                {
                    newRoom.Status = "Reserved";
                    _context.Rooms.Update(newRoom);
                }
            }

            // 6) Aktualizujemy pola rezerwacji (z formularza)
            existing.ReservationNumber = updated.ReservationNumber;
            existing.FirstName = updated.FirstName;
            existing.LastName = updated.LastName;
            existing.FromDate = updated.FromDate;
            existing.ToDate = updated.ToDate;
            existing.AdultCount = updated.AdultCount;
            existing.ChildrenCount = updated.ChildrenCount;
            existing.DinnerChildren = updated.DinnerChildren;
            existing.DinnerAdults = updated.DinnerAdults;
            existing.LunchChildren = updated.LunchChildren;

            existing.LunchAdults = updated.LunchAdults;
            existing.BreakfastChildren = updated.BreakfastChildren;
            existing.BreakfastAdults = updated.BreakfastAdults;


            // Pola płatności, statusu, kontaktu
            existing.IsPaid = updated.IsPaid;
            existing.PaymentMethod = updated.PaymentMethod;
            existing.RStatus = updated.RStatus;
            existing.ContactPhone = updated.ContactPhone;
            existing.ContactEmail = updated.ContactEmail;
            existing.UpdatedAt = DateTime.Now;

            // 7) Jeśli rezerwacja jest w jednym z „zakończonych” statusów – zwalniamy pokoje
            var finishedStatuses = new[]
            {
            GroupReservationStatus.CompletedSettledStay,
            GroupReservationStatus.CompletedUnsettledStay,
            GroupReservationStatus.NoShow
        };
            // Możesz też dodać: GroupReservationStatus.SettledStay / UnsettledStay,
            // jeżeli chcesz zwalniać przy tych statusach.

            if (finishedStatuses.Contains(existing.RStatus))
            {
                var currentRooms = await _context.GroupReservationRooms
                    .Where(rr => rr.GroupReservationID == existing.ID)
                    .Include(rr => rr.Room)
                    .ToListAsync();

                foreach (var rr in currentRooms)
                {
                    if (rr.Room != null && rr.Room.Status == "Reserved")
                    {
                        rr.Room.Status = "Available";
                        _context.Rooms.Update(rr.Room);
                    }
                }
            }

            // 8) Zapis do bazy
            _context.GroupReservations.Update(existing);
            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd przy aktualizacji rezerwacji grupowej ID={0}.", updated.ID);
            await trans.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// Usuwanie rezerwacji grupowej – zwalniamy pokoje, usuwamy wpis.
    /// </summary>
    public async Task<bool> DeleteAsync(int groupReservationId)
    {
        using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var gr = await _context.GroupReservations
                .Include(g => g.GroupReservationRooms)
                .FirstOrDefaultAsync(x => x.ID == groupReservationId);

            if (gr == null)
                return false;

            // Zwolnij pokoje
            foreach (var grr in gr.GroupReservationRooms)
            {
                var room = await _context.Rooms.FindAsync(grr.RoomID);
                if (room != null && room.Status == "Reserved")
                {
                    room.Status = "Available";
                    _context.Rooms.Update(room);
                }
            }

            _context.GroupReservationRooms.RemoveRange(gr.GroupReservationRooms);
            _context.GroupReservations.Remove(gr);

            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd przy usuwaniu rezerwacji grupowej ID={0}.", groupReservationId);
            await trans.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// Oblicza łączną cenę: (pokój * noce) – pomijamy wyżywienie, 
    /// bo według Twojej wersji "jest usunięte".
    /// </summary>
    public decimal CalculateTotalPrice(GroupReservation groupRes)
    {
        if (groupRes == null)
            return 0m;

        int nights = (groupRes.ToDate - groupRes.FromDate).Days;
        if (nights < 0)
            nights = 0;

        decimal total = 0m;

        // 1) Cena pokoi
        if (groupRes.GroupReservationRooms != null)
        {
            foreach (var grr in groupRes.GroupReservationRooms)
            {
                var room = grr.Room
                    ?? _context.Rooms.Find(grr.RoomID);
                if (room == null)
                    continue;

                total += room.PricePerNight * nights;
            }
        }

        // 2) Posiłki – stawki takie jak w JS
        const decimal PRICE_BREAKFAST = 20m;
        const decimal PRICE_LUNCH = 25m;
        const decimal PRICE_DINNER = 30m;

        // (ilość dorosłych i dzieci, pomnożone przez liczbę nocy)
        total += (groupRes.BreakfastAdults * PRICE_BREAKFAST * nights);
        total += (groupRes.BreakfastChildren * PRICE_BREAKFAST * nights);

        total += (groupRes.LunchAdults * PRICE_LUNCH * nights);
        total += (groupRes.LunchChildren * PRICE_LUNCH * nights);

        total += (groupRes.DinnerAdults * PRICE_DINNER * nights);
        total += (groupRes.DinnerChildren * PRICE_DINNER * nights);

        return total;
    }


    /// <summary>
    /// Przykładowy backtracking – odfiltrowanie pokoi, które mają 
    /// Accommodation.MaxGuests >= totalGuests. (placeholder)
    /// </summary>
    public List<Room> FindMinRoomsForGuestsBacktracking(List<Room> candidateRooms, int totalGuests)
    {
        return candidateRooms
            .Where(r => (r.Accommodation?.MaxGuests ?? 0) >= totalGuests)
            .ToList();
    }
}
