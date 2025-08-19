using FluentValidation;

namespace Application.Common.ValidatorsExtensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MinimumLength(6).WithMessage("{PropertyName} must be at least {MinLength} characters long.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Matches("^[a-zA-Z0-9_#@!-]*$")
                .WithMessage("{PropertyName} must contain only letters, numbers, and the following special characters: _ @ # -");
        }
    }
}
