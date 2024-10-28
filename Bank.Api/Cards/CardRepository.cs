using Bank.Api.Accounts;
using Bank.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bank.Api.Cards;

public sealed class CardRepository(
    ILogger<CardRepository> logger,
    AppDbContext context)
{
    private readonly ILogger<CardRepository> _logger = logger;
    private readonly AppDbContext _context = context;

    public async Task<Result<Card>> GetByAccountIdAsync(string accountId)
        => await GetByExpressionAsync(c => c.Owner.Id.Equals(accountId));
    public async Task<Result<Card>> GetByCardNumberAsync(string cardNumber)
        => await GetByExpressionAsync(c => c.Id.Equals(int.Parse(cardNumber)));
    private async Task<Result<Card>> GetByExpressionAsync(Expression<Func<Card, bool>> expression)
    {
        try
        {
            Card? card = await _context.Cards
                .Include(c => c.Owner)
                .Include(c => c.TellerMachineTransactions)
                .Include(c => c.ServicesPaymentTrasnactions)
                .Include(c => c.SentTransferTransactions)
                    .ThenInclude(t => t.ReceiverCard)
                        .ThenInclude(c => c.Owner)
                .Include(c => c.ReceivedTransferTransactions)
                    .ThenInclude(t => t.SenderCard)
                        .ThenInclude(c => c.Owner)
                .FirstOrDefaultAsync(expression);

            return card is null ? Result.Fail("Card not found") : Result.Ok(card);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get card by expression: @{Condition}, caused by @{ErrorMessage}", expression, ex.Message);
            return Result.Fail("An error occured while trying get card data");
        }
    }

    public async Task<Result> UpdateAsync(Card card)
    {
        try
        {
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update card @{CardId}, caused by: @{ErrorMessage}", card, ex.Message);
        }
        return Result.Fail("An error occured while updating card data");
    }
    public async Task<int> GetLastId()
        => await _context.Cards.AnyAsync()
        ? await _context.Cards.MaxAsync(c => c.Id)
        : 1;
}
