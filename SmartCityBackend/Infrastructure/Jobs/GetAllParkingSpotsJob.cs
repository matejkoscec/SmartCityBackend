using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Service;
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
                existing.Lat = spotDto.Latitude;
                existing.Lng = spotDto.Longitude;
                existing.Zone = spotDto.ParkingZone;
                _dbContext.Update(existing);
            }
            else
            {
                var newSpot = new ParkingSpot
                {
                    Id = guid,
                    Lat = spotDto.Latitude,
                    Lng = spotDto.Longitude,
                    Zone = spotDto.ParkingZone
                };
                _dbContext.Add(newSpot);
                _logger.LogInformation("Added new parking spot {Guid}", newSpot.Id);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}