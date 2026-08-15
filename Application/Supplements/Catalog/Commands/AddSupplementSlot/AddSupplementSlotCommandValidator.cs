using Domain.Models;
using FluentValidation;

namespace Application.Supplements.Catalog.Commands.AddSupplementSlot
{
    public class AddSupplementSlotCommandValidator : AbstractValidator<AddSupplementSlotCommand>
    {
        public AddSupplementSlotCommandValidator()
        {
            RuleFor(c => c.SupplementId)
                .GreaterThan(0).WithMessage("SupplementId must be greater than 0.");

            RuleFor(c => c.Timing)
                .IsInEnum().WithMessage("Timing is invalid.");

            RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(c => c.OffsetMinutes)
                .InclusiveBetween(SupplementScheduleSlot.MinOffsetMinutes, SupplementScheduleSlot.MaxOffsetMinutes)
                .When(c => c.OffsetMinutes.HasValue)
                .WithMessage($"OffsetMinutes must be between {SupplementScheduleSlot.MinOffsetMinutes} and {SupplementScheduleSlot.MaxOffsetMinutes}.");

            RuleFor(c => c.Note)
                .MaximumLength(SupplementScheduleSlot.NoteMaxLength).When(c => c.Note is not null)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
