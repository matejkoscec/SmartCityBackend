using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Analytics;

public record GetRevenueRequest(DateTimeOffset? Start, DateTimeOffset? End, Guid ParkingSpotId) : IRequest<GetRevenueResponse>;

public record GetRevenueResponse(decimal Revenue);


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
        app.MapPost("/analytics/get-revenue", async (ISender sender, GetRevenueRequest getRevenueRequest) =>
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
        IList<ParkingSpotHistory> parkingSpotHistories = await _databaseContext.ParkingSpotsHistory
            .Where(psh => psh.ParkingSpotId == request.ParkingSpotId)
            .Include(x => x.ZonePrice)
            .OrderBy(x => x.StartTime)
            .AsNoTracking()
            .ToListAsync();

        IList<ParkingSpotHistory> parkingSpotHistoriesFiltered = new List<ParkingSpotHistory>();
        
        if (request.Start != null)
            parkingSpotHistoriesFiltered = parkingSpotHistoriesFiltered.Where(x => x.StartTime >= request.Start).ToList();
        
        
        if (request.End != null)
            parkingSpotHistoriesFiltered = parkingSpotHistoriesFiltered.Where(x => x.StartTime <= request.End).ToList();
        
        
        if(request.Start == null && request.End == null)
            parkingSpotHistoriesFiltered = parkingSpotHistories.ToList();

        if(parkingSpotHistoriesFiltered.Count == 0)
            return new GetRevenueResponse(0);

        decimal totalRevenue = 0;
        decimal totalDuration = 0;
        
        for (int i = 0; i < parkingSpotHistoriesFiltered.Count - 1; i += 2)
        {
            ParkingSpotHistory firstHistory = parkingSpotHistoriesFiltered[i];
            ParkingSpotHistory secondHistory = parkingSpotHistoriesFiltered[i + 1];
            
            
            DateTimeOffset reservationStart = firstHistory.StartTime;
            DateTimeOffset reservationEnd = secondHistory.StartTime;
            
            
            decimal price = firstHistory.ZonePrice.Price;
            
            decimal reservationDuration = (decimal) (reservationEnd - reservationStart).TotalHours;
            
            totalDuration += reservationDuration;
            totalRevenue += price * reservationDuration;
        }
        
        return new GetRevenueResponse(totalRevenue);
    }
}