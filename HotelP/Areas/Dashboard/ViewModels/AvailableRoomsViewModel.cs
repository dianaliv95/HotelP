using HMS.Entities;
using System;
using System.Collections.Generic;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class AvailableRoomsViewModel
    {
        public Accommodation Accommodation { get; set; }
        public List<Room> AvailableRooms { get; set; }
        public DateTime FromDate { get; set; } // Dodano tę właściwość
        public DateTime ToDate { get; set; }   // Dodano tę właściwość
    }

}
