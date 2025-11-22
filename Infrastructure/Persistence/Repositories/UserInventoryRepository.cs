using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;

namespace Infrastructure.Persistence.Repositories
{
    public class UserInventoryRepository(AppDbContext context) : BaseRepository<UserInventory>(context), IUserInventoryRepository
    {
    }
}
