using HMS.Entities;
using HMS.ViewModels;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class AccommodationPackagesListingModel
    {
        public IEnumerable<AccommodationPackageViewModel> AccommodationPackages { get; set; }

        public int? AccommodationTypeID { get; set; }

        public IEnumerable<AccommodationTypeViewModel> AccommodationTypes { get; set; }

        public string SearchTerm { get; set; }
        public Pager Pager { get; set; }
    }
    public class AccommodationPackageViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal FeePerNight { get; set; }
        public int AccommodationTypeID { get; set; }
        public string AccommodationTypeName { get; set; } 
    }

    public class AccommodationPackageActionModel
    {
        public int ID { get; set; }
        public int AccommodationTypeID { get; set; }
        public AccommodationType AccommodationType { get; set; }
        public string Name { get; set; }
        public int NoOfRoom { get; set; }
        public decimal FeePerNight { get; set; }
        public string PictureIDs { get; set; }

        public List<AccommodationTypeViewModel> AccommodationTypes { get; set; }
        public List<AccommodationPackagePicture> AccommodationPackagePictures { get; set; }

    }

    public class AccommodationTypeViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }


}

