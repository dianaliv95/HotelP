using HMS.Entities;

namespace Hotel.Areas.Dashboard.ViewModels
{
    
        public class ManageRoomsViewModel
        {
            public Accommodation Accommodation { get; set; }
            public IEnumerable<Room> Rooms { get; set; }
        }

    }
