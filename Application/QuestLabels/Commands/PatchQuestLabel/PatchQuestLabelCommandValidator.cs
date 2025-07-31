using Domain.Interfaces;
using FluentValidation;

namespace Application.QuestLabels.Commands.PatchQuestLabel
{
    public class PatchQuestLabelCommandValidator : AbstractValidator<PatchQuestLabelCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatchQuestLabelCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(cmd => cmd.PatchDto.Value)
                .MaximumLength(25).WithMessage("Value must not exceed {MaxLength} characters")
                .MustAsync(async (command, value, cancellationToken) =>
                {
                    return await _unitOfWork.QuestLabels.GetLabelByValueAsync(value, command.PatchDto.AccountId, cancellationToken) == null;
                })
                .WithMessage("A label with value '{PropertyValue}' already exists for this account.");

            RuleFor(cmd => cmd.PatchDto.BackgroundColor)
                .MaximumLength(7).WithMessage("Background Color must not exceed {MaxLength} characters");
        }
    }
}