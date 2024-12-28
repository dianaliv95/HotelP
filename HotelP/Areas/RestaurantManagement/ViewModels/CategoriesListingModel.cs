using HMS.Entities;

namespace Hotel.Areas.RestaurantManagement.ViewModels
{
    public class CategoriesListingModel
    {
        public List<Category> Categories { get; set; } // Lista kategorii
        public string SearchTerm { get; set; } // Wpisany tekst do wyszukiwania
    }
}
