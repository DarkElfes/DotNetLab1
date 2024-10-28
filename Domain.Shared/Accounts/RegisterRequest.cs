
namespace Bank.Shared.Accounts;

public record RegisterRequest(
    string Username,
    string Email,
    string Password) { }
