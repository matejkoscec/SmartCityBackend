using Polly;
using SmartCityBackend.Infrastructure.Service;

namespace SmartCityBackend.Infrastructure;

public static class HttpClients
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IParkingSimulationService, DefaultParkingSimulationService>(client =>
            {
                client.BaseAddress = new Uri("https://hackathon.kojikukac.com/");
                client.DefaultRequestHeaders.Add("Api-Key", configuration["Codebooq:ApiKey"]);
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(PollyConstants.RetryCount, PollyConstants.ExponentialBackoff)
            );

        return services;
    }
}