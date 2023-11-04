using backend_template.Features.DummyDomain.Response;
using Carter;
using FluentValidation;
using MediatR;

namespace backend_template.Features.DummyDomain;

public record GetWeatherForecastQuery(int End) : IRequest<IEnumerable<WeatherForecastResponse>>;

public class GetWeatherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/weather",
            async (ISender sender, int end) =>
            {
                var response = await sender.Send(new GetWeatherForecastQuery(end));
                return Results.Ok(response);
            });
    }
}

public class GetWeatherForecastQueryValidator : AbstractValidator<GetWeatherForecastQuery>
{
    public GetWeatherForecastQueryValidator() { RuleFor(x => x.End).InclusiveBetween(2, 5); }
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

        var responses = Enumerable.Range(1, request.End)
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