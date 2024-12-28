using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Entities
{
    public class ReservationAjaxDto
    {
        public int? RoomID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime DateTo { get; set; }

        // Ilość dorosłych i dzieci
        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }

        // Pola posiłków
        public int BreakfastAdults { get; set; }
        public int BreakfastChildren { get; set; }
        public int LunchAdults { get; set; }
        public int LunchChildren { get; set; }
        public int DinnerAdults { get; set; }
        public int DinnerChildren { get; set; }
    }
}

