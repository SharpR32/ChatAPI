using ChatAPI.Application.Messages;
using ChatAPI.Controlers.Common;
using ChatAPI.Policies.RateLimitting;

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
            .IncludeMetadata(GROUP_NAME);

        builder.MapGet(
            pattern: PrependController("{participantId}"),
            handler: EndpointGen<SendMessage>(DataSource.Query));

        return builder;
    }


    static string PrependController(string? path = null)
        => $"/{GROUP_NAME}/{path}";
}