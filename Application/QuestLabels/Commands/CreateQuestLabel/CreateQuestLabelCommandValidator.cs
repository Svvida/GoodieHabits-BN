using Domain.Interfaces;
using FluentValidation;

namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public class CreateQuestLabelCommandValidator : AbstractValidator<CreateQuestLabelCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateQuestLabelCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(cmd => cmd.CreateDto.Value)
                .NotEmpty().WithMessage("Value is required")
                .MaximumLength(25).WithMessage("Value must not exceed {MaxLength} characters")
                .MustAsync(async (command, value, cancellationToken) =>
                {
                    return await _unitOfWork.QuestLabels.GetLabelByValueAsync(value, command.CreateDto.AccountId, cancellationToken) == null;
                })
                .WithMessage("A label with value '{PropertyValue}' already exists for this account.");

            RuleFor(cmd => cmd.CreateDto.BackgroundColor)
                .NotEmpty().WithMessage("Background Color is required")
                .MaximumLength(7).WithMessage("Background Color must not exceed {MaxLength} characters");
        }
    }
}

