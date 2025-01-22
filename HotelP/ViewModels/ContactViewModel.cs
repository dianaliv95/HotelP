using System.ComponentModel.DataAnnotations;

namespace HotelP.ViewModels
{
	public class ContactViewModel
	{
		public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Wpisz poprawny adres e-mail.")]

        public string Email { get; set; }
		public string Message { get; set; }
	}
}
