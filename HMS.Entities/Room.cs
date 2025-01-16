using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Entities
{
    public class Room
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Nazwa pokoju jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa pokoju nie może przekraczać 100 znaków.")]
        public string Name { get; set; }

        public bool IsReserved { get; set; }

        [Required(ErrorMessage = "Status pokoju jest wymagany.")]
        [StringLength(50, ErrorMessage = "Status pokoju nie może przekraczać 50 znaków.")]
        public string Status { get; set; } // "Available", "Blocked"

        [DataType(DataType.Date)]
        public DateTime? AvailableFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AvailableTo { get; set; }
        public DateTime? BlockedFrom { get; set; }
        public DateTime? BlockedTo { get; set; }


        public bool IsBlocked { get; set; } // np. do blokady administracyjnej
		public DateTime? BlockedUntil { get; set; }
		[Required]
        public int AccommodationID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [ForeignKey("AccommodationID")]
        public Accommodation Accommodation { get; set; }

        // Zastąpienie RoomBookings kolekcją Reservations
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
