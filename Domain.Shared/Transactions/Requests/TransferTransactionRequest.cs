using System.ComponentModel.DataAnnotations;

namespace Bank.Shared.Transactions.Requests;

public class TransferTransactionRequest : TransactionRequest
{    
    [Required]
    [RegularExpression(@"^\d{4}( \d{4}){3}$", ErrorMessage = "Card must have 16 digits")]
    public string CardNumber { get; set; } = default!;
}
