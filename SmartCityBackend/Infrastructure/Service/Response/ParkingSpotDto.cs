namespace SmartCityBackend.Infrastructure.Service.Response;

public record ParkingSpotDto(string? Id,
    double Latitude,
    double Longitude,
    ParkingSpotZoneDto ParkingSpotZoneDto,
    bool Occupied,
    string OccupiedTimestamp);

public enum ParkingSpotZoneDto
{
    Zone1,
    Zone2,
    Zone3,
    Zone4
}