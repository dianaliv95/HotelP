using HMS.Entities;

namespace Hotel.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<AccommodationType> AccommodationTypes { get; set; }
        public IEnumerable<AccommodationPackage> AccommodationPackages { get; set; }
        public List<Category> DishCategories { get; set; } = new();

        // Wszystkie dania (lub wczytane filtrowane) - do dopasowania w widoku
        public List<Dish> AllDishes { get; set; } = new();
    }
}
