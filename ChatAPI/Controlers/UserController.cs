using ChatAPI.Application.Users;
using ChatAPI.Application.Users.Requests;
using ChatAPI.Controlers.Common;

namespace ChatAPI.Controlers;

public static class UserController
{
    public static IEndpointRouteBuilder MapUserRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapPost(
            pattern: PrependController("register"),
            handler: EndpointGenerator.EndpointGen<RegistrationData>());

        builder.MapPost(
            pattern: PrependController("login"),
            handler: EndpointGenerator.EndpointGen<LoginData>());

        return builder;

        static string PrependController(string path)
            => $"/user/{path}";
    }
}
