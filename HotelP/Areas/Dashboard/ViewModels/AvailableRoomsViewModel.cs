using HMS.Entities;
using System;
using System.Collections.Generic;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class AvailableRoomsViewModel
    {
        public Accommodation Accommodation { get; set; }
        public List<Room> AvailableRooms { get; set; }
        public DateTime FromDate { get; set; } 
        public DateTime ToDate { get; set; }  
    }

}
