using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgriculturalCoop.DataStore.Sql
{
    public class SqlMemberRepository : IMemberRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlMemberRepository(ISqlDataAccess db) => _db = db;

        public Task<IEnumerable<Member>> GetAllMembersAsync() =>
            _db.QueryAsync<Member, dynamic>("SELECT * FROM Members ORDER BY MemberId", new { });

        public Task<Member?> GetMemberByIdAsync(int id) =>
            _db.QuerySingleOrDefaultAsync<Member, dynamic>("SELECT * FROM Members WHERE MemberId = @Id", new { Id = id });

        public Task AddMemberAsync(Member member) =>
            _db.ExecuteAsync(@"INSERT INTO Members (FullName, Phone, Address, Email, AreaHectare, JoinDate)
                               VALUES (@FullName, @Phone, @Address, @Email, @AreaHectare, @JoinDate)", member);

        public Task UpdateMemberAsync(Member member) =>
            _db.ExecuteAsync(@"UPDATE Members 
                               SET FullName = @FullName, Phone = @Phone, Address = @Address, 
                                   Email = @Email, AreaHectare = @AreaHectare 
                               WHERE MemberId = @MemberId", member);
        public async Task<bool> DeleteMemberAsync(int id)
        {
            try
            {
                return await _db.ExecuteAsync(
                    "DELETE FROM Members WHERE MemberId=@Id",
                    new { Id = id }) == 1;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return false;
            }
        }

    }

    public class SqlProductRepository : IProductRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlProductRepository(ISqlDataAccess db) => _db = db;

        public Task<IEnumerable<Product>> GetAllProductsAsync() =>
            _db.QueryAsync<Product, dynamic>("SELECT * FROM Products ORDER BY ProductId", new { });

        public Task<Product?> GetProductByIdAsync(int id) =>
            _db.QuerySingleOrDefaultAsync<Product, dynamic>("SELECT * FROM Products WHERE ProductId = @Id", new { Id = id });

        public Task AddProductAsync(Product product) =>
            _db.ExecuteAsync(@"INSERT INTO Products (ProductName, Category, Unit, StandardPrice, Description, ImageUrl)
                               VALUES (@ProductName, @Category, @Unit, @StandardPrice, @Description, @ImageUrl)", product);
        public Task UpdateProductAsync(Product product) =>
            _db.ExecuteAsync(@"
        UPDATE Products
        SET ProductName=@ProductName,
            Category=@Category,
            Unit=@Unit,
            StandardPrice=@StandardPrice,
            Description=@Description,
            ImageUrl=@ImageUrl
        WHERE ProductId=@ProductId", product);

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                return await _db.ExecuteAsync(
                    "DELETE FROM Products WHERE ProductId=@Id",
                    new { Id = id }) == 1;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return false;
            }
        }

    }

    public class SqlHarvestBatchRepository : IHarvestBatchRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlHarvestBatchRepository(ISqlDataAccess db) => _db = db;

        public async Task<IEnumerable<HarvestBatch>> GetAllBatchesAsync()
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT b.*, COALESCE((SELECT SUM(l.Quantity) FROM PreOrderLines l INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId WHERE l.HarvestBatchId = b.HarvestBatchId AND o.Status<>4), 0) AS TotalOrderedQuantity, (SELECT COUNT(DISTINCT l.PreOrderId) FROM PreOrderLines l WHERE l.HarvestBatchId=b.HarvestBatchId) AS LinkedOrderCount, m.*, p.*
                FROM HarvestBatches b
                INNER JOIN Members m ON b.MemberId = m.MemberId
                INNER JOIN Products p ON b.ProductId = p.ProductId
                ORDER BY b.HarvestBatchId DESC";

            var batches = await conn.QueryAsync<HarvestBatch, Member, Product, HarvestBatch>(
                sql,
                (batch, member, product) =>
                {
                    batch.Member = member;
                    batch.Product = product;
                    return batch;
                },
                splitOn: "MemberId,ProductId");

            return batches;
        }

        public async Task<HarvestBatch?> GetBatchByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT b.*, COALESCE((SELECT SUM(l.Quantity) FROM PreOrderLines l INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId WHERE l.HarvestBatchId = b.HarvestBatchId AND o.Status<>4), 0) AS TotalOrderedQuantity, (SELECT COUNT(DISTINCT l.PreOrderId) FROM PreOrderLines l WHERE l.HarvestBatchId=b.HarvestBatchId) AS LinkedOrderCount, m.*, p.*
                FROM HarvestBatches b
                INNER JOIN Members m ON b.MemberId = m.MemberId
                INNER JOIN Products p ON b.ProductId = p.ProductId
                WHERE b.HarvestBatchId = @Id";

            var batches = await conn.QueryAsync<HarvestBatch, Member, Product, HarvestBatch>(
                sql,
                (batch, member, product) =>
                {
                    batch.Member = member;
                    batch.Product = product;
                    return batch;
                },
                new { Id = id },
                splitOn: "MemberId,ProductId");

            return batches.FirstOrDefault();
        }

        public async Task<IEnumerable<HarvestBatch>> GetOpenBatchesAsync()
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT b.*, COALESCE((SELECT SUM(l.Quantity) FROM PreOrderLines l INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId WHERE l.HarvestBatchId = b.HarvestBatchId AND o.Status<>4), 0) AS TotalOrderedQuantity, (SELECT COUNT(DISTINCT l.PreOrderId) FROM PreOrderLines l WHERE l.HarvestBatchId=b.HarvestBatchId) AS LinkedOrderCount, m.*, p.*
                FROM HarvestBatches b
                INNER JOIN Members m ON b.MemberId = m.MemberId
                INNER JOIN Products p ON b.ProductId = p.ProductId
                WHERE b.Status = @OpenStatus AND b.IsArchived=0
                ORDER BY b.ExpectedHarvestDate ASC";

            var batches = await conn.QueryAsync<HarvestBatch, Member, Product, HarvestBatch>(
                sql,
                (batch, member, product) =>
                {
                    batch.Member = member;
                    batch.Product = product;
                    return batch;
                },
                new { OpenStatus = (int)HarvestBatchStatus.OpenForPreOrder },
                splitOn: "MemberId,ProductId");

            return batches;
        }

        public async Task AddBatchAsync(HarvestBatch batch)
        {
            try
            {
                await _db.ExecuteAsync(@"
            INSERT INTO HarvestBatches
                (BatchCode,MemberId,ProductId,ExpectedQuantity,
                 ActualQuantity,ExpectedHarvestDate,
                 ActualHarvestDate,UnitPrice,Status)
            VALUES
                (@BatchCode,@MemberId,@ProductId,@ExpectedQuantity,
                 @ActualQuantity,@ExpectedHarvestDate,
                 @ActualHarvestDate,@UnitPrice,@Status)", batch);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                    "Mã lô đã được sử dụng.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                    "Xã viên hoặc nông sản đã thay đổi. Vui lòng tải lại danh mục.");
            }
        }

        public async Task UpdateBatchAsync(HarvestBatch batch)
        {
            try
            {
                var count = await _db.ExecuteAsync(@"
            UPDATE HarvestBatches
            SET BatchCode=@BatchCode,
                MemberId=@MemberId,
                ProductId=@ProductId,
                ExpectedQuantity=@ExpectedQuantity,
                ExpectedHarvestDate=@ExpectedHarvestDate,
                UnitPrice=@UnitPrice
            WHERE HarvestBatchId=@HarvestBatchId
              AND Status < 2 AND IsArchived=0
              AND @ExpectedQuantity >= COALESCE(
                  (SELECT SUM(l.Quantity)
                   FROM PreOrderLines l INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId
                   WHERE l.HarvestBatchId=@HarvestBatchId AND o.Status<>4),0)
              AND (
                  NOT EXISTS(
                      SELECT 1 FROM PreOrderLines
                      WHERE HarvestBatchId=@HarvestBatchId)
                  OR (
                      MemberId=@MemberId
                      AND ProductId=@ProductId
                      AND UnitPrice=@UnitPrice
                  )
              )", batch);

                if (count != 1)
                    throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                        "Lô đã thay đổi hoặc đã chốt giao dịch. Vui lòng tải lại.");
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                    "Mã lô đã được sử dụng.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                    "Xã viên hoặc nông sản đã thay đổi. Vui lòng tải lại danh mục.");
            }
        }

        public async Task<bool> DeleteBatchAsync(int id)
        {
            try
            {
                return await _db.ExecuteAsync(@"
            DELETE FROM HarvestBatches
            WHERE HarvestBatchId=@Id AND Status < 2",
                    new { Id = id }) == 1;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return false;
            }
        }

        public async Task UpdateBatchYieldAsync(int batchId, decimal actualYield, DateTime harvestDate)
        {
            var count = await _db.ExecuteAsync(@"UPDATE HarvestBatches
                SET ActualQuantity=@ActualYield,ActualHarvestDate=@HarvestDate,Status=2
                WHERE HarvestBatchId=@BatchId AND Status<3 AND IsArchived=0",
                new { BatchId = batchId, ActualYield = actualYield, HarvestDate = harvestDate });
            if (count != 1) throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                "Lô đã lưu trữ, phân bổ hoặc hoàn tất. Vui lòng tải lại trước khi sửa sản lượng.");
        }

        public Task UpdateBatchStatusAsync(int batchId, HarvestBatchStatus status) =>
            _db.ExecuteAsync(@"UPDATE HarvestBatches SET Status = @Status WHERE HarvestBatchId = @BatchId",
                             new { BatchId = batchId, Status = (int)status });

        public async Task<bool> SetPreOrderAvailabilityAsync(int batchId, bool isOpen) =>
            await _db.ExecuteAsync(@"UPDATE HarvestBatches SET Status=@Status
                WHERE HarvestBatchId=@BatchId AND Status IN (0,1) AND ActualQuantity IS NULL AND IsArchived=0",
                new { BatchId = batchId, Status = (int)(isOpen ? HarvestBatchStatus.OpenForPreOrder : HarvestBatchStatus.Planned) }) == 1;

        public async Task<bool> SetArchivedAsync(int batchId, bool archived) =>
            await _db.ExecuteAsync(@"UPDATE HarvestBatches SET IsArchived=@Archived,
                Status=CASE WHEN @Archived=1 AND Status=1 THEN 0 ELSE Status END WHERE HarvestBatchId=@Id",
                new { Id = batchId, Archived = archived }) == 1;
        public async Task<bool> CompleteBatchAsync(int batchId) =>
            await _db.ExecuteAsync(@"UPDATE HarvestBatches SET Status=4 WHERE HarvestBatchId=@Id AND Status=3 AND IsArchived=0
                AND EXISTS(SELECT 1 FROM Allocations WHERE HarvestBatchId=@Id)
                AND NOT EXISTS(SELECT 1 FROM PreOrderLines l JOIN PreOrders o ON o.PreOrderId=l.PreOrderId
                    LEFT JOIN Allocations a ON a.PreOrderLineId=l.PreOrderLineId WHERE l.HarvestBatchId=@Id AND o.Status<>4 AND a.AllocationId IS NULL)",
                new { Id = batchId }) == 1;
    }

    public class SqlDealerRepository : IDealerRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlDealerRepository(ISqlDataAccess db) => _db = db;

        public Task<IEnumerable<Dealer>> GetAllDealersAsync() =>
            _db.QueryAsync<Dealer, dynamic>("SELECT d.*, CAST(CASE WHEN EXISTS(SELECT 1 FROM UserAccounts u WHERE u.DealerId=d.DealerId) THEN 1 ELSE 0 END AS BIT) HasAccount FROM Dealers d ORDER BY DealerId", new { });

        public Task<Dealer?> GetDealerByIdAsync(int id) =>
            _db.QuerySingleOrDefaultAsync<Dealer, dynamic>("SELECT d.*, CAST(CASE WHEN EXISTS(SELECT 1 FROM UserAccounts u WHERE u.DealerId=d.DealerId) THEN 1 ELSE 0 END AS BIT) HasAccount FROM Dealers d WHERE DealerId = @Id", new { Id = id });

        public Task AddDealerAsync(Dealer dealer) =>
            _db.ExecuteAsync(@"INSERT INTO Dealers (DealerName, ContactPerson, Phone, Address, Email, Tier,IsActive)
                               VALUES (@DealerName, @ContactPerson, @Phone, @Address, @Email, @Tier,@IsActive)", dealer);
        public async Task<bool> UpdateDealerAsync(Dealer dealer) =>
            await _db.ExecuteAsync(@"UPDATE Dealers SET DealerName=@DealerName,ContactPerson=@ContactPerson,Phone=@Phone,
                Address=@Address,Email=@Email,Tier=@Tier,IsActive=@IsActive WHERE DealerId=@DealerId", dealer) == 1;
        public async Task<bool> DeleteDealerAsync(int dealerId)
        {
            try { return await _db.ExecuteAsync("DELETE FROM Dealers WHERE DealerId=@Id", new { Id = dealerId }) == 1; }
            catch (SqlException ex) when (ex.Number == 547) { return false; }
        }
    }

    public class SqlPreOrderRepository : IPreOrderRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlPreOrderRepository(ISqlDataAccess db) => _db = db;

        /// <summary>
        /// Ghi nhận PreOrder và PreOrderLines trong 1 TRANSACTION duy nhất (K2.2).
        /// Tất cả truy vấn tham số hóa (K2.3).
        /// </summary>
        public async Task<int> CreatePreOrderTransactionAsync(PreOrder preOrder)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction(
                IsolationLevel.Serializable);

            try
            {
                if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Dealers WITH (UPDLOCK,HOLDLOCK) WHERE DealerId=@Id AND IsActive=1",
                    new { Id = preOrder.DealerId }, transaction) != 1)
                    throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException("Đại lý đã ngừng giao dịch hoặc không còn tồn tại.");
                foreach (var group in preOrder.PreOrderLines
                    .GroupBy(l => l.HarvestBatchId)
                    .OrderBy(g => g.Key))
                {
                    var batch = await conn.QuerySingleOrDefaultAsync<HarvestBatch>(
                        @"SELECT * FROM HarvestBatches WITH(UPDLOCK,HOLDLOCK)
                  WHERE HarvestBatchId=@Id",
                        new { Id = group.Key }, transaction);

                    if (batch == null ||
                        batch.IsArchived || batch.Status != HarvestBatchStatus.OpenForPreOrder)
                    {
                        throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                            "Lô không còn mở đặt trước.");
                    }

                    var current = await conn.ExecuteScalarAsync<decimal>(
                        @"SELECT COALESCE(SUM(l.Quantity),0)
                  FROM PreOrderLines l INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId
                  WHERE l.HarvestBatchId=@Id AND o.Status<>4",
                        new { Id = group.Key }, transaction);

                    var requested = group.Sum(l => l.Quantity);

                    if (current + requested > batch.ExpectedQuantity)
                    {
                        throw new AgriculturalCoop.CoreBusiness.Exceptions.ExceededExpectedYieldException(
                            group.Key, batch.ExpectedQuantity, current, requested);
                    }

                    foreach (var line in group)
                        line.UnitPrice = batch.UnitPrice;
                }

                preOrder.RecalculateTotal();

                var preOrderId = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO PreOrders
                (OrderNumber,DealerId,OrderDate,Status,TotalAmount,Notes,RecipientName,ContactPhone,DeliveryAddress,PaymentMethod,PaymentStatus,TransferContent)
            OUTPUT INSERTED.PreOrderId
            VALUES
                (@OrderNumber,@DealerId,@OrderDate,@Status,@TotalAmount,@Notes,@RecipientName,@ContactPhone,@DeliveryAddress,@PaymentMethod,@PaymentStatus,@TransferContent)",
                    new
                    {
                        preOrder.OrderNumber,
                        preOrder.DealerId,
                        preOrder.OrderDate,
                        Status = (int)preOrder.Status,
                        preOrder.TotalAmount,
                        preOrder.Notes,
                        preOrder.RecipientName,
                        preOrder.ContactPhone,
                        preOrder.DeliveryAddress,
                        PaymentMethod = string.IsNullOrWhiteSpace(preOrder.PaymentMethod) ? "COD" : preOrder.PaymentMethod,
                        PaymentStatus = string.IsNullOrWhiteSpace(preOrder.PaymentStatus) ? "Chưa thanh toán" : preOrder.PaymentStatus,
                        preOrder.TransferContent
                    }, transaction);

                preOrder.PreOrderId = preOrderId;

                foreach (var line in preOrder.PreOrderLines)
                {
                    line.PreOrderId = preOrderId;

                    line.PreOrderLineId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO PreOrderLines
                    (PreOrderId,HarvestBatchId,Quantity,UnitPrice)
                OUTPUT INSERTED.PreOrderLineId
                VALUES
                    (@PreOrderId,@HarvestBatchId,@Quantity,@UnitPrice)",
                        new
                        {
                            PreOrderId = preOrderId,
                            line.HarvestBatchId,
                            line.Quantity,
                            line.UnitPrice
                        }, transaction);
                }

                transaction.Commit();
                return preOrderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<PreOrder>> GetPreOrdersByDealerAsync(int dealerId)
        {
            using var conn = _db.CreateConnection();

            var result = (await conn.QueryAsync<PreOrder, Dealer, PreOrder>(@"
        SELECT o.*,d.*
        FROM PreOrders o
        INNER JOIN Dealers d ON o.DealerId=d.DealerId
        WHERE o.DealerId=@DealerId
        ORDER BY o.OrderDate DESC",
                (order, dealer) =>
                {
                    order.Dealer = dealer;
                    return order;
                },
                new { DealerId = dealerId },
                splitOn: "DealerId")).ToList();

            foreach (var order in result)
            {
                order.PreOrderLines =
                    (await GetPreOrderByIdAsync(order.PreOrderId))?.PreOrderLines
                    ?? new();
                order.RefreshAllocationStatus();
            }

            return result;
        }

        public async Task<IEnumerable<PreOrder>> GetAllPreOrdersAsync()
        {
            using var conn = _db.CreateConnection();

            var result = (await conn.QueryAsync<PreOrder, Dealer, PreOrder>(@"
        SELECT o.*,d.*
        FROM PreOrders o
        INNER JOIN Dealers d ON o.DealerId=d.DealerId
        ORDER BY o.OrderDate DESC",
                (order, dealer) =>
                {
                    order.Dealer = dealer;
                    return order;
                },
                splitOn: "DealerId")).ToList();

            foreach (var order in result)
            {
                order.PreOrderLines =
                    (await GetPreOrderByIdAsync(order.PreOrderId))?.PreOrderLines
                    ?? new();
                order.RefreshAllocationStatus();
            }

            return result;
        }

        public async Task<PreOrder?> GetPreOrderByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var orderSql = @"SELECT o.*, d.* FROM PreOrders o INNER JOIN Dealers d ON o.DealerId = d.DealerId WHERE o.PreOrderId = @Id";
            var orders = await conn.QueryAsync<PreOrder, Dealer, PreOrder>(
                orderSql,
                (order, dealer) => { order.Dealer = dealer; return order; },
                new { Id = id },
                splitOn: "DealerId");

            var preOrder = orders.FirstOrDefault();
            if (preOrder != null)
            {
                var linesSql = @"
                    SELECT l.*, b.*, p.*, a.*
                    FROM PreOrderLines l
                    INNER JOIN HarvestBatches b ON l.HarvestBatchId = b.HarvestBatchId
                    INNER JOIN Products p ON b.ProductId = p.ProductId
                    LEFT JOIN Allocations a ON l.PreOrderLineId = a.PreOrderLineId
                    WHERE l.PreOrderId = @PreOrderId";

                var lines = await conn.QueryAsync<PreOrderLine, HarvestBatch, Product, Allocation, PreOrderLine>(
                    linesSql,
                    (line, batch, product, alloc) =>
                    {
                        batch.Product = product;
                        line.HarvestBatch = batch;
                        line.Allocation = alloc;
                        return line;
                    },
                    new { PreOrderId = id },
                    splitOn: "HarvestBatchId,ProductId,AllocationId");

                preOrder.PreOrderLines = lines.ToList();
                preOrder.RefreshAllocationStatus();
            }
            return preOrder;
        }

        public Task<decimal> GetTotalOrderedQuantityForBatchAsync(int batchId) =>
            _db.QuerySingleOrDefaultAsync<decimal, dynamic>(
                "SELECT COALESCE(SUM(l.Quantity), 0) FROM PreOrderLines l INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId WHERE l.HarvestBatchId = @BatchId AND o.Status<>4",
                new { BatchId = batchId });

        public async Task<IEnumerable<PreOrderLine>> GetLinesByBatchIdAsync(int batchId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT l.*, o.*, d.*, b.*, a.*
                FROM PreOrderLines l
                INNER JOIN PreOrders o ON l.PreOrderId = o.PreOrderId
                INNER JOIN Dealers d ON o.DealerId = d.DealerId
                INNER JOIN HarvestBatches b ON l.HarvestBatchId = b.HarvestBatchId
                LEFT JOIN Allocations a ON l.PreOrderLineId = a.PreOrderLineId
                WHERE l.HarvestBatchId = @BatchId";

            var lines = await conn.QueryAsync<PreOrderLine, PreOrder, Dealer, HarvestBatch, Allocation, PreOrderLine>(
                sql,
                (line, order, dealer, batch, alloc) =>
                {
                    order.Dealer = dealer;
                    line.PreOrder = order;
                    line.HarvestBatch = batch;
                    line.Allocation = alloc;
                    return line;
                },
                new { BatchId = batchId },
                splitOn: "PreOrderId,DealerId,HarvestBatchId,AllocationId");

            return lines;
        }
    }

    public class SqlAllocationRepository : IAllocationRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlAllocationRepository(ISqlDataAccess db) => _db = db;

        /// <summary>
        /// Ghi nhận phân bổ hàng loạt trong 1 TRANSACTION duy nhất (K2.2).
        /// </summary>
        public async Task SaveAllocationsTransactionAsync(int batchId, IEnumerable<Allocation> allocations,
            decimal actualQuantity, DateTime harvestDate, IEnumerable<DealerNotification> notifications)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                await SaveWithinTransactionAsync(conn, transaction, batchId, allocations, actualQuantity, harvestDate, notifications);
                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }

        internal static async Task SaveWithinTransactionAsync(SqlConnection conn, SqlTransaction transaction,
            int batchId, IEnumerable<Allocation> allocations, decimal actualQuantity, DateTime harvestDate,
            IEnumerable<DealerNotification> notifications)
        {
                var batchExists = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM HarvestBatches WITH (UPDLOCK,HOLDLOCK) WHERE HarvestBatchId=@BatchId AND IsArchived=0 AND Status<>4", new { BatchId = batchId }, transaction);
                var currentLines = (await conn.QueryAsync<PreOrderLine>(@"SELECT l.* FROM PreOrderLines l
                    INNER JOIN PreOrders o ON o.PreOrderId=l.PreOrderId WHERE l.HarvestBatchId=@BatchId AND o.Status<>4",
                    new { BatchId = batchId }, transaction)).ToList();
                var items = allocations.ToList();
                if (batchExists != 1 || currentLines.Count != items.Count ||
                    currentLines.Any(l => !items.Any(a => a.PreOrderLineId == l.PreOrderLineId && a.RequestedQuantity == l.Quantity)))
                    throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException("Danh sách đơn đã thay đổi. Hãy tải lại trước khi phân bổ.");
                await conn.ExecuteAsync("DELETE FROM Allocations WHERE HarvestBatchId = @BatchId", new { BatchId = batchId }, transaction);

                var insertSql = @"
                    INSERT INTO Allocations (PreOrderLineId, HarvestBatchId, RequestedQuantity, AllocatedQuantity, AllocationDate, Reason)
                    VALUES (@PreOrderLineId, @HarvestBatchId, @RequestedQuantity, @AllocatedQuantity, @AllocationDate, @Reason)";

                foreach (var alloc in items)
                {
                    await conn.ExecuteAsync(insertSql, new
                    {
                        alloc.PreOrderLineId,
                        HarvestBatchId = batchId,
                        alloc.RequestedQuantity,
                        alloc.AllocatedQuantity,
                        alloc.AllocationDate,
                        alloc.Reason
                    }, transaction);
                }

                await conn.ExecuteAsync(@"UPDATE HarvestBatches SET ActualQuantity=@ActualQuantity,ActualHarvestDate=@HarvestDate,Status=3
                    WHERE HarvestBatchId=@BatchId;
                    UPDATE DealerNotifications SET IsSuperseded=1 WHERE HarvestBatchId=@BatchId AND Kind=1;",
                    new { BatchId = batchId, ActualQuantity = actualQuantity, HarvestDate = harvestDate }, transaction);
                foreach (var notification in notifications)
                    await SqlDealerNotificationRepository.UpsertAsync(conn, transaction, notification);
                await conn.ExecuteAsync(@"UPDATE o SET Status=CASE WHEN NOT EXISTS (
                    SELECT 1 FROM PreOrderLines l LEFT JOIN Allocations a ON a.PreOrderLineId=l.PreOrderLineId
                    WHERE l.PreOrderId=o.PreOrderId AND (a.AllocationId IS NULL OR a.AllocatedQuantity<l.Quantity)) THEN 3 ELSE 2 END
                    FROM PreOrders o WHERE o.Status<>4 AND EXISTS(SELECT 1 FROM PreOrderLines l WHERE l.PreOrderId=o.PreOrderId AND l.HarvestBatchId=@BatchId)",
                    new { BatchId = batchId }, transaction);
        }

        public async Task<IEnumerable<Allocation>> GetAllocationsByBatchIdAsync(int batchId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT a.*, l.*, o.*, d.*
                FROM Allocations a
                INNER JOIN PreOrderLines l ON a.PreOrderLineId = l.PreOrderLineId
                INNER JOIN PreOrders o ON l.PreOrderId = o.PreOrderId
                INNER JOIN Dealers d ON o.DealerId = d.DealerId
                WHERE a.HarvestBatchId = @BatchId";

            return await conn.QueryAsync<Allocation, PreOrderLine, PreOrder, Dealer, Allocation>(
                sql,
                (alloc, line, order, dealer) =>
                {
                    order.Dealer = dealer;
                    line.PreOrder = order;
                    alloc.PreOrderLine = line;
                    return alloc;
                },
                new { BatchId = batchId },
                splitOn: "PreOrderLineId,PreOrderId,DealerId");
        }

        public async Task<IEnumerable<Allocation>> GetAllocationsByDealerIdAsync(int dealerId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT a.*, l.*, b.*, p.*
                FROM Allocations a
                INNER JOIN PreOrderLines l ON a.PreOrderLineId = l.PreOrderLineId
                INNER JOIN PreOrders o ON l.PreOrderId = o.PreOrderId
                INNER JOIN HarvestBatches b ON a.HarvestBatchId = b.HarvestBatchId
                INNER JOIN Products p ON b.ProductId = p.ProductId
                WHERE o.DealerId = @DealerId
                ORDER BY a.AllocationDate DESC";

            return await conn.QueryAsync<Allocation, PreOrderLine, HarvestBatch, Product, Allocation>(
                sql,
                (alloc, line, batch, product) =>
                {
                    batch.Product = product;
                    alloc.HarvestBatch = batch;
                    alloc.PreOrderLine = line;
                    return alloc;
                },
                new { DealerId = dealerId },
                splitOn: "PreOrderLineId,HarvestBatchId,ProductId");
        }
    }
}
