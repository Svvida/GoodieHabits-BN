using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AccountRepository(AppDbContext context) : BaseRepository<Account>(context), IAccountRepository
    {
        public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email, cancellationToken).ConfigureAwait(false)
                ?? null;
        }
        public async Task<Account?> GetAccountWithProfileAsync(int accountId, CancellationToken cancellationToken = default)
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

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            return !await _context.Accounts.AnyAsync(a => a.Email == email, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Account?> GetByLoginIdentifier(string loginIdentifier, CancellationToken cancellationToken = default)
        {
            if (loginIdentifier.Contains('@'))
            {
                return await _context.Accounts
                    .AsNoTracking()
                    .Include(a => a.Profile)
                    .FirstOrDefaultAsync(a => a.Email == loginIdentifier, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await _context.Accounts
                    .AsNoTracking()
                    .Include(a => a.Profile)
                    .FirstOrDefaultAsync(a => a.Login == loginIdentifier, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}