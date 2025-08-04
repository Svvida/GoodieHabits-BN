using Application.Helpers;
using Domain.Enum;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.UpdateQuest.Validators
{
    public class UpdateSeasonalQuestCommandValidator : UpdateQuestCommandValidator<UpdateSeasonalQuestCommand>
    {
        public UpdateSeasonalQuestCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
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
