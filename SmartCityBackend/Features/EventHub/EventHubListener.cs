using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure;
using SmartCityBackend.Infrastructure.Hubs;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Utils;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.EventHub;

public class ParkingSpotEvent
{
    public Guid Id { get; set; }
    public bool IsOccupied { get; set; }
    public string Time { get; set; } = null!;
}

public class EventHubListener : BackgroundService
{
    private readonly ILogger<EventHubListener> _logger;
    private readonly EventHubConsumerClient _consumerClient;
    private readonly DatabaseContext _dbContext;
    private readonly IHubContext<ParkingSpotFeedHub, ILocationClient> _hubContext;
    private readonly string _partitionId;

    public EventHubListener(IConfiguration configuration,
        IServiceScopeFactory iServiceScopeFactory,
        ILogger<EventHubListener> logger,
        IHubContext<ParkingSpotFeedHub, ILocationClient> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
        var eventHubConnectionString = configuration.GetSection("EventHub:ConnectionString").Value!;
        var eventHubName = configuration.GetSection("EventHub.Name").Value!;
        _consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName,
            eventHubConnectionString,
            eventHubName);
        _dbContext = iServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
        _partitionId = "0";
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumerClient.CloseAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventHubInfo = await GetEventHubInfo(stoppingToken);

        var eventPosition = EventPosition.FromOffset(eventHubInfo.Offset);

        await foreach (PartitionEvent partitionEvent in _consumerClient.ReadEventsFromPartitionAsync(_partitionId,
                           eventPosition,
                           stoppingToken))
        {
            eventHubInfo.Offset = partitionEvent.Data.Offset;
            _dbContext.EventHubInfo.Update(eventHubInfo);

            var eventJson = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
            var parkingSpotEvent = JsonSerializer.Deserialize<ParkingSpotEvent>(eventJson)!;
            
            var parkingSpot =
                await _dbContext.ParkingSpots.SingleOrDefaultAsync(x => x.Id == parkingSpotEvent.Id, stoppingToken);

            if (parkingSpot is null)
            {
                _logger.LogInformation("Parking spot with id {Id} not found", parkingSpotEvent.Id);
                continue;
            }

            var zonePrice = await _dbContext.ZonePrices
                .OrderByDescending(e => e.CreatedAtUtc)
                .FirstOrDefaultAsync(stoppingToken);

            var time = DateTimeOffset.Parse(parkingSpotEvent.Time).ToUniversalTime();
            var activeReservation =
                await _dbContext.ActiveReservations.SingleOrDefaultAsync(x => x.ParkingSpotId == parkingSpotEvent.Id, stoppingToken);

            var parkingSpotHistory = new ParkingSpotHistory
            {
                IsOccupied = parkingSpotEvent.IsOccupied,
                StartTime = time,
                ParkingSpotId = parkingSpot!.Id,
                ParkingSpot = parkingSpot,
                ActiveReservationId = activeReservation?.Id ?? null,
                ActiveReservation = activeReservation,
                ZonePriceId = zonePrice!.Id,
                ZonePrice = zonePrice
            };

            _dbContext.ParkingSpotsHistory.Add(parkingSpotHistory);

            var message = JsonSerializer.Serialize(parkingSpotHistory, Json.DefaultSerializerOptions);
            _hubContext.Clients.All.ReceiveMessage(message);

            await _dbContext.SaveChangesAsync(stoppingToken);
        }
    }
    
    private async Task<EventHubInfo> GetEventHubInfo(CancellationToken cancellationToken)
    {
        var offset = await _dbContext.EventHubInfo.FirstOrDefaultAsync(cancellationToken);
        if (offset is not null)
        {
            return offset;
        }

        offset = new EventHubInfo { Offset = 0 };
        _dbContext.EventHubInfo.Add(offset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return offset;
    }
}