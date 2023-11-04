using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.EventHub;

public class ParkingSpotEvent
{
    public Guid Id { get; set; }
    public bool IsOccupied { get; set; }
    public string Time { get; set; } = null!;
}

public class EventHubListener : IHostedService
{
    private readonly EventHubConsumerClient _consumerClient;
    private readonly DatabaseContext _dbContext;
    private readonly string _partitionId;

    public EventHubListener(IConfiguration configuration, IServiceScopeFactory iServiceScopeFactory)
    {
        var eventHubConnectionString = configuration.GetSection("EventHub:ConnectionString").Value!;
        var eventHubName = configuration.GetSection("EventHub.Name").Value!;
        _consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName,
            eventHubConnectionString, eventHubName);
        _dbContext = iServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
        _partitionId = "0";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // get offset sorted by descending date
        var offset = await _dbContext.EventHubInfo
            .OrderByDescending(e => e.CreatedAtUtc)
            .Select(e => e.Offset)
            .FirstOrDefaultAsync(cancellationToken);

        await StartProcessingEvents(offset, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumerClient.CloseAsync(cancellationToken);
    }

    private async Task StartProcessingEvents(long offset, CancellationToken cancellationToken)
    {
        var eventPosition = EventPosition.FromOffset(offset);

        await foreach (PartitionEvent partitionEvent in _consumerClient.ReadEventsFromPartitionAsync(_partitionId,
                           eventPosition, cancellationToken))
        {
            await PersistOffset(partitionEvent.Data.Offset);
            // Console.WriteLine(Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray()));
            // Console.WriteLine("Enqueued at: " +
            //                   partitionEvent.Partition.ReadLastEnqueuedEventProperties().EnqueuedTime);
            // Console.WriteLine(partitionEvent.Data.Offset);
            
            
            string eventJson = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
            var parkingSpotEvent = JsonSerializer.Deserialize<ParkingSpotEvent>(eventJson)!;
            
            Console.WriteLine(eventJson);
            
            Console.WriteLine(parkingSpotEvent.Id);
            
            // var parkingSpot 
            //     await _dbContext.ParkingSpots.SingleOrDefaultAsync(x => x.Id == parkingSpotEvent.Id, cancellationToken);
            //
            // if (parkingSpot is null)
            // {
            //     Console.WriteLine("Parking spot not found");
            //     continue;
            // }
            //
            // var zonePrice = await _dbContext.ZonePrices
            //     .OrderByDescending(e => e.CreatedAtUtc)
            //     .FirstOrDefaultAsync(cancellationToken);
            //
            // var time = DateTimeOffset.Parse(parkingSpotEvent.Time).ToUniversalTime();
            // var activeReservation =
            //     await _dbContext.ActiveReservations.SingleOrDefaultAsync(x => x.ParkingSpotId == parkingSpotEvent.Id);
            //
            // var parkingSpotHistory = new ParkingSpotHistory
            // {
            //     IsOccupied = parkingSpotEvent.IsOccupied,
            //     StartTime = time,
            //     ParkingSpotId = parkingSpot!.Id,
            //     ParkingSpot = parkingSpot,
            //     ActiveReservationId = activeReservation?.Id ?? null,
            //     ActiveReservation = activeReservation,
            //     ZonePriceId = zonePrice!.Id,
            //     ZonePrice = zonePrice
            // };
            //
            // _dbContext.ParkingSpotsHistory.Add(parkingSpotHistory);
            //
            // await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task PersistOffset(long offset)
    {
        _dbContext.EventHubInfo.Add(new EventHubInfo { Offset = offset });
        await _dbContext.SaveChangesAsync();
    }
}