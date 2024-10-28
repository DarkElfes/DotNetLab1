using Bank.Shared.Transactions.Enums;

namespace Bank.Shared.Transactions.TransactionsDto;

public class TransferTransactionDto : TransactionDto
{
    public int SenderCardId { get;set; }
    public string SenderUsername { get; set; } = string.Empty;
    public int ReceiverCardId { get; set; }
    public string ReceiverUsername { get; set; } = string.Empty;
}
