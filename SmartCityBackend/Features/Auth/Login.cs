using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Hash;
using SmartCityBackend.Infrastructure.JwtProvider;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Infrastructure.Utils;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginCommand(string Email, string Password, HttpContext Context) : IRequest<LoginResponse>;

public sealed record LoginResponse(string Token);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email must not be empty");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty");
    }
}

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/login",
            async (ISender sender, LoginRequest request, HttpContext context) =>
            {
                var response = await sender.Send(new LoginCommand(request.Email, request.Password, context));
                return Results.Ok(response);
            });
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(ILogger<LoginCommandHandler> logger, DatabaseContext databaseContext, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        User user = await _databaseContext.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == command.Email, cancellationToken);

        if(user == null)
            throw new("User not found");
        
        if(!EmailValidator.IsValidEmail(command.Email))
            throw new("Email is not valid");
        
        if (!_passwordHasher.VerifyPassword(command.Password, user.Password))
            throw new("Invalid password");
        
        RefreshToken? refreshToken = _databaseContext.RefreshTokens.FirstOrDefault(x => x.UserId == user.Id);
        
        if (refreshToken == null)
        {
            refreshToken = new RefreshToken();
            refreshToken.UserId = user.Id;
            refreshToken.Token = new Guid().ToString();
            refreshToken.Expires = DateTimeOffset.UtcNow.AddDays(30);
            _databaseContext.RefreshTokens.Add(refreshToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);
            
            command.Context.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            });
        }

        string token = _jwtProvider.GenerateToken(user);
        
        return new LoginResponse(token);
    }
}