using Bank.Shared.Transactions.Enums;

namespace Bank.Shared.Transactions.TransactionsDto;

[Serializable]
public class TransactionDto
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
}
