using MediatR;

namespace Application.Accounts.Commands.UploadAvatar
{
    public record UploadAvatarCommand(int UserProfileId, Stream FileStream, string FileName) : IRequest<UploadAvatarResponse>;
}
