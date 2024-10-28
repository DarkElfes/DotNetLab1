using Bank.Api.Data;
using Bank.Api.Transactions.TransactionModels;
using Microsoft.EntityFrameworkCore;

namespace Bank.Api.Transactions;

public sealed class TransactionRepository(
    AppDbContext context,
    ILogger<TransactionRepository> logger)
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<TransactionRepository> _logger = logger;

    //public async Task<Result<List<Transaction>>> GetAllByAccount(string id)
    //{
    //    try
    //    {
    //        var transactions = await _context.Transactions
    //            .Include(t => t.OwnerCard)
    //            .Include(t => t.MemberCard)
    //            .Where(t => t.OwnerCard.Owner.Id.Equals(id) || t.MemberCard.Owner.Id.Equals(id))
    //           .ToListAsync();

    //        return Result.Ok(transactions);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError("Failed to get transactions of account: @{AccountId}, caused by @{ErrorMessage}", id, ex.Message);
    //        return Result.Fail("Failed to get all transaction");
    //    }
    //}


    public async Task<Result> AddAsync(Transaction transaction)
    {
        try
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to add transaction in to database: @{Trasaction}, caused by @{ErrorMessage}", transaction, ex.Message);
            return Result.Fail("An error occured while adding transaction");
        }
    }

}
