using System.Data;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Dapper;
using Microsoft.Data.SqlClient;
namespace AgriculturalCoop.DataStore.Sql;

public sealed class SqlDealerNotificationRepository(ISqlDataAccess db) : IDealerNotificationRepository
{
    private const string OwnNotices = @" FROM DealerNotifications n
        INNER JOIN PreOrderLines l ON l.PreOrderLineId=n.PreOrderLineId
        INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId
        WHERE n.DealerId=@DealerId AND o.DealerId=@DealerId AND o.Status<>4";
    public Task<IEnumerable<DealerNotification>> GetByDealerAsync(int dealerId) =>
        db.QueryAsync<DealerNotification, object>("SELECT n.*" + OwnNotices + " ORDER BY n.UpdatedAt DESC, n.DealerNotificationId DESC", new { DealerId = dealerId });
    public Task<int> GetUnreadCountAsync(int dealerId) =>
        db.QuerySingleOrDefaultAsync<int, object>("SELECT COUNT(*)" + OwnNotices + " AND n.ReadAt IS NULL AND n.IsSuperseded=0", new { DealerId = dealerId });
    public async Task<bool> MarkReadAsync(int notificationId, int dealerId) =>
        await db.ExecuteAsync(@"UPDATE n SET ReadAt=COALESCE(n.ReadAt,SYSDATETIME())
            FROM DealerNotifications n INNER JOIN PreOrderLines l ON l.PreOrderLineId=n.PreOrderLineId
            INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId
            WHERE n.DealerNotificationId=@Id AND n.DealerId=@DealerId AND o.DealerId=@DealerId AND o.Status<>4",
            new { Id = notificationId, DealerId = dealerId }) == 1;

    public async Task SaveReviewedAsync(DealerNotification notification)
    {
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction(IsolationLevel.Serializable);
        var valid = await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM PreOrderLines l
            INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId INNER JOIN Allocations a ON a.PreOrderLineId=l.PreOrderLineId
            WHERE l.PreOrderLineId=@PreOrderLineId AND l.HarvestBatchId=@HarvestBatchId AND o.DealerId=@DealerId AND o.Status<>4", notification, transaction);
        if (valid != 1) throw new PreOrderDomainException("Kết quả phân bổ hoặc đại lý đã thay đổi. Hãy tải lại.");
        await UpsertAsync(conn, transaction, notification);
        transaction.Commit();
    }

    internal static Task UpsertAsync(SqlConnection conn, SqlTransaction transaction, DealerNotification n) => conn.ExecuteAsync(@"
        UPDATE DealerNotifications WITH (UPDLOCK,HOLDLOCK)
        SET ReadAt=CASE WHEN Body<>@Body OR Subject<>@Subject OR IsSuperseded=1 THEN NULL ELSE ReadAt END,
            UpdatedAt=CASE WHEN Body<>@Body OR Subject<>@Subject OR IsSuperseded=1 THEN @UpdatedAt ELSE UpdatedAt END,
            Subject=@Subject,Body=@Body,DealerId=@DealerId,IsAiGenerated=@IsAiGenerated,IsSuperseded=0
        WHERE PreOrderLineId=@PreOrderLineId AND Kind=@Kind;
        IF @@ROWCOUNT=0
            INSERT INTO DealerNotifications (DealerId,HarvestBatchId,PreOrderLineId,Kind,Subject,Body,IsAiGenerated,IsSuperseded,UpdatedAt)
            VALUES (@DealerId,@HarvestBatchId,@PreOrderLineId,@Kind,@Subject,@Body,@IsAiGenerated,0,@UpdatedAt);", n, transaction);
}
