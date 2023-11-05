using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features;

public record GetUserReservationsRequest(long id) : IRequest<GetUserReservationsResponse>;

public record GetUserReservationsResponse(IEnumerable<ReservationHistory> Reservations);


public class GetUserReservationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/user/reservations/{id}", async (ISender sender, int id) =>
        {
            var response = await sender.Send(new GetUserReservationsRequest(id));
            return Results.Ok(response);
        });
    }
}

public class GetUserReservationsHandler : IRequestHandler<GetUserReservationsRequest, GetUserReservationsResponse>
{
    private readonly DatabaseContext _databaseContext;

    public GetUserReservationsHandler(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<GetUserReservationsResponse> Handle(GetUserReservationsRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _databaseContext.Users.Include(x => x.ReservationsHistory)
            .ThenInclude(x => x.ParkingSpot)
            .ThenInclude(x => x.ParkingSpotsHistory)
            .ThenInclude(x => x.ZonePrice)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.id, cancellationToken);
        
        if (user == null)
            throw new("User not found");

        return new GetUserReservationsResponse(user.ReservationsHistory);
    }
}