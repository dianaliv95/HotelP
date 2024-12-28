namespace Hotel.Areas.Dashboard.ViewModels
{
    
        public class UpdateRoomStatusModel
        {
            public int RoomId { get; set; }
            public string Status { get; set; }
            public DateTime? AvailableFrom { get; set; }
            public DateTime? AvailableTo { get; set; }
        }

    }


