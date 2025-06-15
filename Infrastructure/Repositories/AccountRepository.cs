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
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Login == username, cancellationToken).ConfigureAwait(false)
                ?? null;
        }

        public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email, cancellationToken).ConfigureAwait(false)
                ?? null;
        }
        public async Task<Account?> GetAccountWithProfileInfoAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Profile)
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> DoesLoginExistAsync(string login, int accountIdToExclue, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.AnyAsync(a => a.Id != accountIdToExclue && a.Login == login, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> DoesEmailExistAsync(string email, int accountIdToExclude, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.AnyAsync(a => a.Id != accountIdToExclude && a.Email == email, cancellationToken).ConfigureAwait(false);
        }
    }
}
