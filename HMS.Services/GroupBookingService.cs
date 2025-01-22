using HMS.Data;
using HMS.Entities;
using HMS.Services;
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

	// Dodajmy zależność do RoomService:
	private readonly RoomService _roomService;

	public GroupBookingService(
		HMSContext context,
		ILogger<GroupBookingService> logger,
		RoomService roomService)
	{
		_context = context;
		_logger = logger;
		_roomService = roomService;
	}

	
	public async Task<List<GroupReservation>> GetAllAsync()
	{
		return await _context.GroupReservations
			.Include(gr => gr.GroupReservationRooms)
				.ThenInclude(rr => rr.Room)
			.ToListAsync();
	}

	public async Task<GroupReservation> GetByIdAsync(int id)
	{
		return await _context.GroupReservations
			.Include(gr => gr.GroupReservationRooms)
				.ThenInclude(rr => rr.Room)
			.FirstOrDefaultAsync(gr => gr.ID == id);
	}

	
	public async Task<bool> CreateAsync(GroupReservation groupRes, List<int> roomIDs)
	{
		using var trans = await _context.Database.BeginTransactionAsync();
		try
		{
			foreach (var rid in roomIDs)
			{
				bool free = await _roomService.IsRoomAvailableAsync(
					rid,
					groupRes.FromDate,
					groupRes.ToDate,
					null,           
					null          
				);
				if (!free)
				{
					_logger.LogWarning("Pokój {0} niedostępny w [{1}, {2})",
						rid, groupRes.FromDate, groupRes.ToDate);
					await trans.RollbackAsync();
					return false;
				}
			}

			_context.GroupReservations.Add(groupRes);
			await _context.SaveChangesAsync();

			foreach (var rid in roomIDs)
			{
				var grr = new GroupReservationRoom
				{
					GroupReservationID = groupRes.ID,
					RoomID = rid
				};
				_context.GroupReservationRooms.Add(grr);
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
				_logger.LogWarning("Nie znaleziono rezerwacji ID={0}", updated.ID);
				return false;
			}

			// 2) Sprawdź kolizje dla nowych pokoi
			foreach (var rid in newRoomIDs)
			{
				bool free = await _roomService.IsRoomAvailableAsync(
					rid,
					updated.FromDate,
					updated.ToDate,
					null,           // excludeSingleResId
					updated.ID      // excludeGroupResId (pomijamy samą rezerwację)
				);
				if (!free)
				{
					_logger.LogWarning("Pokój {0} koliduje w {1}-{2}.",
						rid, updated.FromDate, updated.ToDate);
					await trans.RollbackAsync();
					return false;
				}
			}

			// 3) Usuwamy stare GroupReservationRooms
			var oldRooms = existing.GroupReservationRooms.ToList();
			_context.GroupReservationRooms.RemoveRange(oldRooms);
			await _context.SaveChangesAsync();

			// 4) Dodajemy nowe
			foreach (var rid in newRoomIDs)
			{
				var grr = new GroupReservationRoom
				{
					GroupReservationID = existing.ID,
					RoomID = rid
				};
				_context.GroupReservationRooms.Add(grr);
			}
			await _context.SaveChangesAsync();

			// 5) Aktualizujemy dane rezerwacji
			existing.ReservationNumber = updated.ReservationNumber;
			existing.FirstName = updated.FirstName;
			existing.LastName = updated.LastName;
			existing.FromDate = updated.FromDate;
			existing.ToDate = updated.ToDate;
			existing.AdultCount = updated.AdultCount;
			existing.ChildrenCount = updated.ChildrenCount;
			existing.BreakfastAdults = updated.BreakfastAdults;
			existing.BreakfastChildren = updated.BreakfastChildren;
			existing.LunchAdults = updated.LunchAdults;
			existing.LunchChildren = updated.LunchChildren;
			existing.DinnerAdults = updated.DinnerAdults;
			existing.DinnerChildren = updated.DinnerChildren;
			existing.IsPaid = updated.IsPaid;
			existing.PaymentMethod = updated.PaymentMethod;
			existing.RStatus = updated.RStatus;
			existing.ContactPhone = updated.ContactPhone;
			existing.ContactEmail = updated.ContactEmail;
			existing.UpdatedAt = DateTime.Now;

			_context.GroupReservations.Update(existing);
			await _context.SaveChangesAsync();
			await trans.CommitAsync();

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Błąd przy aktualizacji rezerwacji grupowej ID={0}", updated.ID);
			await trans.RollbackAsync();
			return false;
		}
	}

	
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

	
	public decimal CalculateTotalPrice(GroupReservation groupRes)
	{
		if (groupRes == null)
			return 0m;

		int nights = (groupRes.ToDate - groupRes.FromDate).Days;
		if (nights < 0) nights = 0;

		decimal total = 0m;

		
		if (groupRes.GroupReservationRooms != null)
		{
			foreach (var grr in groupRes.GroupReservationRooms)
			{
				var room = grr.Room
					?? _context.Rooms.Find(grr.RoomID);
				if (room == null) continue;

				total += room.PricePerNight * nights;
			}
		}

		
		const decimal PRICE_BREAKFAST = 20m;
		const decimal PRICE_LUNCH = 25m;
		const decimal PRICE_DINNER = 30m;

		total += (groupRes.BreakfastAdults + groupRes.BreakfastChildren) * PRICE_BREAKFAST * nights;
		total += (groupRes.LunchAdults + groupRes.LunchChildren) * PRICE_LUNCH * nights;
		total += (groupRes.DinnerAdults + groupRes.DinnerChildren) * PRICE_DINNER * nights;

		return total;
	}

	
	public List<Room> FindMinRoomsForGuestsBacktracking(List<Room> candidateRooms, int totalGuests)
	{
		return candidateRooms
			.Where(r => (r.Accommodation?.MaxGuests ?? 0) >= totalGuests)
			.ToList();
	}
}
