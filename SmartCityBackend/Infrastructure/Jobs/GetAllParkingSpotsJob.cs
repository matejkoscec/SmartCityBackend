using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Service;
using SmartCityBackend.Infrastructure.Utils;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Jobs;

public class GetAllParkingSpotsJob : IJob
{
    private readonly ILogger<GetAllParkingSpotsJob> _logger;
    private readonly IParkingSimulationService _simulationService;
    private readonly DatabaseContext _dbContext;

    public GetAllParkingSpotsJob(IParkingSimulationService simulationService,
        DatabaseContext dbContext,
        ILogger<GetAllParkingSpotsJob> logger)
    {
        _simulationService = simulationService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var allParkingSpots = await _simulationService.GetAllParkingSpots(context.CancellationToken);
    
        var existingParkingSpots = await _dbContext.ParkingSpots.ToListAsync(context.CancellationToken);
        var existingMappedById = existingParkingSpots.GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.First());

        var updated = 0;
        foreach (var spotDto in allParkingSpots)
        {
            var parsedGuid = Guid.TryParse(spotDto.Id, out var guid);
            if (!parsedGuid)
            {
                _logger.LogInformation("Failed to parse Guid: {Guid}", spotDto.Id);
                continue;
            }

            var exists = existingMappedById.TryGetValue(guid, out var existing);
            if (exists && existing is not null)
            {
                if (existing.Lat == spotDto.Latitude && existing.Lng == spotDto.Longitude)
                {
                    continue;
                }

                existing.Lat = spotDto.Latitude;
                existing.Lng = spotDto.Longitude;
                existing.Zone = spotDto.ParkingSpotZone;
                updated++;
            }
            else
            {
                var parkingSpotStartingHistory = new ParkingSpotHistory()
                {
                    IsOccupied = spotDto.Occupied,
                    ParkingSpotId = guid,
                    // TODO fix this, take OccupiedTimestamp
                    StartTime = CurrentSimulationTime.GetCurrentSimulationTime() ?? DateTimeOffset.Now.ToUniversalTime(),
                    ActiveReservationId = null,
                    ReservationHistoryId = null,
                    ZonePriceId = spotDto.ParkingSpotZone switch
                    {
                        ParkingZone.Zone1 => 1,
                        ParkingZone.Zone2 => 2,
                        ParkingZone.Zone3 => 3,
                        ParkingZone.Zone4 => 4,
                        _ => 1
                    }
                };
                
                var newSpot = new ParkingSpot
                {
                    Id = guid,
                    Lat = spotDto.Latitude,
                    Lng = spotDto.Longitude,
                    Zone = spotDto.ParkingSpotZone
                };
                
                _dbContext.ParkingSpotsHistory.Add(parkingSpotStartingHistory);
                _dbContext.Add(newSpot);
                _logger.LogInformation("Added new parking spot {Guid}", newSpot.Id);
                updated++;
            }
        }


        await _dbContext.SaveChangesAsync();
        
    }
}