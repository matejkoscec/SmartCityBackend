using System.Reflection;
using Carter;
using FluentMigrator.Runner;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCityBackend.Features.EventHub;
using SmartCityBackend.Infrastructure;
using SmartCityBackend.Infrastructure.Hubs;
using SmartCityBackend.Infrastructure.Hash;
using SmartCityBackend.Infrastructure.Jobs;
using SmartCityBackend.Infrastructure.JwtProvider;
using SmartCityBackend.Infrastructure.Middlewares;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.PipelineBehavior;
using SmartCityBackend.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<Program>();
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
});

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(c =>
        c.AddPostgres()
            .WithGlobalConnectionString(builder.Configuration.GetConnectionString(nameof(DatabaseContext)))
            .ScanIn(Assembly.GetExecutingAssembly())
            .For.All());

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(DatabaseContext)));
});

builder.Services.AddHttpClients(builder.Configuration);

builder.Services.AddHostedService<EventHubListener>();

builder.Services.AddQuartz(q =>
{
    var parkingSpotsJobKey = new JobKey("GetAllParkingSpotsJob");
    q.AddJob<GetAllParkingSpotsJob>(opts => opts.WithIdentity(parkingSpotsJobKey));

    q.AddTrigger(opts => opts
        .ForJob(parkingSpotsJobKey)
        .WithIdentity("GetAllParkingSpots-trigger")
        .WithCronSchedule("0 0/1 * * * ?")
    );

    var replaceReservationsJobKey = new JobKey("ReplaceReservationsJob");
    q.AddJob<ReplaceReservationsJob>(opts => opts.WithIdentity(replaceReservationsJobKey));

    q.AddTrigger(opts => opts
        .ForJob(replaceReservationsJobKey)
        .WithIdentity("ReplaceReservations-trigger")
        .WithCronSchedule("0 0/1 * * * ?")
    );
    
    var trainModelJobKey = new JobKey("TrainModelJob");
    q.AddJob<TrainModelJob>(opts => opts.WithIdentity(trainModelJobKey));
    
    q.AddTrigger(opts => opts
        .ForJob(trainModelJobKey)
        .WithIdentity("TrainModel-trigger")
        .WithCronSchedule("0 0/30 * ? * *")
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

var app = builder.Build();

using (var scope = ((IApplicationBuilder)app).ApplicationServices.CreateScope())
{
    var migrator = scope.ServiceProvider.GetService<IMigrationRunner>()!;
    migrator.MigrateUp();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ValidationMiddleware>();

app.MapCarter();

app.InitializeDb();

app.MapHub<ParkingSpotFeedHub>("/hub/parking-spot");

app.Run();