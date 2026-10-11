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
    }

    public class SqlHarvestBatchRepository : IHarvestBatchRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlHarvestBatchRepository(ISqlDataAccess db) => _db = db;

        public async Task<IEnumerable<HarvestBatch>> GetAllBatchesAsync()
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT b.*, m.*, p.*,
                       COALESCE((SELECT SUM(l.Quantity) FROM PreOrderLines l WHERE l.HarvestBatchId = b.HarvestBatchId), 0) AS TotalOrderedQuantity
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
                SELECT b.*, m.*, p.*,
                       COALESCE((SELECT SUM(l.Quantity) FROM PreOrderLines l WHERE l.HarvestBatchId = b.HarvestBatchId), 0) AS TotalOrderedQuantity
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
                SELECT b.*, m.*, p.*,
                       COALESCE((SELECT SUM(l.Quantity) FROM PreOrderLines l WHERE l.HarvestBatchId = b.HarvestBatchId), 0) AS TotalOrderedQuantity
                FROM HarvestBatches b
                INNER JOIN Members m ON b.MemberId = m.MemberId
                INNER JOIN Products p ON b.ProductId = p.ProductId
                WHERE b.Status = @OpenStatus
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

        public Task AddBatchAsync(HarvestBatch batch) =>
            _db.ExecuteAsync(@"INSERT INTO HarvestBatches (BatchCode, MemberId, ProductId, ExpectedQuantity, ActualQuantity, ExpectedHarvestDate, ActualHarvestDate, UnitPrice, Status)
                               VALUES (@BatchCode, @MemberId, @ProductId, @ExpectedQuantity, @ActualQuantity, @ExpectedHarvestDate, @ActualHarvestDate, @UnitPrice, @Status)", batch);

        public Task UpdateBatchYieldAsync(int batchId, decimal actualYield, DateTime harvestDate) =>
            _db.ExecuteAsync(@"UPDATE HarvestBatches 
                               SET ActualQuantity = @ActualYield, ActualHarvestDate = @HarvestDate, Status = @HarvestedStatus 
                               WHERE HarvestBatchId = @BatchId", 
                               new { BatchId = batchId, ActualYield = actualYield, HarvestDate = harvestDate, HarvestedStatus = (int)HarvestBatchStatus.Harvested });

        public Task UpdateBatchStatusAsync(int batchId, HarvestBatchStatus status) =>
            _db.ExecuteAsync(@"UPDATE HarvestBatches SET Status = @Status WHERE HarvestBatchId = @BatchId",
                             new { BatchId = batchId, Status = (int)status });
    }

    public class SqlDealerRepository : IDealerRepository
    {
        private readonly ISqlDataAccess _db;
        public SqlDealerRepository(ISqlDataAccess db) => _db = db;

        public Task<IEnumerable<Dealer>> GetAllDealersAsync() =>
            _db.QueryAsync<Dealer, dynamic>("SELECT * FROM Dealers ORDER BY DealerId", new { });

        public Task<Dealer?> GetDealerByIdAsync(int id) =>
            _db.QuerySingleOrDefaultAsync<Dealer, dynamic>("SELECT * FROM Dealers WHERE DealerId = @Id", new { Id = id });

        public Task AddDealerAsync(Dealer dealer) =>
            _db.ExecuteAsync(@"INSERT INTO Dealers (DealerName, ContactPerson, Phone, Address, Email, Tier)
                               VALUES (@DealerName, @ContactPerson, @Phone, @Address, @Email, @Tier)", dealer);
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
            using var transaction = conn.BeginTransaction();

            try
            {
                var insertOrderSql = @"
                    INSERT INTO PreOrders (OrderNumber, DealerId, OrderDate, Status, TotalAmount, Notes)
                    OUTPUT INSERTED.PreOrderId
                    VALUES (@OrderNumber, @DealerId, @OrderDate, @Status, @TotalAmount, @Notes);";

                int preOrderId = await conn.ExecuteScalarAsync<int>(insertOrderSql, new
                {
                    preOrder.OrderNumber,
                    preOrder.DealerId,
                    preOrder.OrderDate,
                    Status = (int)preOrder.Status,
                    preOrder.TotalAmount,
                    preOrder.Notes
                }, transaction);

                preOrder.PreOrderId = preOrderId;

                var insertLineSql = @"
                    INSERT INTO PreOrderLines (PreOrderId, HarvestBatchId, Quantity, UnitPrice)
                    OUTPUT INSERTED.PreOrderLineId
                    VALUES (@PreOrderId, @HarvestBatchId, @Quantity, @UnitPrice);";

                foreach (var line in preOrder.PreOrderLines)
                {
                    line.PreOrderId = preOrderId;
                    int lineId = await conn.ExecuteScalarAsync<int>(insertLineSql, new
                    {
                        PreOrderId = preOrderId,
                        line.HarvestBatchId,
                        line.Quantity,
                        line.UnitPrice
                    }, transaction);
                    line.PreOrderLineId = lineId;
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
            var sql = @"
                SELECT o.*, d.*
                FROM PreOrders o
                INNER JOIN Dealers d ON o.DealerId = d.DealerId
                WHERE o.DealerId = @DealerId
                ORDER BY o.OrderDate DESC";

            return await conn.QueryAsync<PreOrder, Dealer, PreOrder>(
                sql,
                (order, dealer) => { order.Dealer = dealer; return order; },
                new { DealerId = dealerId },
                splitOn: "DealerId");
        }

        public async Task<IEnumerable<PreOrder>> GetAllPreOrdersAsync()
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT o.*, d.*
                FROM PreOrders o
                INNER JOIN Dealers d ON o.DealerId = d.DealerId
                ORDER BY o.OrderDate DESC";

            return await conn.QueryAsync<PreOrder, Dealer, PreOrder>(
                sql,
                (order, dealer) => { order.Dealer = dealer; return order; },
                splitOn: "DealerId");
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
            }
            return preOrder;
        }

        public Task<decimal> GetTotalOrderedQuantityForBatchAsync(int batchId) =>
            _db.QuerySingleOrDefaultAsync<decimal, dynamic>(
                "SELECT COALESCE(SUM(Quantity), 0) FROM PreOrderLines WHERE HarvestBatchId = @BatchId",
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
        public async Task SaveAllocationsTransactionAsync(int batchId, IEnumerable<Allocation> allocations)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Xóa phân bổ cũ của lô (nếu tính lại)
                await conn.ExecuteAsync("DELETE FROM Allocations WHERE HarvestBatchId = @BatchId", new { BatchId = batchId }, transaction);

                var insertSql = @"
                    INSERT INTO Allocations (PreOrderLineId, HarvestBatchId, RequestedQuantity, AllocatedQuantity, AllocationDate, Reason)
                    VALUES (@PreOrderLineId, @HarvestBatchId, @RequestedQuantity, @AllocatedQuantity, @AllocationDate, @Reason)";

                foreach (var alloc in allocations)
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

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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
