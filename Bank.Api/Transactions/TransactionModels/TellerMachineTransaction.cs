using Bank.Api.Cards;

namespace Bank.Api.Transactions.TransactionModels;

public class TellerMachineTransaction : Transaction
{
    public Card Card { get; set; } = null!;
    public string MachineName { get; set; } = default!;
}
