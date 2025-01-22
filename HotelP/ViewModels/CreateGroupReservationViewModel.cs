using System;
using System.Collections.Generic;

namespace Hotel.ViewModels
{
    public class CreateGroupReservationViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }


        public List<int> SelectedRoomIDs { get; set; } = new();

        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public bool IsBreakfastIncluded { get; set; }
    }
}
