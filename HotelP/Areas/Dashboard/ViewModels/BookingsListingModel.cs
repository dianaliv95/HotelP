using HMS.Entities;
using System.Collections.Generic;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class BookingsListingModel
    {
        public List<Reservation> Bookings { get; set; }
        public string SearchTerm { get; set; }
        public int? SelectedAccommodationId { get; set; }
        public List<Accommodation> Accommodations { get; set; }
    }
}
