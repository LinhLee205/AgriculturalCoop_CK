using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AgriculturalCoop.WebApp.State;

public class UserSessionState(AuthenticationStateProvider authentication)
{
    private ClaimsPrincipal principal = new(new ClaimsIdentity());

    public bool IsAuthenticated =>
        principal.Identity?.IsAuthenticated == true;

    public bool IsAdmin => principal.IsInRole("Admin");
    public bool IsDealer => principal.IsInRole("Dealer");

    public int CurrentDealerId =>
        int.TryParse(principal.FindFirst("DealerId")?.Value, out var id)
            ? id : 0;

    public string CurrentDealerName =>
        principal.Identity?.Name ?? string.Empty;

    public string CurrentUserName =>
        principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public async Task InitializeAsync() =>
        principal = (await authentication.GetAuthenticationStateAsync()).User;
}
