using Application.Dtos.Quests.SeasonalQuest;
using Application.Helpers;
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

            RuleFor(x => x)
                .Must(x => SeasonHelper.IsDateWithinSeason(x.StartDate, x.EndDate, Enum.Parse<SeasonEnum>(x.Season)))
                .WithMessage("StartDate and EndDate must be within the selected season.");
        }
    }
}
