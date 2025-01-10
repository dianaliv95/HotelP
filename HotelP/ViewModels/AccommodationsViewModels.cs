using HMS.Entities;
using System;
using System.Collections.Generic;

namespace Hotel.ViewModels
{
	public class AccommodationsViewModel
	{
		public AccommodationType AccommodationType { get; set; }
		public IEnumerable<AccommodationPackage> AccommodationPackages { get; set; }
		public IEnumerable<Accommodation> Accommodations { get; set; }

		// W starej wersji było int, teraz int? (nullable) — jeśli tego potrzebujesz
		public int? SelectedAccommodationPackageID { get; set; }
	}

	public class AccommodationPackageDetailsViewModel
	{
		public AccommodationPackage AccommodationPackage { get; set; }
	}

	public class CheckAccommodationAvailabilityViewModel
	{
		public DateTime FromDate { get; set; }
		public int Duration { get; set; }
		public int NoOfAdults { get; set; }
		public int NoOfChildren { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Notes { get; set; }
	}
}
