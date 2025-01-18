using HMS.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class BookingActionModel
    {
        public int ID { get; set; }

        public string ReservationNumber { get; set; }
        [Required]
        // Pokój
        public int? RoomID { get; set; }

        // Lista pokoi (np. do dropdown)
        public List<Room> Rooms { get; set; } = new List<Room>();

        // Ewentualnie do wyświetlania listy dostępnych pokoi 
        public List<Room> AvailableRooms { get; set; } = new List<Room>();

        [Required(ErrorMessage = "Pole 'Imię' jest wymagane.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Pole 'Nazwisko' jest wymagane.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana.")]
        public DateTime FromDate { get; set; }
        [Required(ErrorMessage = "Data zakończenia jest wymagana.")]
        public DateTime DateTo { get; set; }

        [Range(1, 20, ErrorMessage = "Liczba dorosłych musi być między 1 a 20.")]
        public int AdultCount { get; set; }

        [Range(0, 20, ErrorMessage = "Liczba dzieci musi być między 0 a 20.")]
        public int ChildrenCount { get; set; }

        // Liczba dorosłych/dzieci korzystających z posiłków
        [Range(0, 20)]
        public int BreakfastAdults { get; set; }
        [Range(0, 20)]
        public int LunchAdults { get; set; }
        [Range(0, 20)]
        public int DinnerAdults { get; set; }

        [Range(0, 20)]
        public int BreakfastChildren { get; set; }
        [Range(0, 20)]
        public int LunchChildren { get; set; }
        [Range(0, 20)]
        public int DinnerChildren { get; set; }

        public bool IsPaid { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public ReservationStatus Status { get; set; }


        public decimal TotalPrice { get; set; }

        // Czas trwania w dniach – można liczyć w locie:
        public int Duration { get; set; }

        // Dla ewentualnego widoku
        public Room Room { get; set; }
        public string? ContactPhone { get; set; } // Numer kontaktowy
        public string? ContactEmail { get; set; } // Adres e-mail
        public List<ReservationStatus> AllowedStatuses { get; set; }
            = new List<ReservationStatus>();
    }
}
