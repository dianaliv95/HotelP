using HMS.Entities;

namespace Hotel.Areas.RestaurantManagement.ViewModels
{
    public class DishesListingModel
    {
        public List<Dish> Dishes { get; set; } // Lista dań
        public string SearchTerm { get; set; } // Wpisany tekst do wyszukiwania
        public int? SelectedCategoryId { get; set; } // Wybrana kategoria (ID)
        public List<Category> Categories { get; set; } // Lista dostępnych kategorii
        public string SelectedCategoryName
        {
            get
            {
                return Categories?.FirstOrDefault(c => c.ID == SelectedCategoryId)?.Name ?? "All Categories";
            }
        } // Wyświetlenie nazwy wybranej kategorii (opcjonalne)
    }
}
