using System.Security.Claims;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.Accounts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AgriculturalCoop.WebApp.Security;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        MapLogin(app, "/auth/login", "/account/login", "Dealer");
        MapLogin(app, "/auth/admin/login", "/admin/login", "Admin");

        app.MapPost("/auth/register", async (
            HttpContext context,
            IAccountUseCases accounts,
            IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest(
                    "Phiên biểu mẫu hết hạn. Hãy tải lại trang.");
            }

            var form = await context.Request.ReadFormAsync();

            try
            {
                await accounts.RegisterAsync(
                    form["email"].ToString(),
                    form["name"].ToString(),
                    form["phone"].ToString(),
                    form["password"].ToString(),
                    form["confirmation"].ToString());

                return Results.Redirect("/account/login?registered=true");
            }
            catch (PreOrderDomainException ex)
            {
                return Results.Redirect(
                    "/account/register?error=" +
                    Uri.EscapeDataString(ex.Message));
            }
        }).RequireRateLimiting("accounts");

        app.MapPost("/auth/setup", async (
            HttpContext context,
            IAccountUseCases accounts,
            IAntiforgery antiforgery) =>
        {
            if (!app.Environment.IsDevelopment() ||
                context.Connection.RemoteIpAddress is not { } ip ||
                !System.Net.IPAddress.IsLoopback(ip) ||
                await accounts.HasAdminAsync())
            {
                return Results.NotFound();
            }

            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest(
                    "Phiên biểu mẫu hết hạn. Hãy tải lại trang.");
            }

            var form = await context.Request.ReadFormAsync();

            try
            {
                await accounts.RegisterAsync(
                    form["email"].ToString(),
                    form["name"].ToString(),
                    "",
                    form["password"].ToString(),
                    form["confirmation"].ToString(),
                    true);

                return Results.Redirect("/admin/login?registered=true");
            }
            catch (PreOrderDomainException ex)
            {
                return Results.Redirect(
                    "/account/setup?error=" +
                    Uri.EscapeDataString(ex.Message));
            }
        }).RequireRateLimiting("accounts");

        app.MapPost("/auth/logout", async (
            HttpContext context,
            IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest(
                    "Phiên biểu mẫu hết hạn. Hãy tải lại trang.");
            }

            await context.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/");
        }).RequireAuthorization();
    }

    private static void MapLogin(WebApplication app, string path, string loginPage, string role)
    {
        app.MapPost(path, async (
            HttpContext context, IAccountUseCases accounts, IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest("Phiên biểu mẫu hết hạn. Hãy tải lại trang.");
            }

            var form = await context.Request.ReadFormAsync();
            var user = await accounts.LoginAsync(form["email"].ToString(), form["password"].ToString());
            if (user == null || user.Role != role)
                return Results.Redirect(loginPage + "?error=invalid");

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Principal(user),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8), AllowRefresh = true });

            var returnUrl = LocalReturnUrl(form["returnUrl"].ToString());
            if (role == "Dealer" && returnUrl != null &&
                returnUrl.Split('?', '#')[0].StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
                returnUrl = null;

            return Results.Redirect(returnUrl ?? (role == "Admin" ? "/admin" : "/batches"));
        }).RequireRateLimiting("accounts");
    }

    private static ClaimsPrincipal Principal(UserAccount user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserAccountId.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.DealerId.HasValue)
            claims.Add(new("DealerId", user.DealerId.Value.ToString()));

        return new ClaimsPrincipal(new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static string? LocalReturnUrl(string? value) =>
        !string.IsNullOrEmpty(value) &&
        value.StartsWith('/') &&
        !value.StartsWith("//") &&
        !value.Contains('\\') &&
        !value.Any(char.IsControl) &&
        !value.StartsWith("/auth/", StringComparison.OrdinalIgnoreCase)
            ? value : null;
}
