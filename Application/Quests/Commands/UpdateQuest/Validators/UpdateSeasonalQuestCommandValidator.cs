using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Utilities;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using NodaTime;

namespace Application.Quests.Commands.UpdateQuest.Validators
{
    public class UpdateSeasonalQuestCommandValidator : UpdateQuestCommandValidator<UpdateSeasonalQuestCommand>
    {
        public UpdateSeasonalQuestCommandValidator(IUnitOfWork unitOfWork, IClock clock) : base(unitOfWork)
        {
            RuleFor(x => x.Season)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .IsEnumName(typeof(SeasonEnum), caseSensitive: true)
                .WithMessage("{PropertyName} must be a valid season.");

            RuleFor(x => x)
                .Must(x => SeasonHelper.IsDateWithinSeason(x.StartDate, x.EndDate, Enum.Parse<SeasonEnum>(x.Season), clock.GetCurrentInstant().ToDateTimeUtc()))
                .WithMessage("StartDate and EndDate must be within the selected season.");
        }
    }
}
