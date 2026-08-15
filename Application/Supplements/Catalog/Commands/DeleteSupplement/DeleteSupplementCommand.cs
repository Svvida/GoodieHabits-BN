using Application.Common.Interfaces;

namespace Application.Supplements.Catalog.Commands.DeleteSupplement
{
    public record DeleteSupplementCommand(int SupplementId, int UserProfileId) : ICommand;
}
