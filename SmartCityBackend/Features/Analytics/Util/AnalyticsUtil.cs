using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Analytics.Util;

public class AnalyticsUtil
{
    public static double GetOccupiedPercentage(DateTimeOffset start, DateTimeOffset end,
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
        
        var unoccupiedRecordsAfterFirstOccupied = unoccupiedRecords
            .Where(unoccupiedRecord => unoccupiedRecord.StartTime > occupiedRecords.First().StartTime)
            .ToList();
        
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
        
        return occupiedPercentage;
    }
}