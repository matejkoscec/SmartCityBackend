namespace SmartCityBackend.Infrastructure.Utils;

public static class CurrentSimulationTime
{
    
    public static DateTimeOffset? GetCurrentSimulationTime()
    {
        // Get the current real-world time
        DateTimeOffset now = DateTimeOffset.UtcNow;
        
        var simulationHours = now.Minute % 30;
        var simulationMinutes = now.Second % 30;
        
        if (simulationHours >= 24 && simulationHours < 30)
        {
            return null;
        }
        
        DateTimeOffset simulationDateTimeOffset = new DateTimeOffset(now.Year, now.Month, now.Day, simulationHours, simulationMinutes, 0, now.Offset);

        // Return the simulation time
        return simulationDateTimeOffset;
    }



}