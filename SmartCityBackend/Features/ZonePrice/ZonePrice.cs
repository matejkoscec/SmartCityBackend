using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;

namespace SmartCityBackend.Features.ZonePrice;

public sealed record ZonePriceResponse(long Id,DateTimeOffset Datetime,  decimal Price);

public sealed record ZonePriceCommand() : IRequest<IList<ZonePriceResponse>>;
public class ZonePriceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/zone-price", async (ISender sender) =>
        {
            var response = await sender.Send(new ZonePriceCommand());
            return Results.Ok(response);
        });
    }
}

public sealed class ZonePriceHandler : IRequestHandler<ZonePriceCommand, IList<ZonePriceResponse>>
{
    private readonly ILogger<ZonePriceHandler> _logger;
    private readonly DatabaseContext _databaseContext;

    public ZonePriceHandler(ILogger<ZonePriceHandler> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    public async Task<IList<ZonePriceResponse>> Handle(ZonePriceCommand command, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var zonePrices = await _databaseContext.ZonePrices.Where(x=> x.CreatedAtUtc >= now.AddDays(-30) ).ToListAsync(cancellationToken: cancellationToken);
        var zones = zonePrices.Select(x => new ZonePriceResponse(x.Id, x.CreatedAtUtc, x.Price)).ToList();

        Console.WriteLine();
        
        return zones;
    }
}