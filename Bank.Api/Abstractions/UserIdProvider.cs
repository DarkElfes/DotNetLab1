using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;
using System.Security.Claims;

namespace Bank.Api.Abstractions;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var id = connection.User?.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
        return id;
    }
}
