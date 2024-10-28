using Bank.Shared.Transactions.Enums;

namespace Bank.Api.Transactions.TransactionModels;

public class Transaction
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    public TransactionType Type { get; set; }
}
