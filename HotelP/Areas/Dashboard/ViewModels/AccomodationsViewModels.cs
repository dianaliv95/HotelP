using HMS.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class AccommodationsListingModel
    {
        public List<Accommodation> Accommodations { get; set; }
        public string SearchTerm { get; set; }
        public int? SelectedPackageId { get; set; }
        public int? AccommodationPackageID { get; set; } // Dodane pole
        public List<AccommodationPackage> Packages { get; set; }
    }


    public class AccommodationActionModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Nazwa zakwaterowania jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa zakwaterowania nie może przekraczać 100 znaków.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Accommodation package is required.")]
        public int AccommodationPackageID { get; set; }

        [Required(ErrorMessage = "Liczba pokoi jest wymagana.")]
        [Range(1, 10, ErrorMessage = "Liczba pokoi musi być między 1 a 10.")]
        public int NumberOfRooms { get; set; }

        [Range(1, 10, ErrorMessage = "Liczba gości musi być między 1 a 10.")]
        public int MaxGuests { get; set; }

        [Range(1, 10, ErrorMessage = "Liczba dorosłych musi być między 1 a 10.")]
        public int MaxAdults { get; set; }

        [Range(0, 10, ErrorMessage = "Liczba dzieci musi być między 0 a 10.")]
        public int MaxChildren { get; set; }


        [BindNever]
        public List<AccommodationPackage> Packages { get; set; } = new List<AccommodationPackage>();
    }
}



