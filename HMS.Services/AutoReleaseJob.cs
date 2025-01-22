using System;
using System.Threading.Tasks;
using HMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class AutoReleaseJob
{
    private readonly HMSContext _context;
    private readonly ILogger<AutoReleaseJob> _logger;

    public AutoReleaseJob(HMSContext context, ILogger<AutoReleaseJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("AutoReleaseJob: start skanowania pokoi do zwolnienia.");
        var today = DateTime.Today;

        var endedReservations = await _context.GroupReservations
            .Include(gr => gr.GroupReservationRooms)
                .ThenInclude(rr => rr.Room)
            .Where(gr => gr.ToDate <= today)
            .ToListAsync();

        foreach (var gr in endedReservations)
        {
            foreach (var rr in gr.GroupReservationRooms)
            {
                if (rr.Room != null && rr.Room.Status == "Reserved")
                {
                    rr.Room.Status = "Available";
                    _context.Rooms.Update(rr.Room);
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("AutoReleaseJob: zakończono zwalnianie pokoi.");
    }
}
