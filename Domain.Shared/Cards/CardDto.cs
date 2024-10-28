using Bank.Shared.Transactions.TransactionsDto;

namespace Bank.Shared.Cards;

[Serializable]
public class CardDto { 
    public int Id { get; set; }
    public decimal Balance  { get; set; } 
    public List<TransactionDto> Transactions { get; set; } = [];

    public string Number => Id.ToString().PadLeft(16, '0');
}
 