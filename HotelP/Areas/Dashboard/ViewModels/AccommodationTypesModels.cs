using HMS.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class AccommodationTypesListingModel
    {
        public IEnumerable<AccommodationType> AccommodationTypes { get; set; }
        public string SearchTerm { get; set; }
    }


    public class AccommodationTypesActionModel
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int AccommodationPackageID { get; set; }

        public List<AccommodationPackage> Packages { get; set; }

    }
}

