namespace HMS.Entities
{
    public class GroupReservationRoom
    {
        public int ID { get; set; }

        public int GroupReservationID { get; set; }
        public virtual GroupReservation GroupReservation { get; set; }

        public int RoomID { get; set; }
        public virtual Room Room { get; set; }
    }
}
