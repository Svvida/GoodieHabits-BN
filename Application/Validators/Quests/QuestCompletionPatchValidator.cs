using Application.Dtos.Quests;
using FluentValidation;

namespace Application.Validators.Quests
{
    public class QuestCompletionPatchValidator<T> : AbstractValidator<T> where T : QuestCompletionPatchDto
    {
        public QuestCompletionPatchValidator()
        {
            RuleFor(x => x.IsCompleted)
                .Must(x => x == false || true)
                .WithMessage("{PropertName} must be either 'false' or 'true'");
        }
    }
}
