using ChatAPI.Infrastructure.Services.Abstraction;
using FluentValidation;

namespace ChatAPI.Application.Messages;

public class SendMessageValidator : AbstractValidator<SendMessage>
{
    private readonly IUserRepository _repository;

    public SendMessageValidator(IUserRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Content)
            .MaximumLength(2000)
            .NotEmpty();

        RuleFor(x => x.ReceiverId)
            .MustAsync(CheckReceiver)
            .WithMessage("User does not exist");
    }


    private async Task<bool> CheckReceiver(Guid receiverId, CancellationToken cancellationToken)
        => !await _repository.UserExistsAsync(receiverId, cancellationToken);
}
