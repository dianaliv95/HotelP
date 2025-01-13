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
    public class AccommodationPackagesService
    {
        private readonly HMSContext _context;
        private readonly ILogger<AccommodationPackagesService> _logger;

        public AccommodationPackagesService(HMSContext context, ILogger<AccommodationPackagesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 1) Zwraca wszystkie pakiety (asynchronicznie)
        public async Task<List<AccommodationPackage>> GetAllAccommodationPackagesAsync()
        {
            return await _context.AccommodationPackages
                .Include(ap => ap.AccomodationPackagePictures)
                    .ThenInclude(pp => pp.Picture)
                .ToListAsync();
        }


        // 2) Zwraca pakiety po typie (asynchronicznie)
        public async Task<List<AccommodationPackage>> GetAllAccommodationPackagesByAccommodationTypeAsync(int accommodationTypeID)
        {
            return await _context.AccommodationPackages
                                 .Where(x => x.AccommodationTypeID == accommodationTypeID)
                                 .ToListAsync();
        }

        // 3) Wyszukiwarka + stronicowanie (synchronicznie)
        public List<AccommodationPackage> SearchAccommodationPackages(string searchTerm, int? accommodationTypeID, int page, int recordSize)
        {
            // Zabezpieczenie, żeby nie wprowadzić np. page=0
            if (page < 1) page = 1;

            var query = _context.AccommodationPackages
                .Include(p => p.AccommodationType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            if (accommodationTypeID.HasValue && accommodationTypeID.Value > 0)
            {
                query = query.Where(p => p.AccommodationTypeID == accommodationTypeID.Value);
            }

            // stronicowanie
            var skip = (page - 1) * recordSize;
            return query.OrderBy(x => x.AccommodationTypeID)
                        .Skip(skip)
                        .Take(recordSize)
                        .ToList();
        }

        // 4) Zliczenie wszystkich rekordów dla wyszukiwarki
        public int SearchAccommodationPackagesCount(string searchTerm, int? accommodationTypeID)
        {
            var query = _context.AccommodationPackages
                .Include(p => p.AccommodationType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            if (accommodationTypeID.HasValue && accommodationTypeID.Value > 0)
            {
                query = query.Where(p => p.AccommodationTypeID == accommodationTypeID.Value);
            }

            return query.Count();
        }

        // 5) Pobieranie pakietu z wczytanymi obrazkami
        public AccommodationPackage GetAccommodationPackageByID(int id)
        {
            // Wczytujemy też listę AccommodationPackagePictures oraz sam Picture
            return _context.AccommodationPackages
                           .Include(ap => ap.AccomodationPackagePictures)
                           .ThenInclude(pp => pp.Picture)
                           .FirstOrDefault(ap => ap.ID == id);
        }

        // 6) Zapisywanie nowego pakietu
        public bool SaveAccommodationPackage(AccommodationPackage package)
        {
            try
            {
                _context.AccommodationPackages.Add(package);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisu nowego AccommodationPackage");
                return false;
            }
        }

        // 7) Aktualizacja pakietu – usuwanie i ponowne dodanie obrazków
        public bool UpdateAccommodationPackage(AccommodationPackage package)
        {
            try
            {
                var existingPackage = _context.AccommodationPackages
                    .Include(x => x.AccomodationPackagePictures)
                    .FirstOrDefault(x => x.ID == package.ID);

                if (existingPackage == null)
                {
                    _logger.LogWarning($"Package with ID={package.ID} not found.");
                    return false;
                }

                // 1) Odfiltruj tylko te obrazki, które istnieją w bazie (ID > 0).
                var oldPictures = existingPackage.AccomodationPackagePictures
                    .Where(p => p.ID > 0)
                    .ToList();

                // 2) Usuń stare, które faktycznie istniały w bazie.
                _context.AccommodationPackagePictures.RemoveRange(oldPictures);

                // 3) Nadpisz dane w existingPackage (Name, Fee, itp.)
                _context.Entry(existingPackage).CurrentValues.SetValues(package);

                // 4) Dodaj nową listę z package.AccomodationPackagePictures
                //    (one będą miały ID=0, i EF utworzy je jako INSERT)
                if (package.AccomodationPackagePictures != null)
                {
                    existingPackage.AccomodationPackagePictures.AddRange(package.AccomodationPackagePictures);
                }

                // 5) Save
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisu w UpdateAccommodationPackage()");
                return false;
            }
        }


        // 8) Usuwanie pakietu
        public bool DeleteAccommodationPackage(int id)
        {
            try
            {
                var package = _context.AccommodationPackages.Find(id);
                if (package == null) return false;

                _context.AccommodationPackages.Remove(package);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania AccommodationPackage");
                return false;
            }
        }

        // 9) Dodatkowa metoda do pobrania samych obrazków (jak w starej wersji)
        public List<AccommodationPackagePicture> GetPicturesByAccommodationPackageID(int accommodationPackageID)
        {
            var package = _context.AccommodationPackages
                                  .Include(ap => ap.AccomodationPackagePictures)
                                  .ThenInclude(pp => pp.Picture)
                                  .FirstOrDefault(ap => ap.ID == accommodationPackageID);

            return package?.AccomodationPackagePictures.ToList() ?? new List<AccommodationPackagePicture>();
        }
    }
}
