using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.JwtProvider;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Auth;

public sealed record RefreshCommand(HttpContext Context) : IRequest<RefreshResponse>;

public sealed record RefreshResponse(string Token);

public class RefreshEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/refresh",
            async (ISender sender, HttpContext context) =>
            {
                var response = await sender.Send(new RefreshCommand(context));
                return Results.Ok(response);
            });
    }
}

public sealed class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResponse>
{
    private readonly ILogger<RefreshCommandHandler> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IJwtProvider _jwtProvider;

    public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, DatabaseContext databaseContext, IJwtProvider jwtProvider)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _jwtProvider = jwtProvider;
    }

    public async Task<RefreshResponse> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var refreshToken = command.Context.Request.Cookies["refresh_token"];

        if (refreshToken == null)
        {
            throw new("Refresh token is missing");
        }

        var refresh = _databaseContext.RefreshTokens.Include(x => x.User).FirstOrDefault(x => x.Token == refreshToken);
        if (refresh == null)
        {
            throw new("Refresh token is invalid");
        }
        
        // if token expired
        if (refresh.Expires < DateTimeOffset.UtcNow)
        {
            // return unathorized
            throw new("Refresh token is expired");
        }
        
        User? user = _databaseContext.Users.FirstOrDefault(x => x.Id == refresh.UserId);
        
        if (user == null)
        {
            throw new("User not found");
        }
        await _databaseContext.SaveChangesAsync(cancellationToken);

        var token = _jwtProvider.GenerateToken(user);
        
        return new RefreshResponse(token);
    }
}