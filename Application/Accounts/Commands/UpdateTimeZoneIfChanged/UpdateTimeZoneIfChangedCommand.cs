using MediatR;

namespace Application.Accounts.Commands.UpdateTimeZoneIfChanged
{
    public record UpdateTimeZoneIfChangedCommand(int AccountId, string? TimeZoneId) : IRequest<Unit>;
}
