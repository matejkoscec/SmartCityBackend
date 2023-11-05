using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Service.Response;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.ParkingSpot;

public record ParkingSpotCommandFilter(
    decimal? Latitude,
    // Parking Spot Type
    decimal? Longitude,
    decimal? Radius,
    ParkingZone? ParkingZone,
    bool? isOccupied,
    decimal? price) : IRequest<List<GetParkingSpotResponse>>;

public record GetParkingSpotResponse(
    Guid Id,
    decimal Latitude,
    decimal Longitude,
    ParkingZone ParkingZone,
    bool? Occupied = null,
    decimal? price = null);

public sealed class GetParkingSpotsValidator : AbstractValidator<ParkingSpotCommandFilter>
{
    public GetParkingSpotsValidator()
    {
    }
}

public class GetParkingSpotsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/parking-spot/get-parking-spots", async (ISender sender, ParkingSpotCommandFilter parkingSpot) =>
        {
            var response = await sender.Send(parkingSpot);
            return Results.Ok(response);
        });
    }
}

public class GetParkingSpotsHandler : IRequestHandler<ParkingSpotCommandFilter, List<GetParkingSpotResponse>>
{
    private readonly DatabaseContext _databaseContext;

    public GetParkingSpotsHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<List<GetParkingSpotResponse>> Handle(ParkingSpotCommandFilter request,
        CancellationToken cancellationToken)
    {
        IQueryable<Models.ParkingSpot> queryable = _databaseContext.ParkingSpots.Include(x => x.ParkingSpotsHistory)
            .ThenInclude(x => x.ZonePrice)
            .AsNoTracking().AsQueryable();

        if (request.ParkingZone != null)
        {
            queryable = queryable.Where(c => c.Zone == request.ParkingZone);
        }

        if (request.isOccupied.HasValue)
        {
            queryable = queryable.Where(p => p.ParkingSpotsHistory.Any(h => h.IsOccupied == request.isOccupied.Value));
        }

        if (request.price.HasValue)
        {
            queryable = queryable.Where(p => p.ParkingSpotsHistory.Any(h => h.ZonePrice.Price == request.price.Value));
        }

        if (request.Latitude.HasValue && request.Longitude.HasValue && request.Radius.HasValue)
        {
            // Calculate the bounding coordinates for the given radius
            double latitude = (double)request.Latitude;
            double longitude = (double)request.Longitude;
            double radius = (double)request.Radius;

            double earthRadius = 6371; // Earth's radius in kilometers (you can adjust this if needed)

            double minLat = latitude - (radius / earthRadius) * (180.0 / Math.PI);
            double maxLat = latitude + (radius / earthRadius) * (180.0 / Math.PI);
            double minLng = longitude -
                            (radius / earthRadius) * (180.0 / Math.PI) / Math.Cos(latitude * (Math.PI / 180.0));
            double maxLng = longitude +
                            (radius / earthRadius) * (180.0 / Math.PI) / Math.Cos(latitude * (Math.PI / 180.0));

            // Filter parking spots within the bounding coordinates
            queryable = queryable.Where(p =>
                p.Lat >= (decimal)minLat &&
                p.Lat <= (decimal)maxLat &&
                p.Lng >= (decimal)minLng &&
                p.Lng <= (decimal)maxLng);
        }

        // find isOccupied information in ParkingSpotsHistory for parking spots that are in the queryable

        var filteredParkingSpots = await queryable.ToListAsync(cancellationToken);

        var response = filteredParkingSpots.Select(MapToGetParkingSpotResponse).ToList();

        return response;
    }

    private static GetParkingSpotResponse MapToGetParkingSpotResponse(Models.ParkingSpot parkingSpot)
    {
        var parkingSpotHistory = parkingSpot.ParkingSpotsHistory.MaxBy(history => history.StartTime);

        var occupied = parkingSpotHistory?.IsOccupied;
        var price = parkingSpotHistory?.ZonePrice.Price;

        return new GetParkingSpotResponse(
            parkingSpot.Id,
            parkingSpot.Lat,
            parkingSpot.Lng,
            parkingSpot.Zone,
            occupied,
            price
        );
    }

    /*IQueryable<ITCost> queryable = context.ItCosts.AsNoTracking().AsQueryable();

    queryable = queryable.Where(c => c.Deleted != true);

    if (filter.TenantId != null)
        queryable = queryable.Where(c => c.TenantId == filter.TenantId);

        if (filter.Provider != null)
        queryable = queryable.Where(c => c.Provider == filter.Provider);

        if (filter.Service != null)
        queryable = queryable.Where(c => c.Service == filter.Service);

        if (filter.From != null)
        queryable = queryable.Where(c => c.CreatedAt >= filter.From);

        if (filter.To != null)
        queryable = queryable.Where(c => c.CreatedAt <= filter.To);

        return await queryable.ToListAsync();*/
}