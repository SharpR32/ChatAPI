using ChatAPI.Application.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controlers.Common;

public static class EndpointGenerator
{
    public enum DataSource
    {
        Body, Query, Path
    }
    public static Delegate EndpointGen<T>(DataSource dataSource = DataSource.Body) where T : IMessage
    {
        return dataSource switch
        {
            DataSource.Body => BodyInvocation,
            DataSource.Query => QueryInvocation,
            _ => throw new NotImplementedException()
        };

        static ValueTask<IResult> QueryInvocation(
            [FromServices] IMediator mediator,
            [FromQuery] T request,
            CancellationToken cancellation)
            => InvokeAction(mediator, request, cancellation);

        static ValueTask<IResult> BodyInvocation(
            [FromServices] IMediator mediator,
            [FromBody] T request,
            CancellationToken cancellation)
            => InvokeAction(mediator, request, cancellation);
    }

    public static async ValueTask<IResult> InvokeAction<T>(IMediator mediator, T request, CancellationToken cancellationToken) where T : IMessage
    {
        try
        {
            var result = await mediator.Send(request);

            return result switch
            {
                null => Results.NotFound(),
                Unit or IAppResult { Success: true, Empty: true } => Results.Ok(),
                IAppResult appResult => appResult.Success
                    ? Results.Ok(appResult.GetSuccessData())
                    : Results.BadRequest(appResult.Errors),
                _ => Results.Ok(result)
            };
        }
        catch
        {
            IDictionary<string, string[]>? errorObject = Result.FromError("error", "Wystąpił nieznany błąd").Errors;
            return Results.BadRequest(errorObject);
        }
    }

    public static IEndpointConventionBuilder IncludeMetadata(
        this IEndpointConventionBuilder builder,
        string controllerName,
        string? summary = null!,
        string? description = null!)
    {
        return builder.WithTags(controllerName)
            .WithSummary(summary ?? string.Empty)
            .WithDescription(description ?? string.Empty);
    }
}

