namespace Bank.Shared.Accounts;

[Serializable]
public class AccountDto { 
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
