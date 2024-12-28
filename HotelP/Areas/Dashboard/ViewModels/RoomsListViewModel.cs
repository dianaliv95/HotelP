using HMS.Entities;
using System;
using System.Collections.Generic;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class RoomsListViewModel
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SelectedStatus { get; set; } // Available / Blocked / Reserved
    }
}
