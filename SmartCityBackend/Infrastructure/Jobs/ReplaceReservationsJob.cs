using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Utils;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Jobs;

public class ReplaceReservationsJob : IJob
{
    private readonly ILogger<ReplaceReservationsJob> _logger;
    private readonly DatabaseContext _dbContext;

    public ReplaceReservationsJob(ILogger<ReplaceReservationsJob> logger, DatabaseContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? simulationDateTimeOffset = CurrentSimulationTime.GetCurrentSimulationTime();
        
        if (simulationDateTimeOffset == null)
            return;
        
        var lastFifteenDays = simulationDateTimeOffset?.AddDays(-15);
        IList<ActiveReservation> activeReservations = await _dbContext.ActiveReservations.Where(ar=>ar.End >= lastFifteenDays).Include(ar => ar.ParkingSpotsHistory).ToListAsync();
        
        if(activeReservations.Count == 0)
            return;

        foreach (var activeReservation in activeReservations)
        {
            if(activeReservation.End < now)
                continue;
            
            _dbContext.ActiveReservations.Remove(activeReservation);
            
            var entry = _dbContext.Entry(activeReservation);
    
            if (entry.State == EntityState.Deleted)
            {
                ReservationHistory reservationHistory = new ReservationHistory();
            
                reservationHistory.Id = activeReservation.Id;
                reservationHistory.Start = activeReservation.Start;
                reservationHistory.End = activeReservation.End;
                reservationHistory.ParkingSpotId = activeReservation.ParkingSpotId;
                reservationHistory.ParkingSpot = activeReservation.ParkingSpot;
                reservationHistory.UserId = activeReservation.UserId;
                reservationHistory.ParkingSpotsHistory = activeReservation.ParkingSpotsHistory;
                
                _dbContext.ReservationHistory.Add(reservationHistory);
            }
            
        }

        await _dbContext.SaveChangesAsync();
    }
}