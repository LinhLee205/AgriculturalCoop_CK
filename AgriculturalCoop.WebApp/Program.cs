using AgriculturalCoop.UseCases.Accounts;
using AgriculturalCoop.UseCases.Management;
using AgriculturalCoop.WebApp.Security;
using AgriculturalCoop.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AgriculturalCoop.WebApp.Components;
using AgriculturalCoop.WebApp.State;
using AgriculturalCoop.CoreBusiness.Services;
using AgriculturalCoop.UseCases.PluginInterfaces;
using AgriculturalCoop.UseCases.Members;
using AgriculturalCoop.UseCases.Products;
using AgriculturalCoop.UseCases.HarvestBatches;
using AgriculturalCoop.UseCases.Dealers;
using AgriculturalCoop.UseCases.PreOrders;
using AgriculturalCoop.UseCases.Allocations;
using AgriculturalCoop.UseCases.AI;
using AgriculturalCoop.DataStore.InMemory;
using AgriculturalCoop.DataStore.Sql;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var provider = builder.Configuration["DataStore:Provider"] ?? "SqlServer";
if (provider is not ("SqlServer" or "InMemory"))
    throw new InvalidOperationException("DataStore:Provider phải là SqlServer hoặc InMemory.");

var useInMemory = provider == "InMemory";

if (useInMemory && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException(
        "InMemory chỉ được dùng để kiểm thử trong Development.");

if (useInMemory)
{
    var store = new InMemoryDataStore();

    builder.Services.AddSingleton<IMemberRepository>(store);
    builder.Services.AddSingleton<IProductRepository>(store);
    builder.Services.AddSingleton<IHarvestBatchRepository>(store);
    builder.Services.AddSingleton<IDealerRepository>(store);
    builder.Services.AddSingleton<IPreOrderRepository>(store);
    builder.Services.AddSingleton<IAllocationRepository>(store);
    builder.Services.AddSingleton<IUserAccountRepository>(store);
    builder.Services.AddSingleton<IRevenueRepository>(store);
    builder.Services.AddSingleton<IDealerNotificationRepository>(store);
    builder.Services.AddSingleton<IOrderManagementRepository>(store);
    builder.Services.AddSingleton<IHomeBannerRepository, InMemoryHomeBannerRepository>();
}
else
{
    var connection = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Cần cấu hình ConnectionStrings:DefaultConnection tới SQL Server.");

    builder.Services.AddScoped<ISqlDataAccess>(_ => new SqlDataAccess(connection));
    builder.Services.AddScoped<IMemberRepository, SqlMemberRepository>();
    builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
    builder.Services.AddScoped<IHarvestBatchRepository, SqlHarvestBatchRepository>();
    builder.Services.AddScoped<IDealerRepository, SqlDealerRepository>();
    builder.Services.AddScoped<IPreOrderRepository, SqlPreOrderRepository>();
    builder.Services.AddScoped<IAllocationRepository, SqlAllocationRepository>();
    builder.Services.AddScoped<IUserAccountRepository, SqlUserAccountRepository>();
    builder.Services.AddScoped<IRevenueRepository, SqlRevenueRepository>();
    builder.Services.AddScoped<IDealerNotificationRepository, SqlDealerNotificationRepository>();
    builder.Services.AddScoped<IOrderManagementRepository, SqlOrderManagementRepository>();
    builder.Services.AddScoped<IHomeBannerRepository, SqlHomeBannerRepository>();
}

var storageRoot = Path.GetFullPath(
    builder.Configuration["Storage:Root"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data"));

Directory.CreateDirectory(Path.Combine(storageRoot, "keys"));
Directory.CreateDirectory(Path.Combine(storageRoot, "product-images"));

builder.Services.AddDataProtection()
    .SetApplicationName("AgriculturalCoop")
    .PersistKeysToFileSystem(
        new DirectoryInfo(Path.Combine(storageRoot, "keys")));

builder.Services.AddSingleton<IProductImageStore>(
    new ProductImageStore(Path.Combine(storageRoot, "product-images")));

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAccountUseCases, AccountUseCases>();
builder.Services.AddScoped<IManageMembersUseCase, ManageMembersUseCase>();
builder.Services.AddScoped<IManageBatchesUseCase, ManageBatchesUseCase>();
builder.Services.AddScoped<IManageProductsUseCase, ManageProductsUseCase>();
builder.Services.AddScoped<IManageOrdersUseCase, ManageOrdersUseCase>();
builder.Services.AddScoped<IManageDealersUseCase, ManageDealersUseCase>();
builder.Services.AddScoped<IViewMemberRevenueUseCase, ViewMemberRevenueUseCase>();
builder.Services.AddScoped<IHomeBannerUseCases, HomeBannerUseCases>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AgriculturalCoop.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect("/admin/login?switchAccount=true&returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect("/admin/login?switchAccount=true&returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        options.Events.OnValidatePrincipal = async context =>
        {
            var email = context.Principal?.FindFirstValue(ClaimTypes.Email);

            var repository = context.HttpContext.RequestServices
                .GetRequiredService<IUserAccountRepository>();

            var account = email == null
                ? null : await repository.FindByEmailAsync(email);

            if (account?.Role == "Dealer" && (!account.DealerId.HasValue ||
                (await context.HttpContext.RequestServices.GetRequiredService<IDealerRepository>()
                    .GetDealerByIdAsync(account.DealerId.Value)) is not { IsActive: true }))
            {
                context.RejectPrincipal();
                return;
            }

            if (account == null ||
                account.UserAccountId.ToString() !=
                    context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ||
                account.Role != context.Principal.FindFirstValue(ClaimTypes.Role) ||
                account.DealerId?.ToString() != context.Principal.FindFirstValue("DealerId"))
            {
                context.RejectPrincipal();
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("accounts", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddTransient<AllocationService>();

builder.Services.AddTransient<IViewMembersUseCase, ViewMembersUseCase>();
builder.Services.AddTransient<IAddMemberUseCase, AddMemberUseCase>();

builder.Services.AddTransient<IViewProductsUseCase, ViewProductsUseCase>();
builder.Services.AddTransient<IAddProductUseCase, AddProductUseCase>();

builder.Services.AddTransient<IViewHarvestBatchesUseCase, ViewHarvestBatchesUseCase>();
builder.Services.AddTransient<IViewOpenBatchesUseCase, ViewOpenBatchesUseCase>();
builder.Services.AddTransient<IViewHarvestBatchByIdUseCase, ViewHarvestBatchByIdUseCase>();
builder.Services.AddTransient<ICreateHarvestBatchUseCase, CreateHarvestBatchUseCase>();
builder.Services.AddTransient<IUpdateActualYieldUseCase, UpdateActualYieldUseCase>();

builder.Services.AddTransient<IViewDealersUseCase, ViewDealersUseCase>();
builder.Services.AddTransient<IGetDealerByIdUseCase, GetDealerByIdUseCase>();

builder.Services.AddTransient<ICreatePreOrderUseCase, CreatePreOrderUseCase>();
builder.Services.AddTransient<IValidatePreOrderUseCase, CreatePreOrderUseCase>();
builder.Services.AddTransient<IViewPreOrdersByDealerUseCase, ViewPreOrdersByDealerUseCase>();
builder.Services.AddTransient<IViewAllPreOrdersUseCase, ViewAllPreOrdersUseCase>();
builder.Services.AddTransient<IViewPreOrderByIdUseCase, ViewPreOrderByIdUseCase>();
builder.Services.AddTransient<IViewPreOrdersForBatchUseCase, ViewPreOrdersForBatchUseCase>();

builder.Services.AddTransient<IAllocateHarvestUseCase, AllocateHarvestUseCase>();
builder.Services.AddTransient<IViewAllocationsByBatchUseCase, ViewAllocationsByBatchUseCase>();
builder.Services.AddTransient<IViewAllocationsByDealerUseCase, ViewAllocationsByDealerUseCase>();

builder.Services.AddTransient<IGenerateAllocationNoticeUseCase, GenerateAllocationNoticeUseCase>();
builder.Services.AddScoped<AllocationNoticeDataReader>();
builder.Services.AddScoped<ISendAllocationNoticeUseCase, SendAllocationNoticeUseCase>();
builder.Services.AddScoped<IPublishAllocationNoticeUseCase, PublishAllocationNoticeUseCase>();
builder.Services.AddScoped<IDealerNotificationUseCases, DealerNotificationUseCases>();
builder.Services.AddScoped<IDealerIdentity, CurrentDealerIdentity>();
builder.Services.AddScoped<IAdminAuthorization, CurrentAdminAuthorization>();
builder.Services.Configure<NoticeEmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IAllocationNoticeEmailSender, SmtpAllocationNoticeSender>();
var aiConnection = NoticeAiConnection.Read(builder.Configuration);
builder.Services.AddSingleton(aiConnection);
builder.Services.AddSingleton<INoticeAiStatus>(aiConnection);
if (aiConnection.IsConfigured)
{
    builder.Services.AddSingleton<IChatClient>(_ => aiConnection.CreateChatClient());
    builder.Services.AddScoped<IAllocationNoticeWriter, ComposeAllocationNoticeWordingUseCase>();
}
else
{
    builder.Services.AddScoped<IAllocationNoticeWriter, UnconfiguredNoticeWriter>();
}

builder.Services.AddScoped<IPreOrderCart, PreOrderCart>();
builder.Services.AddScoped<UserSessionState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(storageRoot, "product-images")),
    RequestPath = "/uploads/products"
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAccountEndpoints();

app.Run();
