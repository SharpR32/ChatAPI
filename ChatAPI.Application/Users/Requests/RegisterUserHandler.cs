using ChatAPI.Application.Common;
using ChatAPI.Infrastructure.Services.Abstraction;
using Mediator;

namespace ChatAPI.Application.Users;
public record RegistrationData(string UserName, string Password) : BaseLoginData(UserName, Password), ICommand<Result>;
public sealed class RegisterUserHandler : ICommandHandler<RegistrationData, Result>
{
    private readonly IUserRepository _repository;

    public RegisterUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result> Handle(RegistrationData command, CancellationToken cancellationToken)
    {
        var success = await _repository.RegisterUserAsync(command.UserName, command.Password, cancellationToken);

        if (success)
            return new Result();

        return Result.FromError("error", "Wystąpił nieznany błąd");
    }
}
