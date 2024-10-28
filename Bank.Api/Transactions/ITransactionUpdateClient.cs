using Bank.Shared.Cards;
using Bank.Shared.Transactions.TransactionsDto;

namespace Bank.Api.Transactions;

public interface ITransactionUpdateClient
{
    Task ReceiveErrorMessage(string errorMessage);
    Task Disconnect(string disconnectMessage);

    Task ReceiveCard(CardDto card);
    Task ReceiveTransaction(TransactionDto transaction, decimal curBalance);
}
