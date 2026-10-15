using System.Security.Claims;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.DataStore.InMemory;
using AgriculturalCoop.UseCases.PreOrders;
using AgriculturalCoop.WebApp.Components.Pages;
using AgriculturalCoop.WebApp.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class CheckoutRenderingChecks
{
    public static async Task<int> RunAsync()
    {
        var store = new InMemoryDataStore();
        var cart = new PreOrderCart();
        cart.AddItem((await store.GetBatchByIdAsync(2))!, 1.25m);
        var initialOrders = (await store.GetAllPreOrdersAsync()).Count();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<AuthenticationStateProvider, DealerAuthentication>();
        services.AddSingleton<UserSessionState>();
        services.AddSingleton<IPreOrderCart>(cart);
        services.AddSingleton<ICreatePreOrderUseCase>(new CreatePreOrderUseCase(store, store, store));
        services.AddSingleton<IValidatePreOrderUseCase>(new CreatePreOrderUseCase(store, store, store));
        services.AddSingleton<NavigationManager, TestNavigation>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<Cart>()).ToHtmlString());
        foreach (var required in new[] { "checkout-recipient", "checkout-phone", "checkout-address", "LO-CACHUA-002", "type=\"submit\"" })
            if (!html.Contains(required))
                throw new Exception("Checkout with a populated cart is missing: " + required);
        if ((await store.GetAllPreOrdersAsync()).Count() != initialOrders)
            throw new Exception("Viewing checkout created an order before confirmation.");
        return 2;
    }

    private sealed class DealerAuthentication : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test Dealer"),
                new Claim(ClaimTypes.Role, "Dealer"), new Claim("DealerId", "1") }, "Test"))));
    }

    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("http://localhost/", "http://localhost/checkout");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }
}
