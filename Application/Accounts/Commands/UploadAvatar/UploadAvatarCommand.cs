using MediatR;

namespace Application.Accounts.Commands.UploadAvatar
{
    public record UploadAvatarCommand(int AccountId, Stream FileStream, string FileName) : IRequest<UploadAvatarResponse>;
}
