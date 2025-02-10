using Application.Dtos.SeasonalQuest;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.Quests.SeasonalQuest
{
    public class CreateSeasonalQuestValidator : BaseCreateQuestValidator<CreateSeasonalQuestDto>
    {
        public CreateSeasonalQuestValidator()
        {
            RuleFor(x => x.Season)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .IsEnumName(typeof(SeasonEnum), caseSensitive: true)
                .WithMessage("{PropertyName} must be a valid season.");
        }
    }
}
