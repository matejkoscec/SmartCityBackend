using Carter;
using MediatR;
using SmartCityBackend.Features.DummyDomain.Response;

namespace SmartCityBackend.Features.DummyDomain;

public record GetWeatherForecastQuery : IRequest<IEnumerable<WeatherForecastResponse>>;

public class GetWeatherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/weather",
            async (ISender sender) =>
            {
                var response = await sender.Send(new GetWeatherForecastQuery());
                return Results.Ok(response);
            });
    }
}

public sealed class
    GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecastResponse>>
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<GetWeatherForecastQueryHandler> _logger;

    public GetWeatherForecastQueryHandler(ILogger<GetWeatherForecastQueryHandler> logger) { _logger = logger; }

    public Task<IEnumerable<WeatherForecastResponse>> Handle(GetWeatherForecastQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Some logging");

        var responses = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecastResponse
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        return Task.FromResult<IEnumerable<WeatherForecastResponse>>(responses);
    }
}