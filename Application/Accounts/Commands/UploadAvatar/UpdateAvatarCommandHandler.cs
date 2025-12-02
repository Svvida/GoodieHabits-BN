using Application.Common.Interfaces;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Accounts.Commands.UploadAvatar
{
    public class UpdateAvatarCommandHandler(
        IUnitOfWork unitOfWork,
        IPhotoService cloudinaryPhotoService,
        IUrlBuilder urlBuilder) : IRequestHandler<UploadAvatarCommand, UploadAvatarResponse>
    {
        public async Task<UploadAvatarResponse> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetUserProfileForAvatarUploadAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {request.UserProfileId} not found.");

            var publicId = await cloudinaryPhotoService.UploadPhotoAsync(request.FileStream, request.FileName, request.UserProfileId);

            userProfile.UploadAvatar(publicId);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new UploadAvatarResponse(urlBuilder.BuildProfilePageAvatarUrl(publicId));
        }
    }
}
