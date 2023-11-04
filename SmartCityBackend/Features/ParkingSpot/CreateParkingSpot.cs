using Carter;
using FluentValidation;
using MediatR;
using SmartCityBackend.Infrastructure.Service;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.ParkingSpot;

public sealed record ParkingSpotCommand(decimal Latitude, decimal Longitude, ParkingZone ParkingZone) : IRequest<ParkingSpotResponse>;

public sealed record ParkingSpotResponse(string Id,
    double? Latitude,
    double? Longitude,
    string? ParkingSpotZone,
    bool? Occupied,
    DateTimeOffset? OccupiedTimestamp);

public sealed class CreateParkingSpotCommandValidator : AbstractValidator<ParkingSpotCommand>
{
    public CreateParkingSpotCommandValidator()
    {
        RuleFor(x => x.Latitude).NotEmpty().WithMessage("Latitude must not be empty");
        RuleFor(x => x.Longitude).NotEmpty().WithMessage("Longitude must not be empty");
        RuleFor(x => x.ParkingZone).NotEmpty().WithMessage("ParkingZone must not be empty");
        RuleFor(x => x.Latitude).NotEmpty().WithMessage("Lat must not be empty").InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).NotEmpty().WithMessage("Lng must not be empty").InclusiveBetween(-180, 180);

    }
}
public class CreateParkingSpotEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/parking-spot/create",async (ISender sender, ParkingSpotCommand parkingSpot) =>
            {
                var response = await sender.Send(parkingSpot);
                return Results.Ok(response);
            });
        
    }
}

public class ParkingSpotHandler : IRequestHandler<ParkingSpotCommand, ParkingSpotResponse>
{   
    private readonly IParkingSimulationService _parkingSimulationService;

    public ParkingSpotHandler(IParkingSimulationService parkingSimulationService)
    {
        _parkingSimulationService = parkingSimulationService;
    }

    public async Task<ParkingSpotResponse> Handle(ParkingSpotCommand request, CancellationToken cancellationToken)
    {
        return await _parkingSimulationService.CreateParkingSpot(request, cancellationToken);
    }
    
}