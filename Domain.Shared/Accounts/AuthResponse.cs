
namespace Bank.Shared.Accounts;

public record AuthResponse(
    AccountDto Account,
    string Token) { }
