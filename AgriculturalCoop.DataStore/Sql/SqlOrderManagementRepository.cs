using System.Data;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Dapper;
using Microsoft.Data.SqlClient;
namespace AgriculturalCoop.DataStore.Sql;

public sealed class SqlOrderManagementRepository(ISqlDataAccess db) : IOrderManagementRepository
{
    public Task<bool> ConfirmOrderAsync(int orderId) => ChangeAsync(orderId, 1, null);
    public Task<bool> CancelOrderAsync(int orderId, string reason) => ChangeAsync(orderId, 4, reason);
    private async Task<bool> ChangeAsync(int orderId, int status, string? reason)
    {
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction(IsolationLevel.Serializable);
        var changed = await ChangeWithinTransactionAsync(conn, transaction, orderId, status, reason);
        transaction.Commit();
        return changed;
    }
    internal static async Task<bool> ChangeWithinTransactionAsync(SqlConnection conn, SqlTransaction transaction, int orderId, int status, string? reason)
    {
        // Lock the same batches as checkout/allocation before changing an order.
        await conn.QueryAsync<int>(@"SELECT b.HarvestBatchId FROM HarvestBatches b WITH (UPDLOCK,HOLDLOCK)
            WHERE EXISTS(SELECT 1 FROM PreOrderLines l WHERE l.HarvestBatchId=b.HarvestBatchId AND l.PreOrderId=@Id)
            ORDER BY b.HarvestBatchId", new { Id = orderId }, transaction);
        return await conn.ExecuteAsync(@"UPDATE o SET Status=@Status,CancellationReason=@Reason FROM PreOrders o
            JOIN Dealers d ON d.DealerId=o.DealerId WHERE o.PreOrderId=@Id
            AND ((@Status=1 AND o.Status=0 AND d.IsActive=1) OR (@Status=4 AND o.Status IN(0,1)))
            AND EXISTS(SELECT 1 FROM PreOrderLines WHERE PreOrderId=@Id)
            AND NOT EXISTS(SELECT 1 FROM PreOrderLines l JOIN HarvestBatches b ON b.HarvestBatchId=l.HarvestBatchId
                LEFT JOIN Allocations a ON a.PreOrderLineId=l.PreOrderLineId
                WHERE l.PreOrderId=@Id AND (a.AllocationId IS NOT NULL OR b.Status>=3))",
            new { Id = orderId, Status = status, Reason = reason }, transaction) == 1;
    }
}