using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Features.Analytics.Util;
using SmartCityBackend.Infrastructure.Persistence;

namespace SmartCityBackend.Features.Analytics;

public record GetPopularRequest
    (DateTimeOffset Start, DateTimeOffset End, int NoOfParkingSpots) : IRequest<GetPopularResponse>;

public record GetPopularResponse(Dictionary<Guid, double> ParkingSpotsOccupation);

public sealed class GetPopularValidator : AbstractValidator<GetPopularRequest>
{
    public GetPopularValidator()
    {
        RuleFor(x => x.NoOfParkingSpots).GreaterThan(0);
        RuleFor(x => (x.End - x.Start)).GreaterThan(TimeSpan.FromHours(1));
    }
}

public class GetPopularEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/analytics/get-popular", async (ISender sender, GetPopularRequest getPopularRequest) =>
        {
            var response = await sender.Send(getPopularRequest);
            return Results.Ok(response);
        });
    }
}

public class GetPopularHandler : IRequestHandler<GetPopularRequest, GetPopularResponse>
{
    private readonly DatabaseContext _dbContext;

    public GetPopularHandler(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetPopularResponse> Handle(GetPopularRequest request, CancellationToken cancellationToken)
    {
        var parkingSpots =
            await _dbContext.ParkingSpots.Include(x => x.ParkingSpotsHistory).ToListAsync(cancellationToken);
        
        var spotOccupationDictionary = new Dictionary<Guid, double>();
        foreach (var parkingSpot in parkingSpots)
        {
            var relevantParkingSpotsHistory = parkingSpot.ParkingSpotsHistory
                .Where(x => x.StartTime >= request.Start).ToList();
            var occupiedPercentage = AnalyticsUtil.GetOccupiedPercentage(request.Start, request.End,
                relevantParkingSpotsHistory);
            spotOccupationDictionary.Add(parkingSpot.Id, occupiedPercentage);
        }
        
        var sortedDictionary = spotOccupationDictionary.OrderByDescending(x => x.Value).Take(request.NoOfParkingSpots)
            .ToDictionary(x => x.Key, x => x.Value);
        
        return new GetPopularResponse(sortedDictionary);
    }
}