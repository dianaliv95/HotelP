using System.ComponentModel.DataAnnotations;
using HMS.Entities;

namespace Hotel.Areas.RestaurantManagement.ViewModels
{
    public class DishActionModel
    {
        public int ID { get; set; } // ID dania

        [Required(ErrorMessage = "Dish name is required.")]
        [StringLength(100, ErrorMessage = "Dish name cannot exceed 100 characters.")]
        public string Name { get; set; } // Nazwa dania

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } // Opis dania

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10,000.")]
        public decimal Price { get; set; } // Cena dania

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryID { get; set; } // ID wybranej kategorii

        public List<Category> Categories { get; set; } // Lista dostępnych kategorii
    }
}
