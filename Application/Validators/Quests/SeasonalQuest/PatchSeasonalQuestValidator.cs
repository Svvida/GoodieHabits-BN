using Application.Dtos.Quests.SeasonalQuest;
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
        }
    }
}
