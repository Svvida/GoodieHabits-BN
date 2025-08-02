using System.Text.Json.Serialization;
using MediatR;

namespace Application.Accounts.Commands.WipeoutData
{
    public record WipeoutDataCommand(string Password, string ConfirmPassword) : IRequest<Unit>
    {
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
