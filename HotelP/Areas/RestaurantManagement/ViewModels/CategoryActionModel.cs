using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.RestaurantManagement.ViewModels
{
    public class CategoryActionModel
    {
        public int ID { get; set; } // ID kategorii

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; } // Nazwa kategorii

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } // Opis kategorii
    }
}
