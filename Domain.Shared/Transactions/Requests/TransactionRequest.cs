using System.ComponentModel.DataAnnotations;
using Bank.Shared.Transactions.Enums;

namespace Bank.Shared.Transactions.Requests;

public class TransactionRequest
{
    public TransactionType Type { get; set; }

    [Required]
    [Range(1, 100000, ErrorMessage = "Amount must be more than 1 and limited by 100000")]
    public decimal Amount { get; set; }
}
