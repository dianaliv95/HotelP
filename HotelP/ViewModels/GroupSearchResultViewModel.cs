using System;
using System.Collections.Generic;
using HMS.Entities;

namespace Hotel.ViewModels
{
    public class GroupSearchResultViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }

        public List<Room> FoundRooms { get; set; } = new();
    }
}
