using Bank.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Bank.Api.Accounts;

public record AccountRepository(
    ILogger<AccountRepository> _logger,
    AppDbContext _context)
{
    public async Task<Result<Account?>> GetByIdAsync(string id)
    {
        try
        {
            return Result.Ok(await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id.Equals(id)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account by id @{Id}", id);
        }
        return Result.Fail("An error ocurred while obtaining account by id");
    }
    public async Task<Result<Account?>> GetByEmailAsync(string email)
    {
        try
        {
            return Result.Ok(await _context.Accounts
               .FirstOrDefaultAsync(a => a.Email.Equals(email)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account by email: @{Email}", email);
        }
        return Result.Fail("An error occurred while obtaining account by email");
    }


    public async Task<Result> AddAsync(Account account)
    {
        try
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add account @{Account} in to database", account);
            return Result.Fail("An error occurred while adding account");
        }
    }

    public async Task<Result> UpdateAsync(Account account)
    {
        try
        {
            _context.Update(account);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update account @{Account}, caused by @{ErrorMessage}", account, ex.Message);
        }
        return Result.Fail("An error occurred while updating account");
    }


}
