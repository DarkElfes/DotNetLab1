using Bank.Shared.Accounts;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bank.Client.Authentications;

public class AuthenticationService(
    IHttpClientFactory httpClientFactory,
    AuthenticationStateProvider authStateProvider)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    private AccountDto? _account;
    private string _token = string.Empty;


    // Getters method
    public async ValueTask<Result<AccountDto>> GetAccountAsync()
    {
        if (_account is not null)
            return Result.Ok(_account);


        if (await SecureStorage.Default.GetAsync("Account") is string json)
        {
            _account = JsonSerializer.Deserialize<AccountDto>(json);
            if (_account is not null)
                return Result.Ok(_account);
        }

        await LogoutAsync();
        return Result.Fail("Failed to get account data");
    }
    public async ValueTask<Result<string>> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_token))
            return Result.Ok(_token);

        var token = await SecureStorage.Default.GetAsync("Token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            _token = token;
            return Result.Ok(token);
        }

        await LogoutAsync();
        return Result.Fail("Failed to get access token");
    }



    // Authentication method
    public async Task<Result> LogoutAsync()
    {
        SecureStorage.Default.RemoveAll();
        _account = null;
        _token = string.Empty;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated ?? false)
            return Result.Ok();

        return Result.Fail("Failed to remove data from storage");
    }
    public async Task<Result> LoginAsync(LoginRequest loginRequest)
    {
        using HttpClient client = _httpClientFactory.CreateClient("ServerApi");
        return await ResponseHandlerAsync(await client.PostAsJsonAsync("api/auth/login", loginRequest));
    }
    public async Task<Result> RegisterAsync(RegisterRequest registerRequest)
    {
        using HttpClient client = _httpClientFactory.CreateClient("ServerApi");
        return await ResponseHandlerAsync(await client.PostAsJsonAsync("api/auth/register", registerRequest));
    }

    
    private async Task<Result> ResponseHandlerAsync(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
                return Result.Fail(await response.Content.ReadAsStringAsync());

            if (await response.Content.ReadFromJsonAsync<AuthResponse>() is not AuthResponse authResponse)
                return Result.Fail("Failed to read response of authentication");

            (_account, _token) = authResponse;

            await SecureStorage.Default.SetAsync("Account", JsonSerializer.Serialize(authResponse.Account));
            await SecureStorage.Default.SetAsync("Token", authResponse.Token);
            await _authStateProvider.GetAuthenticationStateAsync();

            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Unhandled exception");
        }
    }
}
