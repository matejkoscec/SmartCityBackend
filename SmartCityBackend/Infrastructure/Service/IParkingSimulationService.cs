using SmartCityBackend.Features.ParkingSpot;
using SmartCityBackend.Features.Reservation;
using SmartCityBackend.Infrastructure.Service.Response;

namespace SmartCityBackend.Infrastructure.Service;

public interface IParkingSimulationService
{
    Task<List<ParkingSpotDto>> GetAllParkingSpots(CancellationToken cancellationToken);
    Task<ParkingSpotResponse> CreateParkingSpot(ParkingSpotCommand parkingSpot, CancellationToken cancellationToken);
    Task<string> DeleteParkingSpot(Guid id, CancellationToken cancellationToken);

    Task<string> CreateReservation(CreateReservationCommand createReservationCommand, CancellationToken cancellationToken);
}