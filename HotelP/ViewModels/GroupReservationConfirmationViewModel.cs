using System;

namespace Hotel.ViewModels
{
    public class GroupReservationConfirmationViewModel
    {
        public string ReservationNumber { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
