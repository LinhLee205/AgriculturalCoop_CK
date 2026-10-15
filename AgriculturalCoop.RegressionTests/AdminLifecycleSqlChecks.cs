using System.Data;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.DataStore.Sql;
using Dapper;

internal static class AdminLifecycleSqlChecks
{
    public static async Task<int> RunAsync(SqlDataAccess db)
    {
        var checks = 0;
        void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction(IsolationLevel.Serializable);
        var testDb = new TransactionalSqlDataAccess(conn, tx);
        var ids = await conn.QuerySingleAsync<(int MemberId, int ProductId)>(
            "SELECT (SELECT TOP(1) MemberId FROM Members ORDER BY MemberId) MemberId,(SELECT TOP(1) ProductId FROM Products ORDER BY ProductId) ProductId", transaction: tx);
        var dealerId = await conn.ExecuteScalarAsync<int>(@"INSERT INTO Dealers(DealerName,ContactPerson,Phone,Address,Email,Tier,IsActive)
            OUTPUT INSERTED.DealerId VALUES(N'Test rollback',N'Test','0901234567',N'Test','',N'Test',1)", transaction: tx);
        var batchId = await conn.ExecuteScalarAsync<int>(@"INSERT INTO HarvestBatches(BatchCode,MemberId,ProductId,ExpectedQuantity,ExpectedHarvestDate,UnitPrice,Status,IsArchived)
            OUTPUT INSERTED.HarvestBatchId VALUES(@Code,@MemberId,@ProductId,20,SYSDATETIME(),1000,1,0)",
            new { Code = "CHECK-" + Guid.NewGuid().ToString("N"), ids.MemberId, ids.ProductId }, tx);
        var orderId = await conn.ExecuteScalarAsync<int>(@"INSERT INTO PreOrders(OrderNumber,DealerId,OrderDate,Status,TotalAmount,RecipientName,ContactPhone,DeliveryAddress)
            OUTPUT INSERTED.PreOrderId VALUES(@Code,@DealerId,SYSDATETIME(),0,10000,N'Test','0901234567',N'Test')",
            new { Code = "CHECK-" + Guid.NewGuid().ToString("N"), DealerId = dealerId }, tx);
        var lineId = await conn.ExecuteScalarAsync<int>(@"INSERT INTO PreOrderLines(PreOrderId,HarvestBatchId,Quantity,UnitPrice)
            OUTPUT INSERTED.PreOrderLineId VALUES(@OrderId,@BatchId,10,1000)", new { OrderId = orderId, BatchId = batchId }, tx);
        Check(await SqlOrderManagementRepository.ChangeWithinTransactionAsync(conn, tx, orderId, 1, null), "SQL confirmation failed.");
        Check(await SqlOrderManagementRepository.ChangeWithinTransactionAsync(conn, tx, orderId, 4, "Kiểm tra hoàn tác"), "SQL cancellation failed.");
        Check(await new SqlPreOrderRepository(testDb).GetTotalOrderedQuantityForBatchAsync(batchId) == 0, "SQL cancelled quantity still consumes batch capacity.");
        Check(!await SqlOrderManagementRepository.ChangeWithinTransactionAsync(conn, tx, orderId, 4, "Lặp lại"), "SQL cancelled order was changed again.");
        await conn.ExecuteAsync("UPDATE PreOrders SET Status=1,CancellationReason=NULL WHERE PreOrderId=@Id", new { Id = orderId }, tx);
        await SqlAllocationRepository.SaveWithinTransactionAsync(conn, tx, batchId,
            [new Allocation { PreOrderLineId = lineId, HarvestBatchId = batchId, RequestedQuantity = 10, AllocatedQuantity = 5, Reason = "Kiểm tra" }],
            5, DateTime.Now, [new DealerNotification { DealerId = dealerId, PreOrderLineId = lineId, HarvestBatchId = batchId, Subject = "Kiểm tra", Body = "Dữ liệu kiểm tra được hoàn tác." }]);
        Check(await conn.ExecuteScalarAsync<int>("SELECT Status FROM PreOrders WHERE PreOrderId=@Id", new { Id = orderId }, tx) == 2,
            "SQL allocation did not update partial order status.");
        Check(!await SqlOrderManagementRepository.ChangeWithinTransactionAsync(conn, tx, orderId, 4, "Không được hủy"), "SQL allocated order could be cancelled.");
        var batchRepo = new SqlHarvestBatchRepository(testDb);
        Check(await batchRepo.CompleteBatchAsync(batchId), "SQL completed batch transition failed.");
        Check(await batchRepo.SetArchivedAsync(batchId, true), "SQL archiving failed.");
        var savedBatch = await conn.QuerySingleAsync<HarvestBatch>("SELECT * FROM HarvestBatches WHERE HarvestBatchId=@Id", new { Id = batchId }, tx);
        Check(savedBatch.IsArchived && savedBatch.Status == HarvestBatchStatus.Completed &&
            await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Allocations WHERE HarvestBatchId=@Id", new { Id = batchId }, tx) == 1, "SQL archive removed allocation history.");
        Check(await batchRepo.SetArchivedAsync(batchId, false), "SQL restore failed.");
        try
        {
            await SqlAllocationRepository.SaveWithinTransactionAsync(conn, tx, batchId, [], 0, DateTime.Now, []);
            throw new Exception("Completed batch could be allocated again in SQL.");
        }
        catch (PreOrderDomainException) { checks++; }
        var dealerRepo = new SqlDealerRepository(testDb);
        var dealer = (await dealerRepo.GetDealerByIdAsync(dealerId))!;
        dealer.IsActive = false;
        Check(await dealerRepo.UpdateDealerAsync(dealer) && (await dealerRepo.GetDealerByIdAsync(dealerId)) is { IsActive: false }, "SQL dealer status was not persisted.");
        await tx.RollbackAsync();
        Check(!(await new SqlDealerRepository(db).GetAllDealersAsync()).Any(d => d.DealerId == dealerId),
            "SQL lifecycle test did not roll back its temporary dealer.");
        return checks;
    }
}
