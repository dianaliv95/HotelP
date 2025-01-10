using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Entities
{
	public class Accommodation
	{
		[Key]
		public int ID { get; set; }
		[Required]

		public int AccommodationPackageID { get; set; }
		[ForeignKey("AccommodationPackageID")]
		public virtual AccommodationPackage AccommodationPackage { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		public string Description { get; set; }

		public int MaxGuests { get; set; }
		public int MaxAdults { get; set; }
		public int MaxChildren { get; set; }

		public ICollection<Reservation> Reservations { get; set; }
		public List<AccommodationPicture> AccommodationPictures { get; set; }

	}
}
