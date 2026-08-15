using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;

namespace Application.Supplements.Catalog.Commands.SetSupplementActive
{
    public record SetSupplementActiveCommand(int SupplementId, bool IsActive, int UserProfileId)
        : ICommand<SupplementDto>;
}
