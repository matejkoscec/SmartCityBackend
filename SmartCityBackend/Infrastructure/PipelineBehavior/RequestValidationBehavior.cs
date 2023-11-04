using FluentValidation;
using MediatR;

namespace SmartCityBackend.Infrastructure.PipelineBehavior;

public sealed class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var validationContext = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var errors = validationResults.Where(v => v.Errors.Any()).SelectMany(v => v.Errors).ToList();
        if (errors.Any())
        {
            throw new ValidationException("One or more validation errors occurred", errors);
        }

        return await next();
    }
}