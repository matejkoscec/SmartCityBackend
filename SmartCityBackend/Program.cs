using System.Reflection;
using Carter;
using FluentMigrator.Runner;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCityBackend.Features.EventHub;
using SmartCityBackend.Infrastructure;
using SmartCityBackend.Infrastructure.Jobs;
using SmartCityBackend.Infrastructure.Middlewares;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.PipelineBehavior;

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
    var jobKey = new JobKey("GetAllParkingSpotsJob");
    q.AddJob<GetAllParkingSpotsJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("GetAllParkingSpots-trigger")
        .WithCronSchedule("0 0/1 * * * ?")
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


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

app.Run();