using Bank.Client.Authentications;
using Bank.Shared.Cards;
using Bank.Shared.Transactions.Requests;
using Bank.Shared.Transactions.TransactionsDto;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Bank.Client.Services;

public sealed class TransactionService(
    AuthenticationService authService,
    IConfiguration config,
    IDispatcher dispatcher
    )
{
    private readonly AuthenticationService _authService = authService;
    private readonly IConfiguration _config = config;
    private readonly IDispatcher _dispatcher = dispatcher;

    public CardDto Card { get; private set; } = null!;
    public event Action? NotifyCardUpdate;
    public event Action<string>? NotifyErrorUpdate;

    private HubConnection? _hubConnection;

    public async Task<Result> ConnectAsync()
    {
        if (_config["ServerUrl"] is not string serverUrl)
            return Result.Fail("Server url is not set. Can not connected");

        var result = await _authService.GetTokenAsync();
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        var token = result.Value;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hubs/transactions", options =>
                 options.AccessTokenProvider = () => Task.FromResult(token)!)
            .AddNewtonsoftJsonProtocol(options =>
                options.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.All)
            .Build();

        _hubConnection.Closed += async arg => await _authService.LogoutAsync();

        _hubConnection.On<CardDto>("ReceiveCard",
            card =>
            {
                Card = card;
                NotifyCardUpdate?.Invoke();
            });

        _hubConnection.On<TransactionDto, decimal>("ReceiveTransaction",
            (transaction, curBalance) =>
            {
                Card.Balance = curBalance;
                Card.Transactions.Insert(0, transaction);
                NotifyCardUpdate?.Invoke();
            });

        _hubConnection.On<string>("ReceiveErrorMessage",
            errorMessage =>
            {
                NotifyErrorUpdate?.Invoke(errorMessage); 
            });

        _hubConnection.On<string>("Disconnect",
            async disconnectMessage =>
            {
                NotifyErrorUpdate?.Invoke(disconnectMessage);
                await _hubConnection.StopAsync();
            });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }

    public async ValueTask DisconnectAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }

    public async Task<Result> SendTransactionAsync(TransactionRequest transactionRequest)
        => await TrySendToHubAsync("SendTransaction", transactionRequest);

    private async Task<Result> TrySendToHubAsync(string methodName, object value)
    {
        if (_hubConnection is null)
            return Result.Fail("Hub connection is not initialized");

        try
        {
            await _hubConnection.SendAsync(methodName, value);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }

}
