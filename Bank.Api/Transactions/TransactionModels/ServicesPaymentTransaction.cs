using Bank.Api.Cards;

namespace Bank.Api.Transactions.TransactionModels;

public class ServicesPaymentTransaction : Transaction
{
    public Card Card { get; set; } = null!;
    public string ServiceName { get; set; } = default!;
}
