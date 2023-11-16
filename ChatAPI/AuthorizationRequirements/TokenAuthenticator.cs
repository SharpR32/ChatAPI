using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CurrentUser.Abstractions;
using ChatAPI.Infrastructure.Services.TokenHandler.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace ChatAPI.Middlewares;

public class TokenAuthenticatedSchema : AuthenticationSchemeOptions { }
public partial class TokenAuthenticator(
    ICurrentUserProvider userProvider,
    ITokenManager tokenManager,
    IOptionsMonitor<TokenAuthenticatedSchema> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    ILogger<TokenAuthenticatedSchema> logger) : AuthenticationHandler<TokenAuthenticatedSchema>(options, loggerFactory, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var values))
        {
            return AuthenticateResult.NoResult();
        }

        var token = GetTokenRegex()
            .Match(values!)
            .Groups
            .GetValueOrDefault("Token")
            ?.Value ?? string.Empty;

        try
        {
            var data = await tokenManager.ValidateTokenAsync(token);
            userProvider.SetUserData(data);

            var ticket = GenerateTicket(data);
            return AuthenticateResult.Success(ticket);
        }
        catch (InvalidTokenException)
        {
            return AuthenticateResult.Fail("Invalid token");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while authenticating token");
            return AuthenticateResult.Fail("Unexpected error occured while processing token");
        }
    }

    [GeneratedRegex("Bearer (?<Token>.+)")]
    private static partial Regex GetTokenRegex();
    private AuthenticationTicket GenerateTicket(IReadOnlyDictionary<string, string[]> data)
    {
        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, data.GetValueOrDefault(TokenConstants.ID)?.FirstOrDefault()!),
                    new(ClaimTypes.Name, data.GetValueOrDefault(TokenConstants.DISPLAY_NAME)?.FirstOrDefault()!),
                    new(ClaimTypes.Authentication, true.ToString())
                };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Principal.GenericPrincipal(identity, null);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return ticket;
    }
}
