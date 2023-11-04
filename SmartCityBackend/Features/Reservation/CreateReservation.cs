using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Service;

namespace SmartCityBackend.Features.Reservation;

public sealed record CreateReservationCommand(
    Guid ParkingSpotId,
    int DurationInHours,
    int DurationInMinutes) : IRequest<string>;
    

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.ParkingSpotId).NotEmpty().WithMessage("ParkingSpotId must not be empty");
        RuleFor(x => x.DurationInHours).NotEmpty().WithMessage("DurationInHours must not be empty");
        RuleFor(x => x.DurationInMinutes).NotEmpty().WithMessage("DurationInMinutes must not be empty");
        
        
    }
}

public class CreateReservationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reservation/create",async (ISender sender, CreateReservationCommand reservation) =>
            {
                var response = await sender.Send(reservation);
                return Results.Ok(response);
            });
        
    }
}

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, string>
{   
    private readonly IParkingSimulationService _parkingSimulationService;
    private readonly DatabaseContext _databaseContext; 

    public CreateReservationHandler(IParkingSimulationService parkingSimulationService, DatabaseContext databaseContext)
    {
        _parkingSimulationService = parkingSimulationService;
        _databaseContext = databaseContext;
    }

    public async Task<string> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        /*Models.ParkingSpot? parkingSpot = await _databaseContext.ParkingSpots.SingleOrDefaultAsync(x => x.Id == request.ParkingSpotId, cancellationToken);
        
        if(parkingSpot == null)
            throw new("ParkingSpotId does not exist");*/
        return await _parkingSimulationService.CreateReservation(request, cancellationToken);
    }
    
}