using Bank.Api.Cards;

namespace Bank.Api.Transactions.TransactionModels;

public class TransferTransaction : Transaction
{
    public Card SenderCard { get; set; } = null!;
    public Card ReceiverCard { get; set; } = null!;
}
