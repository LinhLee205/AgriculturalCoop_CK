using AgriculturalCoop.WebApp.Security;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.CoreBusiness.Services;
using AgriculturalCoop.DataStore.InMemory;
using AgriculturalCoop.UseCases.Accounts;
using AgriculturalCoop.UseCases.Allocations;
using AgriculturalCoop.UseCases.Management;
using AgriculturalCoop.UseCases.PreOrders;
using AgriculturalCoop.WebApp.Services;

internal static class AdminLifecycleChecks
{
    public static async Task<int> RunAsync()
    {
        var checks = 0;
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
        async Task Reject(Func<Task> action)
        {
            try { await action(); } catch (PreOrderDomainException) { checks++; return; }
            throw new Exception("Expected lifecycle rule rejection.");
        }
        var store = new InMemoryDataStore();
        var auth = new TestAdminAuthorization();
        var manageOrders = new ManageOrdersUseCase(store, auth);
        var batches = new ManageBatchesUseCase(store);
        var create = new CreatePreOrderUseCase(store, store, store);
        PreOrder Order(params (int Id, decimal Qty)[] entries) => new() { DealerId = 1, RecipientName = "Người nhận",
            ContactPhone = "0901234567", DeliveryAddress = "Địa chỉ nhận hàng",
            PreOrderLines = entries.Select(e => new PreOrderLine { HarvestBatchId = e.Id, Quantity = e.Qty }).ToList() };
        var order = Order((2, 10));
        await create.ExecuteAsync(order);
        auth.Allowed = false;
        await Reject(() => manageOrders.CancelAsync(order.PreOrderId, "Yêu cầu hủy"));
        auth.Allowed = true;
        await Reject(() => manageOrders.CancelAsync(order.PreOrderId, "  "));
        await manageOrders.CancelAsync(order.PreOrderId, "Đại lý yêu cầu hủy");
        Check(order.Status == PreOrderStatus.Cancelled && order.CancellationReason == "Đại lý yêu cầu hủy" &&
            await store.GetTotalOrderedQuantityForBatchAsync(2) == 0, "Cancelling an unallocated order did not release its quantity.");
        Check((await store.GetBatchByIdAsync(2))!.LinkedOrderCount == 1, "Cancelled order lost its historical batch relationship.");
        await Reject(() => batches.DeleteAsync(2));
        await batches.SetArchivedAsync(2, true);
        Check(!(await store.GetOpenBatchesAsync()).Any(b => b.HarvestBatchId == 2), "Archived batch remains public.");
        await Reject(() => create.ValidateAsync(Order((2, 1))));
        await Reject(() => batches.SetOpenAsync(2, true));
        await batches.SetArchivedAsync(2, false);
        Check((await store.GetBatchByIdAsync(2))!.Status == HarvestBatchStatus.Planned, "Restoring a batch silently reopened preorder.");
        await batches.SetOpenAsync(2, true);
        var multi = Order((2, 5), (3, 5));
        multi.Status = PreOrderStatus.Pending;
        await create.ExecuteAsync(multi);
        await manageOrders.ConfirmAsync(multi.PreOrderId);
        Check(multi.Status == PreOrderStatus.Confirmed, "Pending order confirmation failed.");
        var allocate = new AllocateHarvestUseCase(store, store, store, new AllocationService());
        await allocate.ExecuteAsync(2, 5);
        Check(multi.Status == PreOrderStatus.PartiallyAllocated, "Order with an unallocated batch was incorrectly fully allocated.");
        await Reject(() => manageOrders.CancelAsync(multi.PreOrderId, "Không được hủy sau phân bổ"));
        await allocate.ExecuteAsync(3, 5);
        Check(multi.Status == PreOrderStatus.FullyAllocated, "Order did not become fully allocated after every line received its quantity.");
        await batches.CompleteAsync(2);
        await Reject(() => allocate.ExecuteAsync(2, 1));
        await batches.SetArchivedAsync(2, true);
        Check((await store.GetPreOrderByIdAsync(multi.PreOrderId))?.PreOrderLines.Count == 2 &&
            (await store.GetAllocationsByBatchIdAsync(2)).Count() == 1, "Archiving removed order/allocation history.");
        await batches.SetArchivedAsync(2, false);
        Check((await store.GetBatchByIdAsync(2))!.Status == HarvestBatchStatus.Completed, "Restore changed completed batch state.");
        var accounts = new AccountUseCases(store, new PasswordService(), store);
        await accounts.RegisterAsync("lifecycle@example.test", "Đại lý kiểm tra", "0901234567", "Lifecycle12345", "Lifecycle12345");
        var account = (await accounts.LoginAsync("lifecycle@example.test", "Lifecycle12345"))!;
        var dealer = (await store.GetDealerByIdAsync(account.DealerId!.Value))!;
        var management = new ManageDealersUseCase(store, auth);
        dealer.IsActive = false;
        await management.SaveAsync(dealer);
        Check(await accounts.LoginAsync(account.Email, "Lifecycle12345") == null, "Inactive dealer can still sign in.");
        var blocked = Order((3, 1)); blocked.DealerId = dealer.DealerId;
        await Reject(() => create.ExecuteAsync(blocked));
        await Reject(() => management.DeleteAsync(dealer.DealerId));
        dealer.IsActive = true; await management.SaveAsync(dealer);
        Check(await accounts.LoginAsync(account.Email, "Lifecycle12345") != null, "Reactivated dealer cannot sign in.");
        var offline = new Dealer { DealerName = "Hồ sơ chưa giao dịch", Phone = "0901234567" };
        await management.SaveAsync(offline); await management.DeleteAsync(offline.DealerId);
        Check(await store.GetDealerByIdAsync(offline.DealerId) == null, "An empty dealer profile cannot be deleted.");
        return checks;
    }
}
