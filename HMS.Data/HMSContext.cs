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
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<GroupReservation> GroupReservations { get; set; }
        public DbSet<GroupReservationRoom> GroupReservationRooms { get; set; }
        public DbSet<DishPicture> DishPictures { get; set; }


        public DbSet<AccommodationPicture> AccommodationPictures { get; set; }
        public DbSet<AccommodationPackagePicture> AccommodationPackagePictures { get; set; }



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
                .WithMany(p => p.Accommodations)
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
            modelBuilder.Entity<DishPicture>()
    .HasOne(dp => dp.Dish)
    .WithMany(d => d.DishPictures) // lub .WithMany(d => d.DishPictures) jeśli dodasz nawigację w Dish
    .HasForeignKey(dp => dp.DishID)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DishPicture>()
                .HasOne(dp => dp.Picture)
                .WithMany()
                .HasForeignKey(dp => dp.PictureID)
                .OnDelete(DeleteBehavior.Restrict);


            // 12. Unikalność w TableReservation
            modelBuilder.Entity<TableReservation>()
                .HasIndex(tr => new { tr.TableNumber, tr.ReservationDate })
                .IsUnique();

            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.RStatus)
                .HasConversion<string>()
                .IsRequired(); // o ile chcesz, żeby Status zawsze był wypełniony

            // 2) PaymentMethod zapisywany jako string
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.PaymentMethod)
                .HasConversion<string>(); // PaymentMethod? -> string

            // 3) Pola typu int – wartości domyślne, np. 0
            // (jeśli chcesz, tak jak w Reservation AdultCount jest domyślnie 1)
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.AdultCount)
                .HasDefaultValue(1);

            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.ChildrenCount)
                .HasDefaultValue(0);

            // Pola do posiłków (wszystkie int) – też 0:
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.BreakfastAdults)
                .HasDefaultValue(0);
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.BreakfastChildren)
                .HasDefaultValue(0);
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.LunchAdults)
                .HasDefaultValue(0);
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.LunchChildren)
                .HasDefaultValue(0);
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.DinnerAdults)
                .HasDefaultValue(0);
            modelBuilder.Entity<GroupReservation>()
                .Property(gr => gr.DinnerChildren)
                .HasDefaultValue(0);


            modelBuilder.Entity<GroupReservationRoom>()
        .HasOne(grr => grr.GroupReservation)
        .WithMany(gr => gr.GroupReservationRooms)
        .HasForeignKey(grr => grr.GroupReservationID)
        .OnDelete(DeleteBehavior.Cascade);

            // Relacja 1 -> wiele: Room -> GroupReservationRoom
            modelBuilder.Entity<GroupReservationRoom>()
                .HasOne(grr => grr.Room)
                .WithMany() // (chyba, że w Room mamy nawigację)
                .HasForeignKey(grr => grr.RoomID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
