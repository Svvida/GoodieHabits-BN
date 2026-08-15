using Application.Workouts.Sessions.Common;
using FluentValidation;

namespace Application.Workouts.Sessions.Commands.UpdateSet
{
    public class UpdateSetCommandValidator : AbstractValidator<UpdateSetCommand>
    {
        public UpdateSetCommandValidator()
        {
            RuleFor(c => c.SessionId).GreaterThan(0).WithMessage("SessionId must be greater than 0.");
            RuleFor(c => c.EntryId).GreaterThan(0).WithMessage("EntryId must be greater than 0.");
            RuleFor(c => c.SetId).GreaterThan(0).WithMessage("SetId must be greater than 0.");
            RuleFor(c => c.Set).NotNull().WithMessage("Set is required.");

            RuleFor(c => c.Set)
                .SetValidator(new SessionSetInputValidator()!)
                .When(c => c.Set is not null);
        }
    }
}
