using HMS.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class RoomViewModel
    {
        [Required(ErrorMessage = "Nazwa pokoju jest wymagana.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Zakwaterowanie jest wymagane.")]
        public int AccommodationID { get; set; }

        [Required(ErrorMessage = "Cena za noc jest wymagana.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa niż 0.")]
        public decimal PricePerNight { get; set; }

        public List<Accommodation> Accommodations { get; set; }
    }
}
