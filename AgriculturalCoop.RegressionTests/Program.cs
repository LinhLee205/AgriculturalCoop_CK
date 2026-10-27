using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.CoreBusiness.Services;
using AgriculturalCoop.DataStore.InMemory;
using AgriculturalCoop.DataStore.Sql;
using AgriculturalCoop.UseCases.Accounts;
using AgriculturalCoop.UseCases.Management;
using AgriculturalCoop.UseCases.PreOrders;
using AgriculturalCoop.UseCases.AI;
using AgriculturalCoop.UseCases.Allocations;
using AgriculturalCoop.WebApp.Security;
using AgriculturalCoop.WebApp.Services;
using Microsoft.Extensions.Logging.Abstractions;

var root = Environment.GetEnvironmentVariable("COOP_TEST_ROOT")
    ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
}
async Task Reject(Func<Task> action)
{
    try { await action(); }
    catch (PreOrderDomainException) { checks++; return; }
    throw new Exception("Expected business rule rejection.");
}

// Exercise conservation, precision and upper bounds over many fractional harvests.
var random = new Random(2415);
var allocation = new AllocationService();
for (var sample = 0; sample < 1000; sample++)
{
    var lines = Enumerable.Range(1, random.Next(2, 20)).Select(id => new PreOrderLine
    {
        PreOrderLineId = id,
        PreOrderId = id,
        Quantity = random.Next(1, 10000) / 100m
    }).ToList();
    var total = lines.Sum(l => l.Quantity);
    var actual = random.Next(0, (int)(total * 120)) / 100m;
    var result = allocation.CalculateAllocation(new HarvestBatch(), lines, actual);
    Check(result.Sum(r => r.AllocatedQuantity) == Math.Min(actual, total), "Allocation sum drifted.");
    Check(result.All(r => r.AllocatedQuantity >= 0 && r.AllocatedQuantity <= r.RequestedQuantity &&
        r.AllocatedQuantity == decimal.Round(r.AllocatedQuantity, 2)), "Invalid fractional allocation.");
}

var store = new InMemoryDataStore();
var orders = new CreatePreOrderUseCase(store, store, store);
await Reject(() => orders.ExecuteAsync(new PreOrder
{
    DealerId = 1,
    PreOrderLines = [new() { HarvestBatchId = 2, Quantity = 500 },
        new() { HarvestBatchId = 2, Quantity = 500 }]
}));
await Reject(() => orders.ExecuteAsync(new PreOrder
{
    DealerId = 1,
    PreOrderLines = [new() { HarvestBatchId = 2, Quantity = 0.001m }]
}));
var order = new PreOrder
{
    DealerId = 1,
    RecipientName = " Nguyễn Văn An ", ContactPhone = " 0901234567 ", DeliveryAddress = " 12 Lê Lợi, Huế ",
    PreOrderLines = [new() { HarvestBatchId = 2, Quantity = 0.25m, UnitPrice = 1 }]
};
await orders.ExecuteAsync(order);
Check(order.PreOrderLines[0].UnitPrice == 25000 && order.TotalAmount == 6250, "Client supplied price accepted.");
var storedOrder = await store.GetPreOrderByIdAsync(order.PreOrderId);
Check(storedOrder?.RecipientName == "Nguyễn Văn An" && storedOrder.ContactPhone == "0901234567" &&
    storedOrder.DeliveryAddress == "12 Lê Lợi, Huế", "Order delivery information was not retained independently.");
var invalidContact = new PreOrder { DealerId = 1, PreOrderLines = [new() { HarvestBatchId = 2, Quantity = 1 }] };
var countBeforeInvalidContact = (await store.GetAllPreOrdersAsync()).Count();
await Reject(() => orders.ExecuteAsync(invalidContact));
invalidContact.RecipientName = "Người nhận";
invalidContact.ContactPhone = "123";
invalidContact.DeliveryAddress = "Địa chỉ nhận hàng";
await Reject(() => orders.ExecuteAsync(invalidContact));
invalidContact.ContactPhone = "0901234567";
invalidContact.DeliveryAddress = "   ";
await Reject(() => orders.ExecuteAsync(invalidContact));
invalidContact.DeliveryAddress = new string('a', 256);
await Reject(() => orders.ExecuteAsync(invalidContact));
Check((await store.GetAllPreOrdersAsync()).Count() == countBeforeInvalidContact, "An invalid contact created an order.");
var countBeforePreview = (await store.GetAllPreOrdersAsync()).Count();
var cartPreview = new PreOrder { DealerId = 1, PreOrderLines = [new() { HarvestBatchId = 2, Quantity = 1, UnitPrice = 1 }] };
await orders.ValidateAsync(cartPreview);
Check((await store.GetAllPreOrdersAsync()).Count() == countBeforePreview && cartPreview.TotalAmount == 25000,
    "Cart validation must refresh prices without writing an order.");
var availability = new ManageBatchesUseCase(store);
await availability.SetOpenAsync(2, false);
Check(!(await store.GetOpenBatchesAsync()).Any(b => b.HarvestBatchId == 2), "Hidden lot remains visible.");
await Reject(() => orders.ValidateAsync(cartPreview));
await availability.SetOpenAsync(2, true);
Check((await store.GetOpenBatchesAsync()).Any(b => b.HarvestBatchId == 2), "Reopened lot is missing.");
await Reject(() => availability.SetOpenAsync(1, true));
var cancelStore = new InMemoryDataStore();
(await cancelStore.GetPreOrderByIdAsync(1))!.Status = PreOrderStatus.Cancelled;
var batchOrderLines = (await new ViewPreOrdersForBatchUseCase(cancelStore).ExecuteAsync(1)).ToList();
Check(batchOrderLines.Count == 3 && batchOrderLines.All(l => l.PreOrder?.Dealer != null), "Allocation order list lacks order/dealer information.");
Check(await cancelStore.GetTotalOrderedQuantityForBatchAsync(1) == 500 &&
    (await cancelStore.GetBatchByIdAsync(1))!.TotalOrderedQuantity == 500, "Cancelled orders still consume availability.");
var eligibleAllocation = await new AllocateHarvestUseCase(cancelStore, cancelStore, cancelStore, allocation).ExecuteAsync(1, 400);
Check(eligibleAllocation.All(a => a.PreOrderId != 1) && eligibleAllocation.Sum(a => a.AllocatedQuantity) == 400,
    "Cancelled orders participated in allocation.");
checks += await CheckoutRenderingChecks.RunAsync();
await Reject(() => new ManageMembersUseCase(store).DeleteAsync(1));
await Reject(() => new ManageProductsUseCase(store).DeleteAsync(1));
await Reject(() => new ManageBatchesUseCase(store).SaveAsync(new HarvestBatch
{
    HarvestBatchId = 2,
    BatchCode = "CHANGED",
    MemberId = 2,
    ProductId = 2,
    ExpectedQuantity = 800,
    UnitPrice = 1
}));
var accounts = new AccountUseCases(store, new PasswordService(), store);
const string password = "Regression12345";
await accounts.RegisterAsync(" ADMIN@example.test ", "Test Admin", "", password, password, true);
Check(await accounts.LoginAsync("admin@example.test", password) is { Role: "Admin", DealerId: null }, "Admin login failed.");
Check(await accounts.LoginAsync("admin@example.test", "wrong") == null, "Wrong password accepted.");
await Reject(() => accounts.RegisterAsync("second@example.test", "Other Admin", "", password, password, true));
await accounts.RegisterAsync("dealer@example.test", "Dealer Test", "0123456789", password, password);
Check(await accounts.LoginAsync("dealer@example.test", password) is { Role: "Dealer", DealerId: > 0 }, "Dealer registration failed.");
await Reject(() => accounts.RegisterAsync("DEALER@example.test", "Dealer Test", "", password, password));
Check(AccountEndpoints.LocalReturnUrl("//outside.test") == null &&
    AccountEndpoints.LocalReturnUrl("/\\outside.test") == null &&
    AccountEndpoints.LocalReturnUrl("/auth/logout") == null &&
    AccountEndpoints.LocalReturnUrl("/batches/2") == "/batches/2", "Unsafe return URL accepted.");

var storage = Path.Combine(root, "AgriculturalCoop.RegressionTests", "App_Data", Guid.NewGuid().ToString("N"));
var images = new ProductImageStore(Path.Combine(storage, "images"));
var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/lL8AAAAASUVORK5CYII=");
var imageUrl = await images.SaveAsync(new MemoryStream(png), png.Length);
var imagePath = Path.Combine(storage, "images", Path.GetFileName(imageUrl));
Check(File.ReadAllBytes(imagePath).SequenceEqual(png), "Image not persisted.");
await Reject(() => images.SaveAsync(new MemoryStream([1, 2, 3]), 3));
await images.RemoveAsync("/uploads/products/../../outside.png");
Check(File.Exists(imagePath), "Unsafe delete affected image.");
await images.RemoveAsync(imageUrl);
Check(!File.Exists(imagePath), "Image not removed.");

var bannerRepository = new InMemoryHomeBannerRepository();
var bannerUseCases = new HomeBannerUseCases(bannerRepository);
var editedBanner = await bannerUseCases.GetAsync();
editedBanner.Title = "Mùa mới từ nông trại";
Check((await bannerUseCases.GetAsync()).Title != editedBanner.Title, "Banner changed before saving.");
await bannerUseCases.SaveAsync(editedBanner);
Check((await bannerUseCases.GetAsync()).Title == editedBanner.Title, "Banner edits were not persisted.");
editedBanner.ButtonUrl = "//outside.test";
await Reject(() => bannerUseCases.SaveAsync(editedBanner));
editedBanner.ButtonUrl = "/auth/logout";
await Reject(() => bannerUseCases.SaveAsync(editedBanner));
editedBanner.ButtonUrl = "/#nong-san";
editedBanner.Title = " ";
await Reject(() => bannerUseCases.SaveAsync(editedBanner));

// Notification tests use saved allocations and test doubles: no external AI or email is called.
var adminAuthorization = new TestAdminAuthorization();
var noticeWriter = new TestNoticeWriter();
var emailSender = new TestNoticeEmailSender();
var noticeData = new AllocationNoticeDataReader(store, store, store, store);
var generateNotice = new GenerateAllocationNoticeUseCase(noticeData, noticeWriter, adminAuthorization);
var sendNotice = new SendAllocationNoticeUseCase(noticeData, emailSender, adminAuthorization);
await Reject(() => generateNotice.ExecuteAsync(1, 1, "", false));
await new AllocateHarvestUseCase(store, store, store, allocation).ExecuteAsync(1, 720m);
var savedAlloc = (await store.GetAllocationsByBatchIdAsync(1)).First();
var notice = await generateNotice.ExecuteAsync(1, savedAlloc.PreOrderLineId, "Đã kiểm tra sản lượng thực tế.", true);
Check(notice.IsAiGenerated && noticeWriter.Facts?.ActualQuantity == 720m &&
    noticeWriter.Facts.AllocatedQuantity == savedAlloc.AllocatedQuantity, "AI must receive saved allocation facts.");
Check(notice.Body.Contains(savedAlloc.AllocatedQuantity.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"))) &&
    !notice.Body.Contains("VietGAP") && !notice.Body.Contains("sáng mai"), "Notice must preserve quantities without invented promises.");
await Reject(() => sendNotice.ExecuteAsync(notice, false));
Check(emailSender.Count == 0, "Unapproved email was sent.");
adminAuthorization.Allowed = false;
await Reject(() => generateNotice.ExecuteAsync(1, savedAlloc.PreOrderLineId, "", true));
await Reject(() => sendNotice.ExecuteAsync(notice, true));
adminAuthorization.Allowed = true;
await sendNotice.ExecuteAsync(notice, true);
Check(emailSender.Count == 1 && emailSender.Recipient == notice.Snapshot.RecipientEmail, "Email recipient must come from dealer record.");
var savedQuantity = savedAlloc.AllocatedQuantity;
savedAlloc.AllocatedQuantity -= .01m;
await Reject(() => sendNotice.ExecuteAsync(notice, true));
Check(emailSender.Count == 1, "Stale allocation email was sent.");
savedAlloc.AllocatedQuantity = savedQuantity;
var noticeDealer = (await store.GetDealerByIdAsync(notice.Snapshot.DealerId))!;
var originalEmail = noticeDealer.Email;
noticeDealer.Email = "";
var missingEmailNotice = await generateNotice.ExecuteAsync(1, savedAlloc.PreOrderLineId, "", false);
await Reject(() => sendNotice.ExecuteAsync(missingEmailNotice, true));
noticeDealer.Email = originalEmail;
noticeWriter.IsConfigured = false;
await Reject(() => generateNotice.ExecuteAsync(1, savedAlloc.PreOrderLineId, "", true));
Check(!(await generateNotice.ExecuteAsync(1, savedAlloc.PreOrderLineId, "", false)).IsAiGenerated,
    "Template draft must remain usable and must not be labelled AI.");
emailSender.IsConfigured = false;
await Reject(() => sendNotice.ExecuteAsync(notice, true));
var testChat = new TestChatClient();
var aiAdapter = new ComposeAllocationNoticeWordingUseCase(testChat);
await aiAdapter.WriteAsync(noticeWriter.Facts!);
Check(!testChat.Messages.Last().Text.Contains(notice.Snapshot.RecipientEmail) &&
    !testChat.Messages.Last().Text.Contains(notice.Snapshot.DealerName), "AI payload must omit dealer identity and email.");
testChat.Json = "{\"introduction\":\"Phân bổ 9999 kg\",\"closing\":\"Cảm ơn\"}";
await Reject(() => aiAdapter.WriteAsync(noticeWriter.Facts!));
checks += await GeminiConnectionChecks.RunAsync();
checks += await NotificationChecks.RunAsync();
checks += await AdminLifecycleChecks.RunAsync();

// Use a separate development server with memory storage: no test accounts enter SQL.
var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
var web = Path.Combine(root, "AgriculturalCoop.WebApp");
var start = new ProcessStartInfo("dotnet")
{
    WorkingDirectory = web,
    UseShellExecute = false,
    CreateNoWindow = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true
};
start.ArgumentList.Add(Environment.GetEnvironmentVariable("COOP_TEST_WEB_DLL")
    ?? Path.Combine(web, "bin", configuration, "net8.0", "AgriculturalCoop.WebApp.dll"));
start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
start.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
start.Environment["DataStore__Provider"] = "InMemory";
start.Environment["Storage__Root"] = storage;
start.Environment["Logging__LogLevel__Default"] = "Warning";
start.Environment["AI__ApiKey"] = "";
start.Environment["AI__Provider"] = "Disabled";
start.Environment["AI__Model"] = "";
start.Environment["Email__Host"] = "";
using var server = Process.Start(start)!;
var stdout = server.StandardOutput.ReadToEndAsync();
var stderr = server.StandardError.ReadToEndAsync();
HttpClient Browser() => new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = true })
{ BaseAddress = new Uri($"http://127.0.0.1:{port}"), Timeout = TimeSpan.FromSeconds(15) };
try
{
    using var admin = Browser();
    for (var attempt = 0; ; attempt++)
    {
        try { using var ready = await admin.GetAsync("/account/login"); ready.EnsureSuccessStatusCode(); break; }
        catch (HttpRequestException) when (attempt < 50 && !server.HasExited) { await Task.Delay(200); }
    }
    async Task<HttpResponseMessage> Post(HttpClient client, string page, string action, Dictionary<string, string> values)
    {
        var html = await client.GetStringAsync(page);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Check(match.Success, "Missing antiforgery form token.");
        values["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        return await client.PostAsync(action, new FormUrlEncodedContent(values));
    }
    Dictionary<string, string> Registration(string email) => new()
    {
        ["email"] = email,
        ["name"] = "HTTP Test",
        ["phone"] = "0123456789",
        ["password"] = password,
        ["confirmation"] = password
    };
    var guestAdmin = await admin.GetAsync("/admin");
    Check(guestAdmin.StatusCode == HttpStatusCode.Redirect &&
        guestAdmin.Headers.Location!.ToString().Contains("/admin/login"), "Guest can access admin or admin login routing failed.");
    Check((await admin.GetAsync("/admin/banner")).StatusCode == HttpStatusCode.Redirect, "Guest can edit banner.");
    var home = await admin.GetStringAsync("/");
    Check(Regex.Matches(home, "data-product-id=").Count >= 2 && home.Contains("data-product-id=\"2\"") &&
        home.Contains("data-product-id=\"3\"") && !home.Contains("data-product-id=\"1\"") && !home.Contains("data-product-id=\"4\""),
        "Storefront must only show products with an open harvest lot.");
    Check(home.Contains("store-banner") && !home.Contains("K1.3") && !home.Contains("Capstone"), "Storefront still contains course content.");
    Check(home.Contains("store-navbar-inner") && home.Contains("href=\"/account/login\"") &&
        home.Contains("href=\"/account/register\"") && !home.Contains("href=\"/admin\""),
        "Guest storefront must show dealer sign-in and registration only.");
    foreach (var id in new[] { 1, 2, 3, 4 })
        Check((await admin.GetAsync("/products/" + id)).StatusCode == HttpStatusCode.OK, "Product details page missing.");
    var productCatalog = await admin.GetStringAsync("/products");
    Check(productCatalog.Contains("catalog-search") && productCatalog.Contains("catalog-category") &&
        !productCatalog.Contains("class=\"store-banner\""), "Product catalog must be separate from the homepage, with its own filters.");
    Check(home.Contains("href=\"/products\"") && !home.Contains("banner-title-frame"), "Product navigation or banner frame removal is incomplete.");
    var productDetails = await admin.GetStringAsync("/products/2");
    Check(!(await admin.GetStringAsync("/products/4")).Contains("product-purchase-layout") &&
        !(await admin.GetStringAsync("/batches")).Contains("LO-CAM-001"), "Hidden/harvested lots leaked into public listings.");
    Check(productDetails.Contains("product-purchase-layout") && productDetails.Contains("Các lô mùa vụ gợi ý") &&
        productDetails.Contains("product-quantity"), "Product purchase layout and recommendations are missing.");
    Check(home.Contains("account-trigger") && home.Contains("class=\"account-dropdown\" hidden"),
        "Guest sign-in choices must start inside a closed account dropdown.");
    var noToken = await admin.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>()));
    Check(noToken.StatusCode == HttpStatusCode.BadRequest, "Login accepted without CSRF token.");
    var setup = await Post(admin, "/account/setup", "/auth/setup", Registration("http-admin@example.test"));
    Check(setup.StatusCode == HttpStatusCode.Redirect, "Initial setup failed.");
    var rejectedAdminLogin = await Post(admin, "/account/login", "/auth/login", new()
    { ["email"] = "http-admin@example.test", ["password"] = password });
    Check(rejectedAdminLogin.Headers.Location?.ToString() == "/account/login?error=invalid",
        "Dealer sign-in must not authenticate an admin account.");
    var login = await Post(admin, "/admin/login", "/auth/admin/login", new()
    { ["email"] = "http-admin@example.test", ["password"] = password, ["returnUrl"] = "//outside.test" });
    Check(login.Headers.Location?.ToString() == "/admin", "Admin login or return URL guard failed.");
    foreach (var page in new[] { "/admin", "/admin/members", "/admin/products", "/admin/batches",
        "/admin/orders", "/admin/orders/1", "/admin/dealers", "/admin/revenue", "/admin/allocations/1", "/admin/banner", "/admin/integrations" })
        Check((await admin.GetAsync(page)).StatusCode == HttpStatusCode.OK, "Admin page failed: " + page);
    var dashboard = await admin.GetStringAsync("/admin");
    var allocationOrders = await admin.GetStringAsync("/admin/allocations/1");
    Check(allocationOrders.Contains("batch-orders-heading") && allocationOrders.Contains("PRE-202610-001") &&
        allocationOrders.Contains("PRE-202610-002") && allocationOrders.Contains("PRE-202610-003"),
        "Admin cannot see orders before saving allocation.");
    Check(Regex.IsMatch(dashboard, "<a[^>]*href=\"/\"[^>]*target=\"_blank\""), "Storefront link must open a separate tab.");
    var adminStorefront = await admin.GetStringAsync("/");
    Check(adminStorefront.Contains("href=\"/account/login\"") && adminStorefront.Contains("href=\"/account/register\"") &&
        !adminStorefront.Contains("href=\"/admin\""), "Admin session must still see dealer entry links on storefront.");
    Check((await admin.GetAsync("/admin")).StatusCode == HttpStatusCode.OK,
        "Viewing storefront must not sign out the admin.");
    Check(!(await admin.GetStringAsync("/account/setup")).Contains("action=\"/auth/setup\""), "Setup remains available.");
    using var dealer = Browser();
    var registration = await Post(dealer, "/account/register", "/auth/register", Registration("http-dealer@example.test"));
    Check(registration.StatusCode == HttpStatusCode.Redirect, "HTTP registration failed.");
    var dealerLogin = await Post(dealer, "/account/login", "/auth/login", new()
    { ["email"] = "http-dealer@example.test", ["password"] = password });
    Check(dealerLogin.Headers.Location?.ToString() == "/batches", "Dealer login failed.");
    var dealerStorefront = await dealer.GetStringAsync("/");
    Check(dealerStorefront.Contains("href=\"/my-orders\"") && !dealerStorefront.Contains("href=\"/admin\""),
        "Dealer storefront must preserve order navigation without an admin link.");
    Check(dealerStorefront.Contains("account-avatar") && dealerStorefront.Contains("HTTP Test"),
        "Signed-in dealer must have an initial avatar and account menu.");
    var rejectedDealerAdminLogin = await Post(dealer, "/admin/login", "/auth/admin/login", new()
    { ["email"] = "http-dealer@example.test", ["password"] = password });
    Check(rejectedDealerAdminLogin.Headers.Location?.ToString() == "/admin/login?error=invalid",
        "Admin entry must reject dealer credentials.");
    var denied = await dealer.GetAsync("/admin");
    Check(denied.StatusCode == HttpStatusCode.Redirect &&
        denied.Headers.Location!.ToString().StartsWith("/admin/login?switchAccount=true&returnUrl=", StringComparison.Ordinal) &&
        Uri.UnescapeDataString(denied.Headers.Location.ToString()).EndsWith("returnUrl=/admin"),
        "Dealer must be prompted to sign in as admin without accessing the protected page.");
    var bannerDenied = await dealer.GetAsync("/admin/banner");
    Check(bannerDenied.StatusCode == HttpStatusCode.Redirect &&
        Uri.UnescapeDataString(bannerDenied.Headers.Location!.ToString()) == "/admin/login?switchAccount=true&returnUrl=/admin/banner",
        "Admin sign-in must preserve the protected destination.");
    var adminSignIn = await dealer.GetStringAsync(denied.Headers.Location!);
    Check(adminSignIn.Contains("action=\"/auth/admin/login\"") && adminSignIn.Contains("name=\"returnUrl\" value=\"/admin\""),
        "Admin sign-in did not render correctly for an authenticated dealer.");
    foreach (var page in new[] { "/cart", "/checkout", "/my-orders", "/my-allocations", "/my-notifications", "/batches/2" })
        Check((await dealer.GetAsync(page)).StatusCode == HttpStatusCode.OK, "Dealer page failed: " + page);
    var switchToAdmin = await Post(dealer, "/admin/login?returnUrl=%2Fadmin%2Fbanner", "/auth/admin/login", new()
    { ["email"] = "http-admin@example.test", ["password"] = password, ["returnUrl"] = "/admin/banner" });
    Check(switchToAdmin.Headers.Location?.ToString() == "/admin/banner" &&
        (await dealer.GetAsync("/admin/banner")).StatusCode == HttpStatusCode.OK,
        "Valid admin credentials did not switch the dealer session and restore the requested admin page.");
    var adminDealerOnly = await dealer.GetAsync("/my-orders");
    Check(adminDealerOnly.StatusCode == HttpStatusCode.Redirect &&
        adminDealerOnly.Headers.Location!.ToString().Contains("/access-denied"), "Admin role gained unintended dealer access.");
    var logout = await Post(dealer, "/admin", "/auth/logout", new());
    Check(logout.StatusCode == HttpStatusCode.Redirect &&
        (await dealer.GetAsync("/my-orders")).StatusCode == HttpStatusCode.Redirect, "Logout did not clear access.");
}
finally
{
    if (!server.HasExited) server.Kill(entireProcessTree: true);
    await server.WaitForExitAsync();
    var errors = await stderr;
    var output = await stdout;
    if (errors.Length > 0) Console.WriteLine(errors);
    if (output.Contains("fail:")) Console.WriteLine(output.Length > 12000 ? output[^12000..] : output);
}

if (args.Contains("--sql"))
{
    using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(web, "appsettings.Development.json")));
    var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString()!;
    var sql = new SqlDataAccess(connection);
    checks += await AdminLifecycleSqlChecks.RunAsync(sql);
    var batches = (await new SqlHarvestBatchRepository(sql).GetAllBatchesAsync()).ToList();
    var preOrders = new SqlPreOrderRepository(sql);
    var notificationRepository = new SqlDealerNotificationRepository(sql);
    var allocationTestBatch = batches.FirstOrDefault(b => b.TotalOrderedQuantity > 0);
    if (allocationTestBatch != null)
    {
        var testLines = (await preOrders.GetLinesByBatchIdAsync(allocationTestBatch.HarvestBatchId))
            .Where(l => l.PreOrder?.Status != PreOrderStatus.Cancelled).ToList();
        var testActual = decimal.Round(testLines.Sum(l => l.Quantity) / 2, 2);
        var resultItems = allocation.CalculateAllocation(allocationTestBatch, testLines, testActual);
        var testAllocations = resultItems.Select(r => new Allocation { HarvestBatchId = allocationTestBatch.HarvestBatchId,
            PreOrderLineId = r.PreOrderLineId, RequestedQuantity = r.RequestedQuantity, AllocatedQuantity = r.AllocatedQuantity, Reason = r.Note }).ToList();
        var testNotices = testLines.Select(l => new DealerNotification { DealerId = l.PreOrder!.DealerId,
            HarvestBatchId = allocationTestBatch.HarvestBatchId, PreOrderLineId = l.PreOrderLineId,
            Subject = "SQL transaction verification", Body = "Temporary notice rolled back by regression check." }).ToList();
        var originalNoticeCount = (await notificationRepository.GetByDealerAsync(testNotices[0].DealerId)).Count();
        using (var notificationConnection = sql.CreateConnection())
        {
            await notificationConnection.OpenAsync();
            using var notificationTransaction = notificationConnection.BeginTransaction();
            var transactionalNotifications = new TransactionalSqlDataAccess(notificationConnection, notificationTransaction);
            await SqlAllocationRepository.SaveWithinTransactionAsync(notificationConnection, notificationTransaction,
                allocationTestBatch.HarvestBatchId, testAllocations, testActual, DateTime.Now, testNotices);
            var batchCheck = await transactionalNotifications.QuerySingleOrDefaultAsync<HarvestBatch, object>(
                "SELECT * FROM HarvestBatches WHERE HarvestBatchId=@Id", new { Id = allocationTestBatch.HarvestBatchId });
            Check(batchCheck?.ActualQuantity == testActual && batchCheck.Status == HarvestBatchStatus.Allocated,
                "SQL allocation transaction did not persist the batch state.");
            var noticesCheck = (await new SqlDealerNotificationRepository(transactionalNotifications).GetByDealerAsync(testNotices[0].DealerId)).ToList();
            Check(noticesCheck.Any(n => n.Subject == "SQL transaction verification"), "SQL allocation transaction did not persist dealer notices.");
            Check(await new SqlDealerNotificationRepository(transactionalNotifications).GetUnreadCountAsync(testNotices[0].DealerId) > 0,
                "SQL unread notification mapping failed.");
            var testNotice = noticesCheck.First(n => n.Subject == "SQL transaction verification");
            Check(!await new SqlDealerNotificationRepository(transactionalNotifications).MarkReadAsync(testNotice.DealerNotificationId, -1),
                "SQL read marking did not enforce dealer ownership.");
            await notificationTransaction.RollbackAsync();
        }
        var originalBatchCheck = await new SqlHarvestBatchRepository(sql).GetBatchByIdAsync(allocationTestBatch.HarvestBatchId);
        Check(originalBatchCheck?.ActualQuantity == allocationTestBatch.ActualQuantity && originalBatchCheck?.Status == allocationTestBatch.Status &&
            (await notificationRepository.GetByDealerAsync(testNotices[0].DealerId)).Count() == originalNoticeCount,
            "SQL allocation/notification transaction did not fully roll back.");
    }
    foreach (var batch in batches)
    {
        Check(batch.Member != null && batch.Product != null, "SQL navigation mapping failed.");
        Check(batch.TotalOrderedQuantity == await preOrders.GetTotalOrderedQuantityForBatchAsync(batch.HarvestBatchId), "SQL ordered quantity mapping failed.");
        await preOrders.GetLinesByBatchIdAsync(batch.HarvestBatchId);
        await new SqlAllocationRepository(sql).GetAllocationsByBatchIdAsync(batch.HarvestBatchId);
    }
    foreach (var o in await preOrders.GetAllPreOrdersAsync())
        Check(o.PreOrderLines.Count > 0 && o.PreOrderLines.All(l => l.HarvestBatch != null), "SQL order details missing.");
    await new SqlUserAccountRepository(sql).HasAdminAsync();
    await new SqlRevenueRepository(sql).GetMemberRevenueAsync(null, null);
    Check(!string.IsNullOrWhiteSpace((await new SqlHomeBannerRepository(sql).GetAsync()).Title), "SQL banner loading failed.");
    var originalBanner = await new SqlHomeBannerRepository(sql).GetAsync();
    using (var bannerConnection = sql.CreateConnection())
    {
        await bannerConnection.OpenAsync();
        using var bannerTransaction = bannerConnection.BeginTransaction();
        var transactionalDb = new TransactionalSqlDataAccess(bannerConnection, bannerTransaction);
        var bannerToSave = new HomeBanner { Title = "Banner transaction regression check" };
        await new SqlHomeBannerRepository(transactionalDb).SaveAsync(bannerToSave);
        Check((await new SqlHomeBannerRepository(transactionalDb).GetAsync()).Title == bannerToSave.Title,
            "SQL banner edits were not available to a new repository instance.");
        await bannerTransaction.RollbackAsync();
    }
    Check((await new SqlHomeBannerRepository(sql).GetAsync()).Title == originalBanner.Title,
        "Banner test did not roll back its settings.");
    Console.WriteLine("SQL checks passed; allocation, notification and banner test changes rolled back; no business data changes committed.");
}
Console.WriteLine($"Passed {checks} regression checks.");
