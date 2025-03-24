using Application.Dtos.Quests.SeasonalQuest;
using Application.Helpers;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.Quests.SeasonalQuest
{
    public class PatchSeasonalQuestValidator : BasePatchQuestValidator<PatchSeasonalQuestDto>
    {
        public PatchSeasonalQuestValidator()
        {
            RuleFor(x => x.Season)
                .IsEnumName(typeof(SeasonEnum), caseSensitive: true)
                .WithMessage("{PropertyName} must be a valid season.");

            RuleFor(x => x)
                .Must(x => SeasonHelper.IsDateWithinSeason(x.StartDate, x.EndDate, Enum.Parse<SeasonEnum>(x.Season)))
                .When(x => !string.IsNullOrEmpty(x.Season))
                .WithMessage("StartDate and EndDate must be within the selected season.");
        }
    }
}
