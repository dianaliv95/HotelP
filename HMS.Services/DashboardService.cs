using HMS.Data;
using HMS.Entities;
using System.Collections.Generic;
using System.Linq;

namespace HMS.Services
{
    public class DashboardService
    {
        private readonly HMSContext _context;

        public DashboardService(HMSContext context)
        {
            _context = context;
        }

        public bool SavePicture(Picture picture)
        {
            _context.Pictures.Add(picture);
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<Picture> GetPicturesByIDs(List<int> pictureIDs)
        {
            return _context.Pictures
                           .Where(p => pictureIDs.Contains(p.ID))
                           .ToList();
        }
    }
}
