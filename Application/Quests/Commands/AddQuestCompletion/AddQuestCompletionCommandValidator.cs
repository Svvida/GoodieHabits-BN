using Application.Common.ValidatorsExtensions;
using Domain.Interfaces;
using Domain.Models;
using FluentValidation;

namespace Application.Quests.Commands.AddQuestCompletion
{
    public class AddQuestCompletionCommandValidator : AbstractValidator<AddQuestCompletionCommand>
    {
        public AddQuestCompletionCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId).QuestMustBeOwnedByCurrentUser(unitOfWork);

            RuleFor(x => x.Amount)
                .GreaterThan(0).When(x => x.Amount.HasValue)
                .WithMessage("{PropertyName} must be greater than zero.");

            RuleFor(x => x.Note)
                .MaximumLength(QuestCompletion.NoteMaxLength)
                .WithMessage($"{{PropertyName}} cannot exceed {QuestCompletion.NoteMaxLength} characters.");

            // How far back is a rule about the quest's own periods, so the entity enforces it; this only
            // catches the obviously absurd before a database round trip.
            RuleFor(x => x.CompletedOn)
                .GreaterThan(DateOnly.MinValue).When(x => x.CompletedOn.HasValue)
                .WithMessage("{PropertyName} must be a real date.");
        }
    }
}
