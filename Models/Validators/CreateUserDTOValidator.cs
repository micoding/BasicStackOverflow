using BasicStackOverflow.Entities;
using FluentValidation;

namespace BasicStackOverflow.Models.Validators;

public class CreateUserDTOValidator : AbstractValidator<CreateUserDTO>
{
    public CreateUserDTOValidator(BasicStackOverflowContext dbContext)
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.Password).Equal(x => x.ConfirmPassword);

        RuleFor(x => x.Username).Custom((value, context) =>
        {
            var userExists = dbContext.Users.Any(u => u.Username == value);
            if (userExists)
                context.AddFailure("Username", "Username already exists");
        });
    }
}