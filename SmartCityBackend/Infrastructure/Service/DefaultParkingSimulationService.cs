using System.Text;
using System.Text.Json;
using SmartCityBackend.Infrastructure.Service.Response;

namespace SmartCityBackend.Infrastructure.Service;

public class DefaultParkingSimulationService : IParkingSimulationService
{
    private readonly ILogger<DefaultParkingSimulationService> _logger;
    private readonly HttpClient _httpClient;

    public DefaultParkingSimulationService(ILogger<DefaultParkingSimulationService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<List<ParkingSpotDto>> GetAllParkingSpots(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync("api/ParkingSpot/getAll", cancellationToken);
        result.EnsureSuccessStatusCode();

        var response = await ParseResponse<List<ParkingSpotDto>>(result, cancellationToken);
        _logger.LogInformation("Received {Spots} parking spots.", response.Count);

        return response;
    }

    private static HttpContent SerializeToJsonContent(object o)
    {
        var jsonContent = JsonSerializer.Serialize(o, Json.DefaultSerializerOptions);

        return new StringContent(jsonContent, Encoding.UTF8, "application/json");
    }

    private async Task<T> ParseResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken))!;
    }
}