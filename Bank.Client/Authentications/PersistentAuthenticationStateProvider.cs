
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bank.Client.Authentications;

public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> defaultAuthenticationStateTask =
         Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private Task<AuthenticationState> authenticationStateTask = defaultAuthenticationStateTask;
    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (await SecureStorage.Default.GetAsync("Token") is string token)
        {
            JwtSecurityToken jwt = new(token);

            authenticationStateTask = Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
                    claims: jwt.Claims,
                    authenticationType: nameof(AuthenticationStateProvider)
                    ))));
        }
        else
            authenticationStateTask = defaultAuthenticationStateTask;

        NotifyAuthenticationStateChanged(authenticationStateTask);

        return authenticationStateTask.Result;
    }
}
