namespace SmartCityBackend.Infrastructure.Utils;

public static class CurrentSimulationTime
{
    
    public static (int simulationHours, int simulationMinutes)? GetCurrentSimulationTime()
    {
        // Get the current real-world time
        DateTimeOffset now = DateTimeOffset.UtcNow;
        
        var simulationHours = now.Minute % 30;
        var simulationMinutes = now.Second % 30;
        
        if (simulationHours >= 24 && simulationHours < 30)
        {
            return null;
        }
        
        // Return the simulation time
        return (simulationHours, simulationMinutes);
    }



}