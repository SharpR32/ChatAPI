using ChatAPI.Infrastructure.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;

namespace ChatAPI.Middlewares;

public class TokenRequirement : IAuthorizationRequirement { }
public class TokenRequirementHandler(ICurrentUser manager) : AuthorizationHandler<TokenRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenRequirement requirement)
    {
        if (!manager.Initiated)
        {
            context.Fail();
        }
        return Task.CompletedTask;
    }
}
