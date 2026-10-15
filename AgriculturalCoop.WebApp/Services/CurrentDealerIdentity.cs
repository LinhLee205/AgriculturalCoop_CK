using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Microsoft.AspNetCore.Components.Authorization;
namespace AgriculturalCoop.WebApp.Services;
public class CurrentDealerIdentity(AuthenticationStateProvider authentication) : IDealerIdentity
{
    public async Task<int> GetDealerIdAsync()
    {
        var user = (await authentication.GetAuthenticationStateAsync()).User;
        if (!user.IsInRole("Dealer") || !int.TryParse(user.FindFirst("DealerId")?.Value, out var id) || id <= 0)
            throw new PreOrderDomainException("Vui lòng đăng nhập đại lý để xem thông báo của mình.");
        return id;
    }
}
