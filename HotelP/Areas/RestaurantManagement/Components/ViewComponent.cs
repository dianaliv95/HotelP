using HMS.Services;
using HMS.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hotel.Areas.RestaurantManagement.Components
{
    public class DishCategoriesViewComponent : ViewComponent
    {
        private readonly CategoryService _categoryService;

        public DishCategoriesViewComponent(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Category> categories;
            try
            {
                categories = await _categoryService.GetAllCategoriesAsync(); // Pobierz kategorie asynchronicznie
            }
            catch (Exception ex)
            {
                // Obsługa błędu podczas pobierania danych
                categories = new List<Category>(); // Zwróć pustą listę w przypadku błędu
                Console.WriteLine($"Błąd podczas pobierania kategorii: {ex.Message}");
            }

            return View(categories);
        }
    }
}
