using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Features.Analytics.Util;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Analytics;

public record GetZoneOccupancyRequest
    (DateTimeOffset Start, DateTimeOffset End) : IRequest<GetZoneOccupancyResponse>;

public record GetZoneOccupancyResponse(List<ZoneOccupancy> ZoneOccupancy, List<DateTimeOffset> Hours);

public record ZoneOccupancy(List<decimal> ZoneOccupancies);

public sealed class GetZoneOccupancyValidator : AbstractValidator<GetZoneOccupancyRequest>
{
    public GetZoneOccupancyValidator()
    {
        RuleFor(x => (x.End - x.Start)).GreaterThan(TimeSpan.FromHours(3));
    }
}

public class GetZoneOccupancyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/analytics/get-zone-occupancy",
            async (ISender sender, GetZoneOccupancyRequest getZoneOccupancyRequest) =>
            {
                var response = await sender.Send(getZoneOccupancyRequest);
                return Results.Ok(response);
            });
    }
}

public class GetZoneOccupancyHandler : IRequestHandler<GetZoneOccupancyRequest, GetZoneOccupancyResponse>
{
    private readonly DatabaseContext _dbContext;

    public GetZoneOccupancyHandler(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetZoneOccupancyResponse> Handle(GetZoneOccupancyRequest request,
        CancellationToken cancellationToken)
    {
        var fullHours = FullHoursBetween(request.Start, request.End); 
        var zoneOccupancy = new List<ZoneOccupancy>();

        foreach (var hour in fullHours)
        {
            var zoneOccupancyList = new List<decimal>();
            foreach (ParkingZone zone in Enum.GetValues(typeof(ParkingZone)))
            {
                var zoneOccupancyPercentage = await GetZoneOccupancy(zone, hour, hour.AddHours(1));
                zoneOccupancyList.Add(zoneOccupancyPercentage);
            }

            zoneOccupancy.Add(new ZoneOccupancy(zoneOccupancyList));
        }
        
        return new GetZoneOccupancyResponse(zoneOccupancy, fullHours);
    }

    private async Task<decimal> GetZoneOccupancy(ParkingZone zone, DateTimeOffset start, DateTimeOffset end)
    {
        Console.WriteLine(zone);
        var parkingSpots = await _dbContext.ParkingSpots
            .Where(x => x.Zone == zone)
            .Include(x => x.ParkingSpotsHistory)
            .ToListAsync();
        
        if (parkingSpots.Count == 0)
            return 0;

        var zoneOccupancy = 0.0;
        foreach (var parkingSpot in parkingSpots)
        {
            var relevantParkingSpotsHistory = parkingSpot.ParkingSpotsHistory
                .Where(x => x.StartTime >= start).ToList();
            var occupiedPercentage = AnalyticsUtil.GetOccupiedPercentage(start, end,
                relevantParkingSpotsHistory);
            zoneOccupancy += (double) occupiedPercentage;
        }

        return (decimal) (zoneOccupancy / parkingSpots.Count);
    }
    
    private List<DateTimeOffset> FullHoursBetween(DateTimeOffset start, DateTimeOffset end)
    {
        var fullHours = new List<DateTimeOffset>();
        var currentHour = start;
        while (currentHour < end)
        {
            fullHours.Add(currentHour);
            currentHour = currentHour.AddHours(1);
        }

        return fullHours;
    }
}