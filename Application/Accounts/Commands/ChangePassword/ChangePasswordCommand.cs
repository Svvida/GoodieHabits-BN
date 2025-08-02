using System.Text.Json.Serialization;
using MediatR;

namespace Application.Accounts.Commands.ChangePassword
{
    public record ChangePasswordCommand(string OldPassword, string NewPassword, string ConfirmNewPassword) : IRequest<Unit>
    {
        [JsonIgnore]
        public int AccountId
        {
            get; set;
        }
    }
}