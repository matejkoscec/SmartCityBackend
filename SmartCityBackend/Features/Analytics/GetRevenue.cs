using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Features.Analytics.Util;
using SmartCityBackend.Infrastructure.Persistence;

namespace SmartCityBackend.Features.Analytics;

public record GetRevenueRequest
    (DateTimeOffset Start, DateTimeOffset End, Guid ParkingSpotId) : IRequest<GetRevenueResponse>;

public record GetRevenueResponse(decimal Revenue);

// TODO revenue per periods (for example per hour) so it can be easily displayed on a graph

public sealed class GetRevenueValidator : AbstractValidator<GetRevenueRequest>
{
    public GetRevenueValidator()
    {
        RuleFor(x => x.ParkingSpotId).NotEmpty();
    }
}

public class GetRevenueEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/analytics/get-revenue", async (ISender sender, GetRevenueRequest getRevenueRequest) =>
        {
            var response = await sender.Send(getRevenueRequest);
            return Results.Ok(response);
        });
    }
}

public class GetRevenueHandler : IRequestHandler<GetRevenueRequest, GetRevenueResponse>
{
    private readonly DatabaseContext _databaseContext;

    public GetRevenueHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<GetRevenueResponse> Handle(GetRevenueRequest request, CancellationToken cancellationToken)
    {
        var parkingSpotHistories = await _databaseContext.ParkingSpotsHistory
            .Where(psh => psh.ParkingSpotId == request.ParkingSpotId)
            .Include(x => x.ZonePrice)
            .OrderBy(x => x.StartTime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        var totalRevenue = AnalyticsUtil.GetRevenueForParkingSpot(request.Start, request.End, parkingSpotHistories);

        return new GetRevenueResponse(totalRevenue);
    }
}