using Bank.Api.Cards;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.Api.Accounts;

public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Card Card { get; set; } = null!;
}
