using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Entities
{
    public class Payment
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ReservationID { get; set; }

        [ForeignKey("ReservationID")]
        public Reservation Reservation { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }
    }
}
