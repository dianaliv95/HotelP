using HMS.Data;
using HMS.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HMS.Services
{
    public class CategoryService
    {
        private readonly HMSContext _context;

        public CategoryService(HMSContext context)
        {
            _context = context;
        }

        public List<Category> GetAllCategories()
        {
            return _context.Categories.ToList();
        }

        public List<Category> SearchCategory(string searchTerm)
        {
            var Category = _context.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                Category = Category.Where(a => a.Name.ToLower().Contains(searchTerm));
            }
            return Category.ToList();
        }


        public Category GetCategoryById(int id)
        {
            return _context.Categories.Find(id);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        public bool SaveCategory(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCategory(Category category)
        {
            try
            {
                _context.Entry(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCategory(int id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category == null) return false;

                _context.Categories.Remove(category);
                return _context.SaveChanges() > 0;
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
