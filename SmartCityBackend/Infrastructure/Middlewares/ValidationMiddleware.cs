using System.Collections.Immutable;
using FluentValidation;

namespace SmartCityBackend.Infrastructure.Middlewares;

public sealed class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            _logger.LogError("Validation failed: {Exception}", e);

            var errors = e.Errors.GroupBy(f => f.PropertyName)
                .ToImmutableDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage));

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            if (!string.IsNullOrWhiteSpace(e.Message) || errors.Any())
            {
                await context.Response.WriteAsJsonAsync(new { e.Message, errors });
            }
        }
    }
}