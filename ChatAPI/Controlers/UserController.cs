using ChatAPI.Application.Users;
using ChatAPI.Application.Users.Requests;
using ChatAPI.Controlers.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controlers;
using static EndpointGenerator;

public class UserController : IController
{
    private const string GROUP_NAME = "user";

    public static IEndpointRouteBuilder MapRoutes(IEndpointRouteBuilder builder)
    {
        builder.MapPost(
            pattern: PrependController("register"),
            handler: EndpointGen<RegistrationData>())
            .IncludeMetadata(GROUP_NAME);

        builder.MapPost(
            pattern: PrependController("login"),
            handler: EndpointGen<LoginData>())
            .IncludeMetadata(GROUP_NAME);

        builder.MapGet(
            pattern: PrependController("{userId}"),
            handler: (
            [FromServices] IMediator mediator,
            [FromRoute] Guid userId,
            CancellationToken cancellation)
            => InvokeAction(mediator, new UserDataQuery(userId), cancellation))
            .IncludeMetadata(GROUP_NAME)
            .RequireAuthorization();

        return builder;

        static string PrependController(string path)
            => $"api/{GROUP_NAME}/{path}";
    }
}
