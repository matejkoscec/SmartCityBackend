using Carter;
using FluentValidation;
using MediatR;
using SmartCityBackend.Infrastructure.Service;

namespace SmartCityBackend.Features.ParkingSpot;

public record DeleteParkingSpotCommand(Guid Id, string Response) : IRequest<string>;

public class DeleteParkingSpotEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/parking-spot/delete/{id}", async (ISender sender, Guid guidId) =>
        {
            string response = await sender.Send(new DeleteParkingSpotCommand(guidId, null));
            return Results.Ok(response);
        });
    }
}

public class DeleteParkingSpotHandler : IRequestHandler<DeleteParkingSpotCommand, string>
{
    private readonly IParkingSimulationService _parkingSimulationService;

    public DeleteParkingSpotHandler(IParkingSimulationService parkingSimulationService)
    {
        _parkingSimulationService = parkingSimulationService;
    }

    public async Task<string> Handle(DeleteParkingSpotCommand request, CancellationToken cancellationToken)
    {
        return await _parkingSimulationService.DeleteParkingSpot(request.Id, cancellationToken);
    }
}