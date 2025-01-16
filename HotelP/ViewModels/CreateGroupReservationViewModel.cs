using System;
using System.Collections.Generic;

namespace Hotel.ViewModels
{
    public class CreateGroupReservationViewModel
    {
        // Daty i liczba gości
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }


        // Lista identyfikatorów pokojów, które user chce zarezerwować
        public List<int> SelectedRoomIDs { get; set; } = new();

        // Pola kontaktowe
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        // Wyżywienie
        public bool IsBreakfastIncluded { get; set; }
    }
}
