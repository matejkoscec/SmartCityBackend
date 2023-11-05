using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Analytics.Util;

public class AnalyticsUtil
{
    public static decimal GetRevenueForParkingSpot(DateTimeOffset start, DateTimeOffset end,
        List<ParkingSpotHistory> parkingSpotHistories)
    {
        var totalMinutes = GetOccupiedMinutes(start, end, parkingSpotHistories);
        var zonePrice = parkingSpotHistories.First().ZonePrice.Price;

        return (totalMinutes * zonePrice / 60);
    }

    public static decimal GetOccupiedPercentage(DateTimeOffset start, DateTimeOffset end,
        List<ParkingSpotHistory> parkingSpotHistories)
    {
        var occupiedRecords = parkingSpotHistories
            .Where(history => history.IsOccupied)
            .OrderBy(history => history.StartTime)
            .ToList();

        var unoccupiedRecords = parkingSpotHistories
            .Where(history => !history.IsOccupied)
            .OrderBy(history => history.StartTime)
            .ToList();

        
        if (occupiedRecords.Count == 0 || unoccupiedRecords.Count == 0)
        {
            return 0;
        }
        
        var unoccupiedRecordsAfterFirstOccupied = unoccupiedRecords
            .Where(unoccupiedRecord => unoccupiedRecord.StartTime > occupiedRecords.First().StartTime)
            .ToList();

        if (unoccupiedRecordsAfterFirstOccupied.Count == 0)
        {
            return 0;   
        }
        

        var totalOccupiedTime = 0.0;

        foreach (var unoccupiedRecord in unoccupiedRecordsAfterFirstOccupied)
        {
            var occupiedRecord = occupiedRecords
                .FirstOrDefault(occupiedRecord => occupiedRecord.StartTime > unoccupiedRecord.StartTime);

            if (occupiedRecord == null)
                break;

            var timeDifference = occupiedRecord.StartTime - unoccupiedRecord.StartTime;

            totalOccupiedTime += timeDifference.TotalMinutes;
        }

        var totalOccupiedTimeSpan = end - start;
        var totalOccupiedTimeSpanMinutes = totalOccupiedTimeSpan.TotalMinutes;

        var occupiedPercentage = totalOccupiedTime / totalOccupiedTimeSpanMinutes;

        return (decimal)occupiedPercentage;
    }

    public static decimal GetOccupiedMinutes(DateTimeOffset start, DateTimeOffset end,
        List<ParkingSpotHistory> parkingSpotHistories)
    {
        var percentage = GetOccupiedPercentage(start, end, parkingSpotHistories);
        var totalMinutes = (decimal)(end - start).TotalMinutes;
        var occupiedMinutes = percentage * totalMinutes;
        return occupiedMinutes;
    }
}