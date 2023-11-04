using SmartCityBackend.Infrastructure.Service.Response;

namespace SmartCityBackend.Infrastructure.Service;

public interface IParkingSimulationService
{
    Task<List<ParkingSpotDto>> GetAllParkingSpots(CancellationToken cancellationToken);
}