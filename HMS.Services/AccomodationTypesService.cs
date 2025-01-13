using HMS.Data;
using HMS.Entities;
using HMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HMS.Services
{
    public class AccommodationTypesService
    {
        private readonly HMSContext _context;
        private readonly ILogger<AccommodationTypesService> _logger;

        public AccommodationTypesService(HMSContext context, ILogger<AccommodationTypesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<AccommodationType> GetAllAccommodationTypes()
        {
            return _context.AccommodationTypes.ToList();
        }

        public List<AccommodationType> SearchAccommodationTypes(string searchTerm)
        {
            var AccommodationTypes = _context.AccommodationTypes.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                AccommodationTypes = AccommodationTypes.Where(a => a.Name.ToLower().Contains(searchTerm));
            }
            return AccommodationTypes.ToList();
        }

        public AccommodationType GetAccommodationTypeByID(int ID)
        {
            return _context.AccommodationTypes.Find(ID);
        }

        public bool SaveAccommodationType(AccommodationType AccommodationType)
        {
            try
            {
                _context.AccommodationTypes.Add(AccommodationType);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu: {ex.Message}");
                return false;
            }
        }
        public bool UpdateAccommodationType(AccommodationType AccommodationType)
        {
            try
            {
                _context.Entry(AccommodationType).State = EntityState.Modified;
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu: {ex.Message}");
                return false;
            }
        }

        public bool DeleteAccommodationType(int id)
        {
            try
            {
                var AccommodationType = _context.AccommodationTypes.Find(id);

                // Jeśli nie znaleziono, zwróć false
                if (AccommodationType == null)
                {
                    return false;
                }

                // Usuwamy rekord
                _context.AccommodationTypes.Remove(AccommodationType);
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
