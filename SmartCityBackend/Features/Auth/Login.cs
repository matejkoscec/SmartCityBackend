using Carter;
using FluentValidation;
using MediatR;

namespace SmartCityBackend.Features.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginCommand(string Email, string Password, HttpContext Context) : IRequest<LoginResponse>;

public sealed record LoginResponse;

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
        app.MapPost("/login",
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

    public LoginCommandHandler(ILogger<LoginCommandHandler> logger) { _logger = logger; }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        command.Context.Session.SetString("Username", command.Email);

        return new LoginResponse();
    }
}