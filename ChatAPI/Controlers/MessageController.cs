using ChatAPI.Application.Messages;
using ChatAPI.Controlers.Common;
using ChatAPI.Policies.RateLimitting;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controlers;
using static EndpointGenerator;
public class MessageController : IController
{
    const string GROUP_NAME = "Messages";

    public static IEndpointRouteBuilder MapRoutes(IEndpointRouteBuilder builder)
    {
        builder.MapPost(
            pattern: PrependController(),
            handler: EndpointGen<SendMessage>())
            .RequireRateLimiting(SendMessagePolicy.NAME)
            .IncludeMetadata(GROUP_NAME)
            .RequireAuthorization();

        builder.MapGet(
            pattern: PrependController("{participantId}"),
            handler: (Guid participantId,
                [FromQuery] DateTimeOffset since,
                [FromServices] IMediator mediator,
                CancellationToken cancellationToken)
                    => InvokeAction(mediator, new GetMessages(participantId, since), cancellationToken))
            .IncludeMetadata(GROUP_NAME)
            .RequireAuthorization();

        return builder;
    }


    static string PrependController(string? path = null)
        => $"api/{GROUP_NAME}/{path}";
}