using System.Text;
using System.Text.Json;
using SmartCityBackend.Features.ParkingSpot;
using SmartCityBackend.Features.Reservation;
using SmartCityBackend.Infrastructure.Service.Response;

namespace SmartCityBackend.Infrastructure.Service;

public sealed record ParkingSpotRequest(decimal latitude, decimal longitude, string parkingSpotZone);
public sealed record CreateReservationRequest(string parkingSpotId, int endH, int endM);

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
    
    public async Task<ParkingSpotResponse> CreateParkingSpot(ParkingSpotCommand parkingSpot, CancellationToken cancellationToken)
    {
        ParkingSpotRequest parkingSpotRequest = new ParkingSpotRequest(parkingSpot.Latitude, parkingSpot.Longitude, parkingSpot.ParkingZone.ToString());
        var result = await _httpClient.PostAsync("api/ParkingSpot", SerializeToJsonContent(parkingSpotRequest), cancellationToken);
        result.EnsureSuccessStatusCode();
        
        string responseContent = await result.Content.ReadAsStringAsync();
        var deserializedResponse = JsonSerializer.Deserialize<ParkingSpotResponse>(responseContent);
        _logger.LogInformation($"Created parking spot with lat: {parkingSpot.Latitude} and lng: {parkingSpot.Longitude}");
        
        return deserializedResponse;
    }
    
    public async Task<string> DeleteParkingSpot(Guid id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"api/ParkingSpot/{id}", cancellationToken);
        result.EnsureSuccessStatusCode();
        
        _logger.LogInformation($"Deleted parking spot with id: {id}");

        return $"Deleted parking spot with id: {id}";
    }

    public async Task<string> CreateReservation(CreateReservationCommand createReservationCommand,
        CancellationToken cancellationToken)
    {
        
        CreateReservationRequest request = new CreateReservationRequest(createReservationCommand.ParkingSpotId.ToString(), createReservationCommand.DurationInHours, createReservationCommand.DurationInMinutes);
        var result = await _httpClient.PostAsync("api/ParkingSpot/reserve", SerializeToJsonContent(request), cancellationToken);
        result.EnsureSuccessStatusCode();
        
        _logger.LogInformation($"Created reservation for parking spot with id: {createReservationCommand.ParkingSpotId}");
        
        return $"Created reservation for parking spot with id: {createReservationCommand.ParkingSpotId}";
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