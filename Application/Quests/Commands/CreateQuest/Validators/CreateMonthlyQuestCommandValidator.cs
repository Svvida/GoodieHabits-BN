using Application.Quests.Commands.CreateQuest;
using Domain.Interfaces;
using FluentValidation;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest.Validators
{
    public class CreateMonthlyQuestCommandValidator : CreateQuestCommandValidator<CreateMonthlyQuestCommand>
    {
        public CreateMonthlyQuestCommandValidator(IUnitOfWork unitOfWork, IClock clock) : base(unitOfWork, clock)
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
