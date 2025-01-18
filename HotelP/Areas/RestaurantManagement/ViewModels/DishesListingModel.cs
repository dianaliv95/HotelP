using HMS.Entities;

namespace Hotel.Areas.RestaurantManagement.ViewModels
{
    public class DishesListingModel
    {
        public List<Dish> Dishes { get; set; } // Lista dań
        public string SearchTerm { get; set; } // Wyszukiwanie
        public int? SelectedCategoryId { get; set; } // Wybrana kategoria
        public List<Category> Categories { get; set; } = new();

        public string SelectedCategoryName
        {
            get
            {
                return Categories?.FirstOrDefault(c => c.ID == SelectedCategoryId)?.Name
                       ?? "All Categories";
            }
        }
    }
}
