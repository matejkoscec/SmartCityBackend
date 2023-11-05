﻿using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Auth;

public sealed record CurrentUserCommand() : IRequest<CurrentUserResponse>;

public sealed record CurrentUserResponse(User User);

public class CurrentUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/current-user",
            async (ISender sender) =>
            {
                var response = await sender.Send(new CurrentUserCommand());
                return Results.Ok(response);
            });
    }
}

public class CurrentUserHandler : IRequestHandler<CurrentUserCommand, CurrentUserResponse>
{
    private readonly ILogger<CurrentUserHandler> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IUserContextService _userContextService;

    public CurrentUserHandler(ILogger<CurrentUserHandler> logger, DatabaseContext databaseContext, IUserContextService userContextService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _userContextService = userContextService;
    }

    public async Task<CurrentUserResponse> Handle(CurrentUserCommand command, CancellationToken cancellationToken)
    {
        var userContext = _userContextService.GetUserDetails();
        var user = await _databaseContext.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == userContext.Id);
        if (user == null)
        {
            throw new("User not found");
        }
        // set userid in roles to null to avoid circular reference
        user.Password = null;
        
        foreach (var role in user.Roles)
        {
            role.Users = null;
        }
        
        return new CurrentUserResponse(user);
    }
}