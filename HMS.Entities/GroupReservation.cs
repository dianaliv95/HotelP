using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;     
using System.ComponentModel.DataAnnotations.Schema; 

namespace HMS.Entities
{
    public enum GroupReservationStatus
    {
        [Display(Name = "Rezerwacja wstępna")]
        PreliminaryReservation,

        [Display(Name = "Rezerwacja potwierdzona")]
        ConfirmedReservation,

        [Display(Name = "Pobyt rozliczny")]
        SettledStay,

        [Display(Name = "Klient nie przyjechał")]
        NoShow,

        [Display(Name = "Pobyt zakończony rozliczany")]
        CompletedSettledStay,

        [Display(Name = "Pobyt nierozliczny (nieopłacony)")]
        UnsettledStay,

        [Display(Name = "Pobyt zakończony nierozliczony (nieopłacony)")]
        CompletedUnsettledStay
    }

    public enum PaymentsMethod
    {
        [Display(Name = "Karta")]
        Card,

        [Display(Name = "Gotówka")]
        Cash
    }

    public class GroupReservation
    {
        public int ID { get; set; }

        [Required]
        public string ReservationNumber { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Range(1, 99)]
        public int AdultCount { get; set; }

        [Range(0, 99)]
        public int ChildrenCount { get; set; }

        [Required]
        public string ContactPhone { get; set; } = "";
        public bool IsBreakfastIncluded { get; set; }

        [Required]
        public string ContactEmail { get; set; } = "";

        
        public int BreakfastAdults { get; set; }
        public int BreakfastChildren { get; set; }
        public int LunchAdults { get; set; }
        public int LunchChildren { get; set; }
        public int DinnerAdults { get; set; }
        public int DinnerChildren { get; set; }

        // Status i płatność
        public bool IsPaid { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public GroupReservationStatus RStatus { get; set; }

        // Łączna kwota – pole [NotMapped], liczymy w locie
        [NotMapped]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Nawigacja do powiązanych pokojów
        public virtual List<GroupReservationRoom> GroupReservationRooms { get; set; } = new();
    }
}
