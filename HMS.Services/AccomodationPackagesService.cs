using HMS.Data;
using HMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<List<AccommodationPackage>> GetAllAccommodationPackagesAsync()
        {
            return await _context.AccommodationPackages.ToListAsync();
        }
        public List<AccommodationPackage> SearchAccommodationPackages(string searchterm, int? accommodationTypeID, int page, int recordSize)
        {
             page = page < 1 ? 1 : page;


            var query = _context.AccommodationPackages
                .Include(p => p.AccommodationType) // Upewnij się, że relacja jest ładowana
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchterm))
            {
                searchterm = searchterm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchterm));
            }

            if (accommodationTypeID.HasValue && accommodationTypeID.Value > 0)
            {
                query = query.Where(p => p.AccommodationTypeID == accommodationTypeID.Value);
            }

            // Obliczanie przesunięcia
            var skip = (page - 1) * recordSize;

            // Pobierz stronę wyników
            return query.OrderBy(x=>x.AccommodationTypeID).Skip(skip).Take(recordSize).ToList();
        }

        public int SearchAccommodationPackagesCount(string searchterm, int? accommodationTypeID)
        {
            

            var query = _context.AccommodationPackages
                .Include(p => p.AccommodationType) // Upewnij się, że relacja jest ładowana
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchterm))
            {
                searchterm = searchterm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchterm));
            }

            if (accommodationTypeID.HasValue && accommodationTypeID.Value > 0)
            {
                query = query.Where(p => p.AccommodationTypeID == accommodationTypeID.Value);
            }

            // Obliczanie przesunięcia
            

            // Pobierz stronę wyników
            return query.Count();
        }

        public AccommodationPackage GetAccommodationPackageByID(int ID)
        {
            return _context.AccommodationPackages.Find(ID);
        }

        public bool SaveAccommodationPackage(AccommodationPackage AccommodationPackage)
        {
            try
            {
                _context.AccommodationPackages.Add(AccommodationPackage);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu: {ex.Message}");
                return false;
            }
        }

        public bool UpdateAccommodationPackage(AccommodationPackage AccommodationPackage)
        {
            try
            {
                _context.Entry(AccommodationPackage).State = EntityState.Modified;
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu: {ex.Message}");
                return false;
            }
        }

        public bool DeleteAccommodationPackage(int id)
        {
            try
            {
                var AccommodationPackage = _context.AccommodationPackages.Find(id);

                // Jeśli nie znaleziono, zwróć false
                if (AccommodationPackage == null)
                {
                    return false;
                }

                // Usuwamy rekord
                _context.AccommodationPackages.Remove(AccommodationPackage);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Zwróć false w przypadku błędu
                Console.WriteLine($"Błąd podczas usuwania: {ex.Message}");
                return false;
            }
        }
    }
}
