using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using SmartCityBackend.Infrastructure.Hash;
using SmartCityBackend.Infrastructure.JwtProvider;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Utils;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Auth;

public sealed record RegisterRequest(string Email,
    string Password,
    string PreferredUsername,
    string GivenName,
    string FamilyName) : IRequest<RegisterResponse>;

public sealed record RegisterCommand(string Email,
    string Password,
    string PreferredUsername,
    string GivenName,
    string FamilyName,
    HttpContext Context) : IRequest<RegisterResponse>;

public sealed record RegisterResponse(string Token);

public sealed class RegisterCommandValidator : AbstractValidator<RegisterRequest>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email must not be empty");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty");
        RuleFor(x => x.PreferredUsername).NotEmpty().WithMessage("PreferredUsername must not be empty");
        RuleFor(x => x.GivenName).NotEmpty().WithMessage("GivenName must not be empty");
        RuleFor(x => x.FamilyName).NotEmpty().WithMessage("FamilyName must not be empty");
    }
}

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/register",
            async (ISender sender, RegisterRequest request, HttpContext context) =>
            {
                var response = await sender.Send(new RegisterCommand(request.Email,
                    request.Password,
                    request.PreferredUsername,
                    request.GivenName,
                    request.FamilyName,
                    context));
                return Results.Ok(response);
            });
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly DatabaseContext _databaseContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public RegisterCommandHandler(DatabaseContext databaseContext,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _databaseContext = databaseContext;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        User? user = await _databaseContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, cancellationToken);

        if (user != null)
            throw new("User with that email already exists");

        if (!EmailValidator.IsValidEmail(command.Email))
            throw new("Email is not valid");

        User newUser = new User();
        newUser.Name = $"{command.FamilyName} {command.GivenName}";
        newUser.Email = command.Email;
        newUser.Password = _passwordHasher.Hash(command.Password);
        newUser.PreferredUsername = command.PreferredUsername;
        newUser.GivenName = command.GivenName;
        newUser.FamilyName = command.FamilyName;

        Role? role = await _databaseContext.Roles.FirstOrDefaultAsync(x => x.Name == "USER", cancellationToken);

        IEnumerable<Role> roles = new[] { role! };
        newUser.Roles = roles.ToList();

        _databaseContext.Users.Add(newUser);
        await _databaseContext.SaveChangesAsync(cancellationToken);

        long generatedUserId = newUser.Id;

        RefreshToken refreshToken = new RefreshToken();
        refreshToken.UserId = generatedUserId;
        refreshToken.Token = new Guid().ToString();
        refreshToken.Expires = DateTimeOffset.UtcNow.AddDays(30);
        newUser.RefreshTokens = new List<RefreshToken> { refreshToken };

        _databaseContext.RefreshTokens.Add(refreshToken);
        await _databaseContext.SaveChangesAsync(cancellationToken);

        command.Context.Response.Cookies.Append("refreshToken",
            refreshToken.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            });

        string token = _jwtProvider.GenerateToken(newUser);
        return new(token);
    }
}