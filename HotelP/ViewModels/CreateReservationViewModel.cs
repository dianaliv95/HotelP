using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel.ViewModels
{
    public class CreateReservationViewModel
    {
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }

        [Range(1, 20)]
        public int AdultCount { get; set; }

        [Range(0, 20)]
        public int ChildrenCount { get; set; }

        [Required]
        public int RoomID { get; set; }

        // Dane gościa
        [Required(ErrorMessage = "Pole 'Imię' jest wymagane.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Pole 'Nazwisko' jest wymagane.")]
        public string LastName { get; set; }

        // Ewentualnie: Email, Phone, ...
    }
}
