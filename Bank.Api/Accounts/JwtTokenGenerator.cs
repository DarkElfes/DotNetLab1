using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bank.Api.Accounts;

public sealed record JwtTokenGenerator(
    ILogger<JwtTokenGenerator> _logger,
    IOptions<JwtOptions> _jwtIOptions
    )
{
    private readonly JwtOptions _jwtOptions = _jwtIOptions.Value;

    public Result<string> GenerateToken(Account account)
    {
        SigningCredentials signingCredentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                SecurityAlgorithms.HmacSha256Signature
                );


        List<Claim> claims =
        [
            new (ClaimTypes.NameIdentifier, account.Id.ToString()),
            new (ClaimTypes.Name, account.Username),
            new (ClaimTypes.Email, account.Email),
        ];

        JwtSecurityToken jwtSecurityToken = new(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials
            );

        try
        {
            string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            _logger.LogInformation("Access token for account: @{AccountId} has been successfully created", account.Id);
            return Result.Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create access token for account: @{AccountId}, because: @{ErrorMessage}", account.Id, ex.Message);
            return Result.Fail("An error occurred while creating access token");
        }
    }
}
