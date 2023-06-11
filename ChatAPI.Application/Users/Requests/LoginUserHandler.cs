using ChatAPI.Application.Common;
using ChatAPI.Application.Common.Services;
using ChatAPI.Infrastructure.Exceptions;
using ChatAPI.Infrastructure.Services;
using Mediator;
using Microsoft.Extensions.Primitives;

namespace ChatAPI.Application.Users.Requests;

public record LoginData(string UserName, string Password) : BaseLoginData(UserName, Password), ICommand<Result<string>>;
public sealed class LoginUserHandler : ICommandHandler<LoginData, Result<string>>
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

    public async ValueTask<Result<string>> Handle(LoginData command, CancellationToken cancellationToken)
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
                return Result<string>.FromError("loginError", "Podane hasło jest nieprawidłowe");

            return new(_tokenManager.GenerateToken(new Dictionary<string, StringValues>()
            {
                {"id", id.ToString() }
            }));
        }
        catch (UserDoesntExistException)
        {
            return Result<string>.FromError("loginError", "Podane dane nie pasują do żadnego użytkownika");
        }
        catch
        {
            return Result<string>.FromError("error", "Wystąpił nieznany błąd");
        }


    }
}
