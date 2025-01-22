using System.ComponentModel.DataAnnotations;

namespace HotelP.ViewModels
{
    public class CheckAccommodationAvailabilityViewModel
    {
        public DateTime? FromDate { get; set; }
        public int Duration { get; set; }
        public int NoOfAdults { get; set; }
        public int NoOfChildren { get; set; }
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Wpisz poprawny adres e-mail.")]
        public string Email { get; set; }
        public string Notes { get; set; }

    }
}
