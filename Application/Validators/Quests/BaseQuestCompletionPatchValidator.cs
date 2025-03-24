using Application.Dtos.Quests;
using FluentValidation;

namespace Application.Validators.Quests
{
    public class BaseQuestCompletionPatchValidator<T> : AbstractValidator<T> where T : BaseQuestCompletionPatchDto
    {
        public BaseQuestCompletionPatchValidator()
        {
            RuleFor(x => x.IsCompleted)
                .Must(x => x == false || true)
                .WithMessage("{PropertName} must be either 'false' or 'true'");
        }
    }
}
