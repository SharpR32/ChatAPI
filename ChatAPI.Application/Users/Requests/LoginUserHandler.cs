using ChatAPI.Application.Common;
using ChatAPI.Application.Common.Services;
using ChatAPI.Infrastructure.Exceptions;
using ChatAPI.Infrastructure.Services.Abstraction;
using Mediator;
using Microsoft.Extensions.Primitives;

namespace ChatAPI.Application.Users.Requests;
using static TokenConstants;

public record LoginData(string UserName, string Password) : BaseLoginData(UserName, Password), ICommand<Result<LoginResult>>;
public record LoginResult(string Token, Guid Id, string DisplayName);
public sealed class LoginUserHandler : ICommandHandler<LoginData, Result<LoginResult>>
{
    private readonly IUserRepository _repository;
    private readonly UserData _userData;
    private readonly ITokenManager _tokenManager;

    public LoginUserHandler(IUserRepository repository, UserData userData, ITokenManager tokenManager)
    {
        _repository = repository;
        _userData = userData;
        _tokenManager = tokenManager;
    }

    public async ValueTask<Result<LoginResult>> Handle(LoginData command, CancellationToken cancellationToken)
    {
        try
        {
            var ip = _userData.Ip.MapToIPv4().ToString();
            (var success, var id) = await _repository.TryLoginAsync(
                command.UserName,
                command.Password,
                ip,
                cancellationToken);

            if (!success)
                return Result<LoginResult>.FromError("loginError", "Podane hasło jest nieprawidłowe");

            return new(new LoginResult(await _tokenManager.GenerateTokenAsync(new Dictionary<string, StringValues>()
            {
                { ID, id.ToString() },
                { DISPLAY_NAME, command.UserName }
            }), id, command.UserName));
        }
        catch (UserDoesntExistException)
        {
            return Result<LoginResult>.FromError("loginError", "Podane dane nie pasują do żadnego użytkownika");
        }
        catch
        {
            return Result<LoginResult>.FromError("error", "Wystąpił nieznany błąd");
        }


    }
}
