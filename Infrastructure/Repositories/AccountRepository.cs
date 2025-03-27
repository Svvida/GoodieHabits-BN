using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context) { }

        public async Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Nickname == username, cancellationToken).ConfigureAwait(false)
                ?? null;
        }

        public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email, cancellationToken).ConfigureAwait(false)
                ?? null;
        }
    }
}
