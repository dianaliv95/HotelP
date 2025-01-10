using HMS.Entities;

namespace Hotel.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<AccommodationType> AccommodationTypes { get; set; }
        public IEnumerable<AccommodationPackage> AccommodationPackages { get; set; }
    }
}
