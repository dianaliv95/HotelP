using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Pole e-mail jest wymagane.")]
        [EmailAddress(ErrorMessage = "Wprowadź poprawny adres e-mail.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole hasło jest wymagane.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }
}
