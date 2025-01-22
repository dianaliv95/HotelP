using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Entities
{
    public enum ReservationStatus
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

    public enum PaymentMethod
    {
        [Display(Name = "Karta")]
        Card,

        [Display(Name = "Gotówka")]
        Cash
    }

    public class Reservation
    {
        [Key]
        public int ID { get; set; }

        
        public string? UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public int AccommodationID { get; set; }
        [ForeignKey("AccommodationID")]
        public virtual Accommodation Accommodation { get; set; }
        [Required]
        public int RoomID { get; set; }
        [ForeignKey("RoomID")]
        public virtual Room Room { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public string ReservationNumber { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }

        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }

       
        public int BreakfastAdults { get; set; }
        public int LunchAdults { get; set; }
        public int DinnerAdults { get; set; }

        public int BreakfastChildren { get; set; }
        public int LunchChildren { get; set; }
        public int DinnerChildren { get; set; }
        public Payment Payment { get; set; }


       

        public bool IsPaid { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public ReservationStatus Status { get; set; }

        [NotMapped]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
       
        public string? ContactPhone { get; set; } // Numer kontaktowy
        public string? ContactEmail { get; set; } // Adres e-mail
    }
}
