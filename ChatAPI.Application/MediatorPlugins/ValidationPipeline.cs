using ChatAPI.Application.Common;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace ChatAPI.Application.MediatorPlugins;

public sealed class ValidationPipeline<TRequest, TResult> : IPipelineBehavior<TRequest, Result<TResult>>
    where TRequest : IRequest<Result<TResult>>
    where TResult : class
{
    private readonly IValidator<TRequest>? _validator;
    public ValidationPipeline(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async ValueTask<Result<TResult>> Handle(TRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, Result<TResult>> next)
    {
        ValidationResult? validationResult = await _validator?.ValidateAsync(message, cancellationToken)!;

        if (validationResult is null || validationResult is { IsValid: true })
            return await next(message, cancellationToken);

        return new(validationResult);
    }
}
