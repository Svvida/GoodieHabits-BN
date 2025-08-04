using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.CreateQuest.Validators
{
    public class CreateMonthlyQuestCommandValidator : CreateQuestCommandValidator<CreateMonthlyQuestCommand>
    {
        public CreateMonthlyQuestCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            RuleFor(x => x.StartDay)
                .NotNull()
                .WithMessage("{PropertyName} is required")
                .InclusiveBetween(1, 31)
                .WithMessage("{PropertyName} must be between 1 and 31");

            RuleFor(x => x.EndDay)
                .NotNull()
                .WithMessage("{PropertyName} is required")
                .InclusiveBetween(1, 31)
                .WithMessage("{PropertyName} must be between 1 and 31")
                .GreaterThanOrEqualTo(x => x.StartDay)
                .WithMessage("{PropertyName} must be greater than or equal to {ComparisonProperty}");
        }
    }
}
