namespace Bank.Api.Accounts;

public sealed class JwtOptions
{
    public string SecretKey { get; init; } = null!;
    public int ExpiryMinutes { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}