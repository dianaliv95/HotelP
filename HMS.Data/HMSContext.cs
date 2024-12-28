using HMS.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HMS.Data
{
    public class HMSContext : IdentityDbContext<User>
    {
        public HMSContext(DbContextOptions<HMSContext> options) : base(options) { }

        // DbSet dla encji
        public DbSet<AccommodationType> AccommodationTypes { get; set; }
        public DbSet<AccommodationPackage> AccommodationPackages { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TableReservation> TableReservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ReservationDates> ReservationDates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Konfiguracje dla AccommodationPackage
            modelBuilder.Entity<AccommodationPackage>()
                .Property(ap => ap.FeePerNight)
                .HasColumnType("decimal(18,2)");

            // 2. Relacje: Accommodation -> AccommodationPackage
            modelBuilder.Entity<Accommodation>()
                .HasOne(a => a.AccommodationPackage)
                .WithMany()
                .HasForeignKey(a => a.AccommodationPackageID)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Relacje: AccommodationPackage -> AccommodationType
            modelBuilder.Entity<AccommodationPackage>()
                .HasOne(ap => ap.AccommodationType)
                .WithMany()
                .HasForeignKey(ap => ap.AccommodationTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Relacje: Room -> Reservations (jeden pokój może mieć wiele rezerwacji)
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Reservations)
                .WithOne(res => res.Room)
                .HasForeignKey(res => res.RoomID)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Relacje: Reservation -> Accommodation (opcjonalnie)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Accommodation)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AccommodationID)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Konfiguracja właściwości typu wyliczeniowego (enum)
            //    - Status (ReservationStatus)
            //    - PaymentMethod
            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>()    // zapis w DB jako string
                .IsRequired();

            modelBuilder.Entity<Reservation>()
                .Property(r => r.PaymentMethod)
                .HasConversion<string>();   // zapis w DB jako string (nullable)

            // 7. Dodanie konfiguracji pola ReservationNumber (opcjonalnie)
            //    - np. ograniczenie długości (jeśli chcesz)
            modelBuilder.Entity<Reservation>()
                .Property(r => r.ReservationNumber)
                .HasMaxLength(50)           // ograniczenie długości do 50 znaków
                .IsUnicode(false);          // np. jeśli nie potrzebujesz znaków diakrytycznych
                                            // .IsRequired();           // jeśli chcesz, żeby zawsze istniało

            // 8. Relacja Reservation -> Payment (1:1)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Payment>(p => p.ReservationID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Reservation>()
    .Property(r => r.AdultCount)
    .IsRequired()
    .HasDefaultValue(1);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.ChildrenCount)
                .HasDefaultValue(0);


            // 9. Konfiguracja Payment
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasConversion<string>();

            // 10. Wymagalność kolumny MaxGuests w Accommodation
            modelBuilder.Entity<Accommodation>()
                .Property(a => a.MaxGuests)
                .IsRequired();


            // 11. Relacja Dish -> Category (Cascade)
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Category)
                .WithMany(c => c.Dishes)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            // 12. Unikalność w TableReservation
            modelBuilder.Entity<TableReservation>()
                .HasIndex(tr => new { tr.TableNumber, tr.ReservationDate })
                .IsUnique();
        }
    }
}
