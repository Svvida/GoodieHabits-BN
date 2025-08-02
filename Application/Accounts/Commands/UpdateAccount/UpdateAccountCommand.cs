using System.Text.Json.Serialization;
using MediatR;

namespace Application.Accounts.Commands.UpdateAccount
{
    public record UpdateAccountCommand(string? Login, string Email, string Nickname, string? Bio) : IRequest<Unit>
    {
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
