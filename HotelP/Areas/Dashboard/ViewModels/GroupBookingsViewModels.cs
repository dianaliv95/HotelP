using HMS.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class GroupBookingActionModel
    {
        public int ID { get; set; }

        public string ReservationNumber { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana.")]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Data zakończenia jest wymagana.")]
        public DateTime ToDate { get; set; } = DateTime.Today.AddDays(1);

        [Range(1, 50, ErrorMessage = "Liczba dorosłych musi być >= 1.")]
        public int AdultCount { get; set; } = 1;

        [Range(0, 50, ErrorMessage = "Liczba dzieci nie może być ujemna.")]
        public int ChildrenCount { get; set; } = 0;

        // Posiłki
        public int BreakfastAdults { get; set; }
        public int BreakfastChildren { get; set; }
        public int LunchAdults { get; set; }
        public int LunchChildren { get; set; }
        public int DinnerAdults { get; set; }
        public int DinnerChildren { get; set; }

        // Płatność
        public bool IsPaid { get; set; }

        public PaymentsMethod? PaymentMethod { get; set; }
        public GroupReservationStatus RStatus { get; set; }


        // Dodatkowo do kalkulacji
        public decimal TotalPrice { get; set; }

        [Required]
        public string ContactPhone { get; set; } = "";

        [Required]
        public string ContactEmail { get; set; } = "";

        // Lista wszystkich dostępnych pokoi (checkbox)
        public List<Room> AvailableRooms { get; set; } = new();

        // ID-y pokoi wybranych w formularzu
        public List<int> SelectedRoomIDs { get; set; } = new();

        // Timestamp
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<GroupReservationStatus> AllowedGroupStatuses { get; set; }
            = new List<GroupReservationStatus>();
    }
}
