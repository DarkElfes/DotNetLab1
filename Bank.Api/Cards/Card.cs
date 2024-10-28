using Bank.Api.Accounts;
using Bank.Api.Transactions.TransactionModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.Api.Cards;

public class Card
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public Account Owner { get; set; } = null!;
    public List<TellerMachineTransaction> TellerMachineTransactions { get; set; } = [];
    public List<ServicesPaymentTransaction> ServicesPaymentTrasnactions { get; set; } = [];
    public List<TransferTransaction> SentTransferTransactions { get; set; } = [];
    public List<TransferTransaction> ReceivedTransferTransactions { get; set; } = [];
    

    [NotMapped] public string Number => Id.ToString().PadLeft(16, '0');

}
