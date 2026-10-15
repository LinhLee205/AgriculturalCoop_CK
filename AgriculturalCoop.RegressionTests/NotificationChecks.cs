using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.CoreBusiness.Services;
using AgriculturalCoop.DataStore.InMemory;
using AgriculturalCoop.UseCases.AI;
using AgriculturalCoop.UseCases.Allocations;
using AgriculturalCoop.UseCases.PluginInterfaces;

internal static class NotificationChecks
{
    public static async Task<int> RunAsync()
    {
        var checks = 0;
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
        async Task Reject(Func<Task> action)
        {
            try { await action(); } catch (PreOrderDomainException) { checks++; return; }
            throw new Exception("Expected notification access or approval rejection.");
        }
        var store = new InMemoryDataStore();
        var allocate = new AllocateHarvestUseCase(store, store, store, new AllocationService());
        await allocate.ExecuteAsync(1, 720);
        var identity = new DealerIdentity { DealerId = 1 };
        var inbox = new DealerNotificationUseCases(store, identity);
        var first = (await inbox.ListAsync()).Single();
        Check(first.DealerId == 1 && first.Kind == 0 && first.Body.Contains("320,00") && !first.IsAiGenerated,
            "Allocation did not create an accurate deterministic notification for each dealer.");
        Check(await inbox.UnreadCountAsync() == 1, "Unread count missing.");
        await inbox.MarkReadAsync(first.DealerNotificationId);
        Check(await inbox.UnreadCountAsync() == 0, "Read status was not persisted.");
        identity.DealerId = 2;
        Check((await inbox.ListAsync()).All(n => n.DealerId == 2), "Dealer can read another dealer's notification.");
        await Reject(() => inbox.MarkReadAsync(first.DealerNotificationId));
        identity.DealerId = 1;
        var auth = new TestAdminAuthorization();
        var data = new AllocationNoticeDataReader(store, store, store, store);
        var generate = new GenerateAllocationNoticeUseCase(data, new TestNoticeWriter(), auth);
        var publish = new PublishAllocationNoticeUseCase(data, store, auth);
        var draft = await generate.ExecuteAsync(1, first.PreOrderLineId, "Đã kiểm tra sản lượng.", true);
        Check((await inbox.ListAsync()).Count() == 1, "AI draft was written before approval.");
        await Reject(() => publish.ExecuteAsync(draft, false));
        auth.Allowed = false;
        await Reject(() => publish.ExecuteAsync(draft, true));
        auth.Allowed = true;
        var dealer = (await store.GetDealerByIdAsync(1))!;
        dealer.Email = "";
        draft = await generate.ExecuteAsync(1, first.PreOrderLineId, "Đã kiểm tra sản lượng.", true);
        await publish.ExecuteAsync(draft, true);
        Check((await inbox.ListAsync()).Any(n => n.Kind == 1 && n.IsAiGenerated), "Approved AI supplement did not publish without email/SMTP.");
        await allocate.ExecuteAsync(1, 720);
        Check((await inbox.ListAsync()).Count(n => n.Kind == 0) == 1 && first.ReadAt != null, "Repeated allocation duplicated an unchanged result notification.");
        await allocate.ExecuteAsync(1, 360);
        Check((await inbox.ListAsync()).Single(n => n.Kind == 0).Body.Contains("160,00") && first.ReadAt == null,
            "Reallocation did not update and mark the changed result unread.");
        Check((await inbox.ListAsync()).Single(n => n.Kind == 1).IsSuperseded, "Old AI supplement was not identified after reallocation.");
        await Reject(() => publish.ExecuteAsync(draft, true));
        return checks;
    }

    private sealed class DealerIdentity : IDealerIdentity
    {
        public int DealerId { get; set; }
        public Task<int> GetDealerIdAsync() => Task.FromResult(DealerId);
    }
}
