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
            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {request.UserProfileId} not found.");

            var publicId = await cloudinaryPhotoService.UploadPhotoAsync(request.FileStream, request.FileName, request.UserProfileId);

            userProfile.Avatar = publicId;

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new UploadAvatarResponse(urlBuilder.BuildProfilePageAvatarUrl(publicId));
        }
    }
}
