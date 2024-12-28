using HMS.Data;
using HMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HMS.Services
{
    public class DishesService
    {
        private readonly HMSContext _context;
        private readonly ILogger<DishesService> _logger;

        public DishesService(HMSContext context, ILogger<DishesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Pobieranie wszystkich dań
        public List<Dish> GetAllDishes()
        {
            try
            {
                return _context.Dishes
                    .Include(d => d.Category) // Pobierz kategorię dla każdego dania
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich dań.");
                return new List<Dish>();
            }
        }

        // Pobieranie dań na podstawie filtrów
        public List<Dish> SearchDishes(string searchTerm, int? categoryId = null)
        {
            try
            {
                var query = _context.Dishes
                    .Include(d => d.Category) // Pobierz powiązane kategorie
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(d => d.Name.ToLower().Contains(searchTerm.ToLower()));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(d => d.CategoryID == categoryId.Value);
                }

                return query.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wyszukiwania dań.");
                return new List<Dish>();
            }
        }

        // Pobieranie dania na podstawie ID
        public Dish GetDishById(int id)
        {
            try
            {
                return _context.Dishes
                    .Include(d => d.Category) // Pobierz kategorię powiązaną z daniem
                    .FirstOrDefault(d => d.ID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania dania o ID {ID}.", id);
                return null;
            }
        }

        // Dodawanie nowego dania
        public bool SaveDish(Dish dish)
        {
            try
            {
                _context.Dishes.Add(dish);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisywania nowego dania.");
                return false;
            }
        }

        // Edytowanie dania
        public bool UpdateDish(Dish dish)
        {
            try
            {
                _context.Entry(dish).State = EntityState.Modified;
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji dania o ID {ID}.", dish.ID);
                return false;
            }
        }

        // Usuwanie dania
        public bool DeleteDish(int id)
        {
            try
            {
                var dish = _context.Dishes.Find(id);

                if (dish == null)
                {
                    _logger.LogWarning("Nie znaleziono dania o ID {ID} do usunięcia.", id);
                    return false;
                }

                _context.Dishes.Remove(dish);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania dania o ID {ID}.", id);
                return false;
            }
        }

        // Pobieranie dań na podstawie kategorii
        public List<Dish> GetDishesByCategory(int categoryId)
        {
            try
            {
                return _context.Dishes
                    .Include(d => d.Category) // Pobierz powiązane kategorie
                    .Where(d => d.CategoryID == categoryId)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania dań dla kategorii o ID {CategoryID}.", categoryId);
                return new List<Dish>();
            }
        }
    }
}
