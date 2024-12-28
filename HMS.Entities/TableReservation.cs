using System.ComponentModel.DataAnnotations;

namespace HMS.Entities
{
    public class TableReservation
    {
        public int ID { get; set; }

        [Required]
        public string GuestName { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public int TableNumber { get; set; }

        public bool IsHotelGuest { get; set; } // true = gość hotelowy, false = zewnętrzny
    }
}
