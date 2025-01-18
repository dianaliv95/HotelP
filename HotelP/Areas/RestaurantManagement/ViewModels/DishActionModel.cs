using System.ComponentModel.DataAnnotations;
using HMS.Entities;

namespace Hotel.Areas.RestaurantManagement.ViewModels
{
    public class DishActionModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Dish name is required.")]
        [StringLength(100, ErrorMessage = "Dish name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryID { get; set; }

        public List<Category> Categories { get; set; } = new();

        // ID-ki zdjęć wgranych
        public string PictureIDs { get; set; }

        // Do podglądu istniejących zdjęć 
        public List<DishPicture> DishPictures { get; set; } = new();
    }
}
