// using MediatR;
//
// namespace SmartCityBackend.Features.Analytics;
//
// public record GetZoneOccupancyRequest
//     (DateTimeOffset Start, DateTimeOffset End, Guid ParkingSpotId) : IRequest<GetZoneOccupancyResponse>;
//
// public record GetZoneOccupancyResponse(List<ZoneOccupancy> ZoneOccupancy);
//
// public record ZoneOccupancy
//
// public class GetZoneOccupancy
// {
//     
// }