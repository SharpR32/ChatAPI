using ChatAPI.Application.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controlers.Common;

public static class EndpointGenerator
{
    public enum DataSource
    {
        Body, Query
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

            if (result is Unit or null)
                return Results.Ok();
            else if (result is IAppResult appResult)
                return appResult.Success
                    ? Results.Ok(appResult.GetSuccessData())
                    : Results.BadRequest(appResult.Errors);
            else
                return Results.Ok(result);
        }
        catch
        {
            IDictionary<string, string[]>? errorObject = Result.FromError("error", "Wystąpił nieznany błąd").Errors;
            return Results.BadRequest(errorObject);
        }
    }
}
