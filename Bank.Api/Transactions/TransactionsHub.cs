using AutoMapper;
using Bank.Api.Cards;
using Bank.Api.Transactions.TransactionModels;
using Bank.Shared.Cards;
using Bank.Shared.Transactions.Enums;
using Bank.Shared.Transactions.Requests;
using Bank.Shared.Transactions.TransactionsDto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;

namespace Bank.Api.Transactions;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class TransactionsHub(
    TransactionRepository transactionRepo,
    CardRepository cardRepo,
    IMapper mapper
    ) : Hub<ITransactionUpdateClient>
{
    private readonly TransactionRepository _transactionRepo = transactionRepo;
    private readonly CardRepository _cardRepo = cardRepo;
    private readonly IMapper _mapper = mapper;

    private static readonly Dictionary<string, string> _connections = [];

    public override async Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is null)
        {
            await Clients.Caller.Disconnect("Invalid user identifier");
            Context.Abort();
            return;
        }
        else if (_connections.ContainsKey(Context.UserIdentifier))
        {
            await Clients.Caller.Disconnect("User with this identifier already connected");
            Context.Abort();
            return;
        }

        var result = await _cardRepo.GetByAccountIdAsync(Context.UserIdentifier);
        if (result.IsFailed || result.Value is null)
        {
            await Clients.Caller.Disconnect("Internal server error");
            Context.Abort();
            return;
        }

        _connections[Context.UserIdentifier] = Context.ConnectionId;


        Card card = result.Value;

        var ts1 = _mapper.Map<List<TellerMachineTransactionDto>>(card.TellerMachineTransactions);
        var ts2 = _mapper.Map<List<ServicesPaymentTransactionDto>>(card.ServicesPaymentTrasnactions);
        var ts3 = _mapper.Map<List<TransferTransactionDto>>(card.SentTransferTransactions);
        var ts4 = _mapper.Map<List<TransferTransactionDto>>(card.ReceivedTransferTransactions);

        var ts = ts1.Cast<TransactionDto>()
                    .Concat(ts2.Cast<TransactionDto>())
                    .Concat(ts3.Cast<TransactionDto>())
                    .Concat(ts4.Cast<TransactionDto>())
                    .OrderByDescending(t => t.Created)
                    .ToList();

        var cardDto = _mapper.Map<CardDto>(card);
        cardDto.Transactions = ts;

        await Clients.Caller.ReceiveCard(cardDto);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    { 
        if(_connections[Context.UserIdentifier!].Equals(Context.ConnectionId))
            _connections.Remove(Context.UserIdentifier!);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTransaction(TransactionRequest transactionRequest)
    {
        if (!Validator.TryValidateObject(transactionRequest, new(transactionRequest), null))
            await Clients.Caller.ReceiveErrorMessage("Incorrect transaction data");

        var result = await _cardRepo.GetByAccountIdAsync(Context.UserIdentifier!);
        if (result.IsFailed || result.Value is null)
            await Clients.Caller.ReceiveErrorMessage(result.Errors.Last().Message);

        Card card = result.Value!;

        switch (transactionRequest.Type)
        {
            case TransactionType.Deposit:
                await HandleDepositTransaction(transactionRequest, card);
                break;
            case TransactionType.Withdraw:
                await HandleWithdrawTransaction(transactionRequest, card);
                break;
            case TransactionType.Transfer:
                await HandleTansferTransaction(transactionRequest, card);
                break;
            default:
                await Clients.Caller.ReceiveErrorMessage("Incorrect transaction type.Try again");
                break;
        }
    }

    private async Task HandleDepositTransaction(TransactionRequest transactionRequest, Card card)
    {
        card.Balance += transactionRequest.Amount;

        TellerMachineTransaction t = new()
        {
            Amount = transactionRequest.Amount,
            Created = DateTime.UtcNow,
            Balance = card.Balance,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Card = card,
            MachineName = "Violet Bank Terminal"
        };

        card.TellerMachineTransactions.Add(t);
        await _cardRepo.UpdateAsync(card);

        var tDto = _mapper.Map<TellerMachineTransactionDto>(t);

        await Clients.Caller.ReceiveTransaction(tDto, card.Balance);
    }
    private async Task HandleWithdrawTransaction(TransactionRequest transactionRequest, Card card)
    {
        TellerMachineTransaction t = new()
        {
            Amount = -transactionRequest.Amount,
            Created = DateTime.UtcNow,
            Type = TransactionType.Withdraw,
            Card = card,
            MachineName = "Violet Bank Terminal",
            Balance = card.Balance,
        };

        if (card.Balance > transactionRequest.Amount)
        {
            card.Balance -= transactionRequest.Amount;
            t.Status = TransactionStatus.Completed;
        }
        else
        {
            t.Status = TransactionStatus.Failed;
            t.ErrorMessage = "Not enough money for withdrawing";
            await Clients.Caller.ReceiveErrorMessage("Not enough money for withdrawing");
        }

        card.TellerMachineTransactions.Add(t);
        await _cardRepo.UpdateAsync(card);

        var tDto = _mapper.Map<TellerMachineTransactionDto>(t);

        await Clients.Caller.ReceiveTransaction(tDto, card.Balance);
    }
    private async Task HandleTansferTransaction(TransactionRequest transactionRequest, Card senderCard)
    {
        if (transactionRequest is not TransferTransactionRequest tRequest)
        {
            await Clients.Caller.ReceiveErrorMessage("Incorrect transfer transaction request");
            return;
        }

        if (senderCard.Balance < tRequest.Amount)
        {
            await Clients.Caller.ReceiveErrorMessage("Not enough money for sending");
            return;
        }

        var result = await _cardRepo.GetByCardNumberAsync(tRequest.CardNumber.Replace(" ", "").TrimStart('0'));
        if (result.IsFailed)
        {
            await Clients.Caller.ReceiveErrorMessage(result.Errors.Last().Message);
            return;
        }
        Card receiverCard = result.Value;

        senderCard.Balance -= transactionRequest.Amount;
        receiverCard.Balance += transactionRequest.Amount;

        TransferTransaction t = new()
        {
            Amount = transactionRequest.Amount,
            Created = DateTime.UtcNow,
            Type = TransactionType.Transfer,
            SenderCard = senderCard,
            ReceiverCard = receiverCard,
            Balance = senderCard.Balance,
            Status = TransactionStatus.Completed
        };

        await _transactionRepo.AddAsync(t);

        var tDto = _mapper.Map<TransferTransactionDto>(t);

        await Clients.Caller.ReceiveTransaction(tDto, senderCard.Balance);
        await Clients.User(receiverCard.Owner.Id).ReceiveTransaction(tDto, receiverCard.Balance);
    }
}
