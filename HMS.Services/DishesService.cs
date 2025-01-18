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

        // Pobieranie wszystkich dań (z kategorią i zdjęciami)
        public List<Dish> GetAllDishes()
        {
            try
            {
                return _context.Dishes
                    .Include(d => d.Category)
                    .Include(d => d.DishPictures)
                        .ThenInclude(dp => dp.Picture)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich dań.");
                return new List<Dish>();
            }
        }

        // Wyszukiwanie dań z opcjonalnym filtrowaniem po categoryId
        public List<Dish> SearchDishes(string searchTerm, int? categoryId)
        {
            try
            {
                var query = _context.Dishes
                    .Include(d => d.Category)
                    .Include(d => d.DishPictures).ThenInclude(dp => dp.Picture)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(d => d.Name.ToLower().Contains(searchTerm));
                }

                if (categoryId.HasValue && categoryId.Value > 0)
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

        // Pobieranie dania po ID
        public Dish GetDishById(int id)
        {
            try
            {
                return _context.Dishes
                    .Include(d => d.Category)
                    .Include(d => d.DishPictures).ThenInclude(dp => dp.Picture)
                    .FirstOrDefault(d => d.ID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania dania o ID {ID}.", id);
                return null;
            }
        }

        // Dodawanie nowego dania (z listą pictureIDs)
        public bool SaveDish(Dish newDish, List<int> pictureIDs)
        {
            try
            {
                // Inicjalizuj kolekcję
                newDish.DishPictures = new List<DishPicture>();

                if (pictureIDs != null)
                {
                    foreach (var picId in pictureIDs)
                    {
                        newDish.DishPictures.Add(new DishPicture
                        {
                            PictureID = picId
                        });
                    }
                }

                _context.Dishes.Add(newDish);
                var changes = _context.SaveChanges();
                _logger.LogInformation("SaveDish: SaveChanges => {Changes}", changes);

                return changes > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisu nowego dania.");
                return false;
            }
        }

        // Aktualizacja dania (nadpisanie zdjęć)
        public bool UpdateDish(Dish dish, List<int> pictureIDs)
        {
            try
            {
                // Wczytujemy oryginał
                var existing = _context.Dishes
                    .Include(d => d.DishPictures)
                    .FirstOrDefault(d => d.ID == dish.ID);

                if (existing == null)
                {
                    _logger.LogWarning("Dish (ID={0}) nie istnieje.", dish.ID);
                    return false;
                }

                // Usuwamy stare powiązania
                _context.DishPictures.RemoveRange(existing.DishPictures);

                // Nadpisujemy podstawowe pola
                _context.Entry(existing).CurrentValues.SetValues(dish);

                // Dodajemy nowe
                existing.DishPictures = new List<DishPicture>();

                if (pictureIDs != null && pictureIDs.Count > 0)
                {
                    foreach (var picId in pictureIDs)
                    {
                        existing.DishPictures.Add(new DishPicture
                        {
                            PictureID = picId
                        });
                    }
                }

                var changes = _context.SaveChanges();
                _logger.LogInformation("UpdateDish: SaveChanges => {Changes}", changes);

                return changes > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji dania {0}", dish.ID);
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
                    _logger.LogWarning("Nie znaleziono dania ID={0} do usunięcia.", id);
                    return false;
                }

                _context.Dishes.Remove(dish);
                var changes = _context.SaveChanges();
                _logger.LogInformation("DeleteDish: SaveChanges => {Changes}", changes);

                return changes > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania dania o ID={0}.", id);
                return false;
            }
        }

        // Dania według kategorii
        public List<Dish> GetDishesByCategory(int categoryId)
        {
            try
            {
                return _context.Dishes
                    .Include(d => d.Category)
                    .Include(d => d.DishPictures).ThenInclude(dp => dp.Picture)
                    .Where(d => d.CategoryID == categoryId)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania dań dla kategorii={0}.", categoryId);
                return new List<Dish>();
            }
        }
    }
}
