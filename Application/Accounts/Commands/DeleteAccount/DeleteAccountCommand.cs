using System.Text.Json.Serialization;
using MediatR;

namespace Application.Accounts.Commands.DeleteAccount
{
    public record DeleteAccountCommand(string Password, string ConfirmPassword) : IRequest<Unit>
    {
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
