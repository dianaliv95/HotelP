using HMS.Entities;
using HMS.ViewModels;

namespace Hotel.Areas.Dashboard.ViewModels
{
	public class AccommodationPackagesListingModel
	{
		// Lista pakietów zakwaterowania z mapowaniem do widoku
		public IEnumerable<AccommodationPackageViewModel> AccommodationPackages { get; set; }

		// ID wybranego typu zakwaterowania (filtrowanie)
		public int? AccommodationTypeID { get; set; }

		// Lista dostępnych typów zakwaterowania
		public IEnumerable<AccommodationTypeViewModel> AccommodationTypes { get; set; }

		// Pole wyszukiwania
		public string SearchTerm { get; set; }
		public Pager Pager { get; set; }
	}
	public class AccommodationPackageViewModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public decimal FeePerNight { get; set; }
		public int AccommodationTypeID { get; set; }
		public string AccommodationTypeName { get; set; } // Dodaj pole na nazwę typu zakwaterowania
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

		// Lista typów zakwaterowania dla pola wyboru
		public List<AccommodationTypeViewModel> AccommodationTypes { get; set; }
		public List<AccommodationPackagePicture> AccommodationPackagePictures { get; set; }

	}

	public class AccommodationTypeViewModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
	}


}

