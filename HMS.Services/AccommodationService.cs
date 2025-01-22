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
    public class AccommodationService
    {
        private readonly HMSContext _context;
        private readonly ILogger<AccommodationService> _logger;

        public AccommodationService(HMSContext context, ILogger<AccommodationService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Accommodation> GetAccommodationByIdAsync(int id)
        {
                    try
                    {
                        return await _context.Accommodations
                .Include(a => a.AccommodationPackage)
                .ThenInclude(ap => ap.AccomodationPackagePictures)
                    .ThenInclude(pp => pp.Picture)
            .Include(a => a.AccommodationPictures)
                .ThenInclude(ap => ap.Picture)
            .FirstOrDefaultAsync(a => a.ID == id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Błąd podczas pobierania zakwaterowania o ID {ID}.", id);
                        return null;
                    }
                }

        public async Task<List<Accommodation>> GetAllAccommodationsAsync()
        {
            return await _context.Accommodations
                .Include(a => a.AccommodationPackage)
                // Dołącz pakiety
                .ToListAsync();
        }

        public async Task<List<Accommodation>> GetAllAccommodationsByPackageAsync(int accommodationPackageID)
        {
            return await _context.Accommodations
                                 .Include(a => a.AccommodationPackage)  
                                 .Where(a => a.AccommodationPackageID == accommodationPackageID)
                                 .ToListAsync();
        }
        public async Task<bool> SaveAccommodationWithRoomAsync(Accommodation accommodation, decimal pricePerNight)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Accommodations.Add(accommodation);
                await _context.SaveChangesAsync();

                // Tworzenie jednego pokoju
                var room = new Room
                {
                    Name = $"{accommodation.Name}",
                    AccommodationID = accommodation.ID,
                    PricePerNight = pricePerNight,
                    Status = "Available",
                    AvailableFrom = DateTime.Now, // Zakładamy, że pokój jest dostępny od teraz
                    AvailableTo = null
                };

                _context.Rooms.Add(room);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Błąd podczas zapisywania zakwaterowania i pokoju", ex);
            }
        }

        public async Task<bool> CreateAccommodationAsync(Accommodation accommodation, int numberOfRooms)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Dodaj zakwaterowanie do bazy danych
                _context.Accommodations.Add(accommodation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Zakwaterowanie dodane do bazy danych z ID: {AccommodationID}", accommodation.ID);

                // Pobierz FeePerNight z AccommodationPackage
                var package = await _context.AccommodationPackages.FindAsync(accommodation.AccommodationPackageID);
                if (package == null)
                {
                    _logger.LogError("Nie znaleziono pakietu zakwaterowania o ID: {PackageID}", accommodation.AccommodationPackageID);
                    throw new Exception("Nie znaleziono pakietu zakwaterowania.");
                }

                _logger.LogInformation("Pakiet zakwaterowania znaleziony: {PackageName}, FeePerNight: {FeePerNight}", package.Name, package.FeePerNight);

                // Generuj pokoje dla zakwaterowania
                for (int i = 1; i <= numberOfRooms; i++)
                {
                    var room = new Room
                    {
                        Name = $"{accommodation.Name} - Pokój {i}",
                        AccommodationID = accommodation.ID,
                        IsReserved = false,
                        PricePerNight = package.FeePerNight,
                        Status = "Available" // Ustawienie domyślnego statusu
                    };
                    _context.Rooms.Add(room);
                    _logger.LogInformation("Dodano pokój: {RoomName}, Cena za noc: {PricePerNight}", room.Name, room.PricePerNight);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Zakwaterowanie i pokoje zostały zapisane pomyślnie.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia zakwaterowania i pokoi.");
                await transaction.RollbackAsync();
                return false;
            }
        }



        public async Task<List<Accommodation>> SearchAccommodationsAsync(string searchTerm)
        {
            var query = _context.Accommodations
                .Include(a => a.AccommodationPackage) 
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Name.Contains(searchTerm) || a.Description.Contains(searchTerm));
            }

            return await query.ToListAsync();
        }

        // Nowa metoda: Pobieranie zakwaterowań według pakietu
        public async Task<List<Accommodation>> GetAccommodationsByPackageAsync(int packageId)
        {
            return await _context.Accommodations
                .Include(a => a.AccommodationPackage) 
                .Where(a => a.AccommodationPackageID == packageId) // Filtruj po ID pakietu
                .ToListAsync();
        }

        public async Task<bool> SaveAccommodationAsync(Accommodation accommodation)
        {
            try
            {
                _context.Accommodations.Add(accommodation);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas zapisu: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAccommodationAsync(Accommodation accommodation)
        {
            try
            {
                _context.Entry(accommodation).State = EntityState.Modified;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas aktualizacji: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.Accommodation)
                .ToListAsync();
        }

        public async Task<bool> DeleteAccommodationAsync(int id)
        {
            try
            {
                var accommodation = await _context.Accommodations.FindAsync(id);

                // Jeśli nie znaleziono, zwróć false
                if (accommodation == null)
                {
                    return false;
                }

                // Usuwamy rekord
                _context.Accommodations.Remove(accommodation);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas usuwania: {ex.Message}");
                return false;
            }
        }

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
                _logger.LogError(ex, "Błąd podczas pobierania pokoi dla zakwaterowania ID {AccommodationId}.", accommodationId);
                return new List<Room>();
            }
        }
    }
}
