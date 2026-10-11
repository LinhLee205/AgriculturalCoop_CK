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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================================
// 1. ĐĂNG KÝ DATASTORE PLUGIN REPOSITORIES (K1.2 & K1.4)
// ============================================================================
// MẶC ĐỊNH SPRINT 1: Dùng In-Memory Repository để chạy ngay lập tức với dữ liệu mẫu hoàn chỉnh
var inMemoryStore = new InMemoryDataStore();
builder.Services.AddSingleton<IMemberRepository>(inMemoryStore);
builder.Services.AddSingleton<IProductRepository>(inMemoryStore);
builder.Services.AddSingleton<IHarvestBatchRepository>(inMemoryStore);
builder.Services.AddSingleton<IDealerRepository>(inMemoryStore);
builder.Services.AddSingleton<IPreOrderRepository>(inMemoryStore);
builder.Services.AddSingleton<IAllocationRepository>(inMemoryStore);

// KHI CHUYỂN SANG SPRINT 2 (SQL Server + Dapper): Chỉ cần đổi các dòng đăng ký DI ở đây mà UseCase không phải sửa:
/*
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=AgriculturalCoopDB;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddScoped<ISqlDataAccess>(sp => new SqlDataAccess(connectionString));
builder.Services.AddScoped<IMemberRepository, SqlMemberRepository>();
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
builder.Services.AddScoped<IHarvestBatchRepository, SqlHarvestBatchRepository>();
builder.Services.AddScoped<IDealerRepository, SqlDealerRepository>();
builder.Services.AddScoped<IPreOrderRepository, SqlPreOrderRepository>();
builder.Services.AddScoped<IAllocationRepository, SqlAllocationRepository>();
*/

// ============================================================================
// 2. ĐĂNG KÝ DOMAIN SERVICES (CoreBusiness)
// ============================================================================
builder.Services.AddTransient<AllocationService>();

// ============================================================================
// 3. ĐĂNG KÝ USE CASES (UseCases)
// ============================================================================
// Members Use Cases
builder.Services.AddTransient<IViewMembersUseCase, ViewMembersUseCase>();
builder.Services.AddTransient<IAddMemberUseCase, AddMemberUseCase>();

// Products Use Cases
builder.Services.AddTransient<IViewProductsUseCase, ViewProductsUseCase>();
builder.Services.AddTransient<IAddProductUseCase, AddProductUseCase>();

// HarvestBatches Use Cases
builder.Services.AddTransient<IViewHarvestBatchesUseCase, ViewHarvestBatchesUseCase>();
builder.Services.AddTransient<IViewOpenBatchesUseCase, ViewOpenBatchesUseCase>();
builder.Services.AddTransient<IViewHarvestBatchByIdUseCase, ViewHarvestBatchByIdUseCase>();
builder.Services.AddTransient<ICreateHarvestBatchUseCase, CreateHarvestBatchUseCase>();
builder.Services.AddTransient<IUpdateActualYieldUseCase, UpdateActualYieldUseCase>();

// Dealers Use Cases
builder.Services.AddTransient<IViewDealersUseCase, ViewDealersUseCase>();
builder.Services.AddTransient<IGetDealerByIdUseCase, GetDealerByIdUseCase>();

// PreOrders Use Cases (Luật 1: CreatePreOrderUseCase)
builder.Services.AddTransient<ICreatePreOrderUseCase, CreatePreOrderUseCase>();
builder.Services.AddTransient<IViewPreOrdersByDealerUseCase, ViewPreOrdersByDealerUseCase>();
builder.Services.AddTransient<IViewAllPreOrdersUseCase, ViewAllPreOrdersUseCase>();
builder.Services.AddTransient<IViewPreOrderByIdUseCase, ViewPreOrderByIdUseCase>();

// Allocations Use Cases (Luật 2: AllocateHarvestUseCase)
builder.Services.AddTransient<IAllocateHarvestUseCase, AllocateHarvestUseCase>();
builder.Services.AddTransient<IViewAllocationsByBatchUseCase, ViewAllocationsByBatchUseCase>();
builder.Services.AddTransient<IViewAllocationsByDealerUseCase, ViewAllocationsByDealerUseCase>();

// AI Extension (K4.3)
builder.Services.AddTransient<IGenerateAllocationNoticeUseCase, GenerateAllocationNoticeUseCase>();

// ============================================================================
// 4. ĐĂNG KÝ STATE & SESSION (K1.4: Scoped State)
// ============================================================================
builder.Services.AddScoped<IPreOrderCart, PreOrderCart>();
builder.Services.AddScoped<UserSessionState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
