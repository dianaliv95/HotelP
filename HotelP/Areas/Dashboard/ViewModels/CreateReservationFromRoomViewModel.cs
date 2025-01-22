using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class CreateReservationFromRoomViewModel
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; }

        public string ReservationNumber { get; set; } 

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }
    }
}
