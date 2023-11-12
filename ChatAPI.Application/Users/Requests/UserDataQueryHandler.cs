using ChatAPI.Application.Common;
using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Services.Abstraction;
using Mediator;

namespace ChatAPI.Application.Users.Requests;

public sealed record UserDataQuery(Guid UserId) : IRequest<Result<UserDataModel>>;
public sealed class UserDataQueryHandler : IRequestHandler<UserDataQuery, Result<UserDataModel>>
{
    private readonly IUserRepository _repository;

    public UserDataQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<UserDataModel>> Handle(UserDataQuery request, CancellationToken cancellationToken)
    {
        User result = await _repository.GetUserMetadataAsync(request.UserId, cancellationToken);
        return Result.FromData(new UserDataModel(result.DisplayName!));
    }
}

public sealed record UserDataModel(string DisplayName);
