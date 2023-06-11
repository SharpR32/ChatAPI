using FluentValidation;

namespace ChatAPI.Application.Users.Validators;

public sealed class RegistrationDataValidator : AbstractValidator<RegistrationData>
{
    public RegistrationDataValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .NotNull()
            .MinimumLength(7)
            .MaximumLength(25);

        RuleFor(x => x.Password)
            .NotEmpty()
            .NotNull()
            .MinimumLength(8);
    }
}
