using Application.Supplements.Common;
using Domain.Models;
using FluentValidation;

namespace Application.Supplements.Catalog.Commands.UpdateSupplement
{
    public class UpdateSupplementCommandValidator : AbstractValidator<UpdateSupplementCommand>
    {
        public UpdateSupplementCommandValidator()
        {
            RuleFor(c => c.SupplementId)
                .GreaterThan(0).WithMessage("SupplementId must be greater than 0.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(Supplement.NameMaxLength)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.Unit)
                .IsInEnum().WithMessage("Unit is invalid.");

            RuleFor(c => c.DefaultAmount)
                .GreaterThan(0).When(c => c.DefaultAmount.HasValue)
                .WithMessage("DefaultAmount must be greater than 0.");

            RuleFor(c => c.Note)
                .MaximumLength(Supplement.NoteMaxLength).When(c => c.Note is not null)
                .WithMessage("Note must not exceed {MaxLength} characters.");

            RuleFor(c => c.Color)
                .Must(SupplementValidationRules.BeValidHexColor).When(c => c.Color is not null)
                .WithMessage("Color must be a valid hex color code (e.g. #RRGGBB).");

            RuleFor(c => c.Icon)
                .MaximumLength(Supplement.IconMaxLength).When(c => c.Icon is not null)
                .WithMessage("Icon must not exceed {MaxLength} characters.");
        }
    }
}
