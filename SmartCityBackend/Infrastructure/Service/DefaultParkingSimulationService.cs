using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartCityBackend.Features.ParkingSpot;
using SmartCityBackend.Features.Reservation;
using SmartCityBackend.Infrastructure.Service.Response;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Service;

public sealed record ParkingSpotRequest(decimal Latitude, decimal Longitude, string ParkingSpotZone);

public sealed record ParkingSpotRes(
    string Id,
    decimal Latitude,
    decimal Longitude,
    string parkingSpotZone,
    bool Occupied,
    string OccupiedTimestamp);

public sealed record CreateReservationRequest(string ParkingSpotId, int EndH, int EndM);

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
        var contentString = await result.Content.ReadAsStringAsync(cancellationToken);
        
        // convert content string to list of ParkingSpotRes
        
        var response = JsonSerializer.Deserialize<List<ParkingSpotRes>>(contentString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        // List<ParkingSpotRes> response = await ParseResponse<List<ParkingSpotRes>>(result, cancellationToken);
        
        List<ParkingSpotDto> parkingSpotDtos = new List<ParkingSpotDto>();
        foreach (var parkingSpotResponse in response)
        {
            parkingSpotDtos.Add(MapToDto(parkingSpotResponse));
        }
        
        
        _logger.LogInformation("Received {Spots} parking spots.", response.Count);

        return parkingSpotDtos;
    }

    public async Task<ParkingSpotResponse> CreateParkingSpot(ParkingSpotCommand parkingSpot,
        CancellationToken cancellationToken)
    {
        ParkingSpotRequest parkingSpotRequest = new ParkingSpotRequest(parkingSpot.Latitude,
            parkingSpot.Longitude,
            parkingSpot.ParkingZone.ToString());
        var result = await _httpClient.PostAsync("api/ParkingSpot",
            SerializeToJsonContent(parkingSpotRequest),
            cancellationToken);
        result.EnsureSuccessStatusCode();

        var deserializedResponse = await ParseResponse<ParkingSpotResponse>(result, cancellationToken);
        _logger.LogInformation(
            $"Created parking spot with lat: {parkingSpot.Latitude} and lng: {parkingSpot.Longitude}");

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
        CreateReservationRequest request = new CreateReservationRequest(
            createReservationCommand.ParkingSpotId.ToString(),
            createReservationCommand.EndHour,
            createReservationCommand.EndMinute);
        var result = await _httpClient.PostAsync("api/ParkingSpot/reserve",
            SerializeToJsonContent(request),
            cancellationToken);
        result.EnsureSuccessStatusCode();

        _logger.LogInformation(
            $"Created reservation for parking spot with id: {createReservationCommand.ParkingSpotId}");

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

    private ParkingSpotDto MapToDto(ParkingSpotRes response)
    {
        ParkingZone parkingZone = new ParkingZone();

        switch (response.parkingSpotZone)
        {
            case "Zone1":
                parkingZone = ParkingZone.Zone1;
                break;
            case "Zone2":
                parkingZone = ParkingZone.Zone2;
                break;
            case "Zone3":
                parkingZone = ParkingZone.Zone3;
                break;
            case "Zone4":
                parkingZone = ParkingZone.Zone4;
                break;
            default:
                parkingZone = ParkingZone.Zone1;
                break;
        }
        
        ParkingSpotDto dto = new ParkingSpotDto(
            response.Id,
            response.Latitude,
            response.Longitude,
            parkingZone,
            response.Occupied,
            response.OccupiedTimestamp);

        return dto;
    }
}

public class ParkingZoneConverter : JsonConverter<ParkingZone>
{
    public override ParkingZone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (Enum.TryParse(reader.GetString(), true, out ParkingZone result))
            {
                return result;
            }
        }
        return ParkingZone.Zone1; // Default value if parsing fails.
    }

    public override void Write(Utf8JsonWriter writer, ParkingZone value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
