using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.DataStore.InMemory
{
    public class InMemoryDataStore : 
        IMemberRepository, 
        IProductRepository, 
        IHarvestBatchRepository, 
        IDealerRepository, 
        IPreOrderRepository, 
        IAllocationRepository
    {
        private readonly List<Member> _members = new();
        private readonly List<Product> _products = new();
        private readonly List<HarvestBatch> _batches = new();
        private readonly List<Dealer> _dealers = new();
        private readonly List<PreOrder> _preOrders = new();
        private readonly List<PreOrderLine> _preOrderLines = new();
        private readonly List<Allocation> _allocations = new();

        private int _nextMemberId = 1;
        private int _nextProductId = 1;
        private int _nextBatchId = 1;
        private int _nextDealerId = 1;
        private int _nextPreOrderId = 1;
        private int _nextPreOrderLineId = 1;
        private int _nextAllocationId = 1;

        public InMemoryDataStore()
        {
            SeedInitialData();
        }

        private void SeedInitialData()
        {
            // 1. Seed Xã viên
            _members.AddRange(new[]
            {
                new Member { MemberId = _nextMemberId++, FullName = "Nguyễn Văn Hùng", Phone = "0905123456", Address = "Xã Hương Thọ, TP. Huế", Email = "hung.nv@htx.vn", AreaHectare = 2.5m, JoinDate = new DateTime(2022, 3, 15) },
                new Member { MemberId = _nextMemberId++, FullName = "Trần Thị Lan", Phone = "0914234567", Address = "Xã Thủy Bằng, TP. Huế", Email = "lan.tt@htx.vn", AreaHectare = 1.8m, JoinDate = new DateTime(2022, 6, 20) },
                new Member { MemberId = _nextMemberId++, FullName = "Lê Đình Phúc", Phone = "0983345678", Address = "Phường Hương Vân, TX. Hương Trà", Email = "phuc.ld@htx.vn", AreaHectare = 3.2m, JoinDate = new DateTime(2023, 1, 10) }
            });

            // 2. Seed Nông sản
            _products.AddRange(new[]
            {
                new Product { ProductId = _nextProductId++, ProductName = "Thanh trà Thủy Biều (Đặc sản Huế)", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 38000, Description = "Thanh trà vỏ mỏng, tép mọng nước, vị ngọt thanh mát nức tiếng Cố Đô.", ImageUrl = "https://images.unsplash.com/photo-1557800636-894a64c1696f?auto=format&fit=crop&w=600&q=80" },
                new Product { ProductId = _nextProductId++, ProductName = "Cà chua bi hữu cơ VietGAP", Category = "Rau củ quả", Unit = "kg", StandardPrice = 28000, Description = "Trồng trong nhà kính công nghệ cao, vị chua ngọt đậm đà, giàu lycopene.", ImageUrl = "https://images.unsplash.com/photo-1592924357228-91a4daadcfea?auto=format&fit=crop&w=600&q=80" },
                new Product { ProductId = _nextProductId++, ProductName = "Bưởi da xanh ruột hồng", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 45000, Description = "Bưởi da xanh ngọt đậm, ráo múi, đạt tiêu chuẩn hữu cơ xuất khẩu.", ImageUrl = "https://images.unsplash.com/photo-1577234286642-fc512a5f8f11?auto=format&fit=crop&w=600&q=80" },
                new Product { ProductId = _nextProductId++, ProductName = "Rau cải thìa thủy canh", Category = "Rau ăn lá", Unit = "kg", StandardPrice = 22000, Description = "Rau xanh giòn ngọt, tuyệt đối không dư lượng thuốc BVTV, thu hoạch trong ngày.", ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=600&q=80" }
            });

            // 3. Seed Đại lý
            _dealers.AddRange(new[]
            {
                new Dealer { DealerId = _nextDealerId++, DealerName = "Đại lý Nông sản An Phú (Đại lý A)", ContactPerson = "Trần Minh Phú", Phone = "0935111222", Address = "Quận Cẩm Lệ, TP. Đà Nẵng", Email = "anphu.dn@gmail.com", Tier = "Kim Cương" },
                new Dealer { DealerId = _nextDealerId++, DealerName = "Chuỗi Thực phẩm Sạch Xanh Mart (Đại lý B)", ContactPerson = "Lê Hoàng Yến", Phone = "0946222333", Address = "Quận 1, TP. Hồ Chí Minh", Email = "xanhmart.hcm@gmail.com", Tier = "Vàng" },
                new Dealer { DealerId = _nextDealerId++, DealerName = "Hệ thống Siêu thị Co.op Huế (Đại lý C)", ContactPerson = "Phạm Quốc Tuấn", Phone = "0977333444", Address = "TP. Huế, Thừa Thiên Huế", Email = "coopmart.hue@coop.vn", Tier = "Bạc" }
            });

            // 4. Seed Lô thu hoạch
            // Lô 1: Lô Thanh trà ví dụ đề bài: Dự kiến 1000 kg, thực tế 720 kg
            var batch1 = new HarvestBatch
            {
                HarvestBatchId = _nextBatchId++,
                BatchCode = "LO-THANHTRA-001",
                MemberId = 1,
                ProductId = 1,
                ExpectedQuantity = 1000,
                ActualQuantity = 720,
                ExpectedHarvestDate = DateTime.Today.AddDays(-2),
                ActualHarvestDate = DateTime.Today.AddDays(-1),
                UnitPrice = 35000,
                Status = HarvestBatchStatus.Harvested,
                TotalOrderedQuantity = 900
            };

            // Lô 2: Lô Cà chua đang mở đặt trước
            var batch2 = new HarvestBatch
            {
                HarvestBatchId = _nextBatchId++,
                BatchCode = "LO-CACHUA-002",
                MemberId = 2,
                ProductId = 2,
                ExpectedQuantity = 800,
                ActualQuantity = null,
                ExpectedHarvestDate = DateTime.Today.AddDays(7),
                UnitPrice = 25000,
                Status = HarvestBatchStatus.OpenForPreOrder,
                TotalOrderedQuantity = 300
            };

            // Lô 3: Lô Bưởi da xanh đang mở đặt trước
            var batch3 = new HarvestBatch
            {
                HarvestBatchId = _nextBatchId++,
                BatchCode = "LO-BUOI-003",
                MemberId = 3,
                ProductId = 3,
                ExpectedQuantity = 1500,
                ActualQuantity = null,
                ExpectedHarvestDate = DateTime.Today.AddDays(15),
                UnitPrice = 42000,
                Status = HarvestBatchStatus.OpenForPreOrder,
                TotalOrderedQuantity = 500
            };

            _batches.AddRange(new[] { batch1, batch2, batch3 });

            // 5. Seed Đơn đặt trước cho Lô 1 (Ví dụ chuẩn của đề tài: A đặt 400kg, B đặt 300kg, C đặt 200kg => Tổng 900kg)
            var order1 = new PreOrder
            {
                PreOrderId = _nextPreOrderId++,
                OrderNumber = "PRE-202610-001",
                DealerId = 1,
                OrderDate = DateTime.Today.AddDays(-10),
                Status = PreOrderStatus.Confirmed,
                TotalAmount = 400 * 35000,
                Notes = "Đại lý A đặt trước 400kg Thanh trà"
            };
            var line1 = new PreOrderLine
            {
                PreOrderLineId = _nextPreOrderLineId++,
                PreOrderId = order1.PreOrderId,
                HarvestBatchId = 1,
                Quantity = 400,
                UnitPrice = 35000
            };
            order1.PreOrderLines.Add(line1);

            var order2 = new PreOrder
            {
                PreOrderId = _nextPreOrderId++,
                OrderNumber = "PRE-202610-002",
                DealerId = 2,
                OrderDate = DateTime.Today.AddDays(-9),
                Status = PreOrderStatus.Confirmed,
                TotalAmount = 300 * 35000,
                Notes = "Đại lý B đặt trước 300kg Thanh trà"
            };
            var line2 = new PreOrderLine
            {
                PreOrderLineId = _nextPreOrderLineId++,
                PreOrderId = order2.PreOrderId,
                HarvestBatchId = 1,
                Quantity = 300,
                UnitPrice = 35000
            };
            order2.PreOrderLines.Add(line2);

            var order3 = new PreOrder
            {
                PreOrderId = _nextPreOrderId++,
                OrderNumber = "PRE-202610-003",
                DealerId = 3,
                OrderDate = DateTime.Today.AddDays(-8),
                Status = PreOrderStatus.Confirmed,
                TotalAmount = 200 * 35000,
                Notes = "Đại lý C đặt trước 200kg Thanh trà"
            };
            var line3 = new PreOrderLine
            {
                PreOrderLineId = _nextPreOrderLineId++,
                PreOrderId = order3.PreOrderId,
                HarvestBatchId = 1,
                Quantity = 200,
                UnitPrice = 35000
            };
            order3.PreOrderLines.Add(line3);

            _preOrders.AddRange(new[] { order1, order2, order3 });
            _preOrderLines.AddRange(new[] { line1, line2, line3 });
        }

        // ==================== IMemberRepository ====================
        public Task<IEnumerable<Member>> GetAllMembersAsync() =>
            Task.FromResult<IEnumerable<Member>>(_members.ToList());

        public Task<Member?> GetMemberByIdAsync(int id) =>
            Task.FromResult(_members.FirstOrDefault(m => m.MemberId == id));

        public Task AddMemberAsync(Member member)
        {
            member.MemberId = _nextMemberId++;
            _members.Add(member);
            return Task.CompletedTask;
        }

        public Task UpdateMemberAsync(Member member)
        {
            var existing = _members.FirstOrDefault(m => m.MemberId == member.MemberId);
            if (existing != null)
            {
                existing.FullName = member.FullName;
                existing.Phone = member.Phone;
                existing.Address = member.Address;
                existing.Email = member.Email;
                existing.AreaHectare = member.AreaHectare;
            }
            return Task.CompletedTask;
        }

        // ==================== IProductRepository ====================
        public Task<IEnumerable<Product>> GetAllProductsAsync() =>
            Task.FromResult<IEnumerable<Product>>(_products.ToList());

        public Task<Product?> GetProductByIdAsync(int id) =>
            Task.FromResult(_products.FirstOrDefault(p => p.ProductId == id));

        public Task AddProductAsync(Product product)
        {
            product.ProductId = _nextProductId++;
            _products.Add(product);
            return Task.CompletedTask;
        }

        // ==================== IHarvestBatchRepository ====================
        public Task<IEnumerable<HarvestBatch>> GetAllBatchesAsync()
        {
            PopulateBatchRelations();
            return Task.FromResult<IEnumerable<HarvestBatch>>(_batches.ToList());
        }

        public Task<HarvestBatch?> GetBatchByIdAsync(int id)
        {
            PopulateBatchRelations();
            return Task.FromResult(_batches.FirstOrDefault(b => b.HarvestBatchId == id));
        }

        public Task<IEnumerable<HarvestBatch>> GetOpenBatchesAsync()
        {
            PopulateBatchRelations();
            return Task.FromResult<IEnumerable<HarvestBatch>>(
                _batches.Where(b => b.Status == HarvestBatchStatus.OpenForPreOrder).ToList());
        }

        public Task AddBatchAsync(HarvestBatch batch)
        {
            batch.HarvestBatchId = _nextBatchId++;
            if (string.IsNullOrEmpty(batch.BatchCode))
                batch.BatchCode = $"LO-{DateTime.Now:yyyyMMdd}-{batch.HarvestBatchId:D3}";
            _batches.Add(batch);
            return Task.CompletedTask;
        }

        public Task UpdateBatchYieldAsync(int batchId, decimal actualYield, DateTime harvestDate)
        {
            var batch = _batches.FirstOrDefault(b => b.HarvestBatchId == batchId);
            if (batch != null)
            {
                batch.ActualQuantity = actualYield;
                batch.ActualHarvestDate = harvestDate;
                batch.Status = HarvestBatchStatus.Harvested;
            }
            return Task.CompletedTask;
        }

        public Task UpdateBatchStatusAsync(int batchId, HarvestBatchStatus status)
        {
            var batch = _batches.FirstOrDefault(b => b.HarvestBatchId == batchId);
            if (batch != null)
            {
                batch.Status = status;
            }
            return Task.CompletedTask;
        }

        private void PopulateBatchRelations()
        {
            foreach (var b in _batches)
            {
                b.Member = _members.FirstOrDefault(m => m.MemberId == b.MemberId);
                b.Product = _products.FirstOrDefault(p => p.ProductId == b.ProductId);
                b.TotalOrderedQuantity = _preOrderLines
                    .Where(l => l.HarvestBatchId == b.HarvestBatchId)
                    .Sum(l => l.Quantity);
            }
        }

        // ==================== IDealerRepository ====================
        public Task<IEnumerable<Dealer>> GetAllDealersAsync() =>
            Task.FromResult<IEnumerable<Dealer>>(_dealers.ToList());

        public Task<Dealer?> GetDealerByIdAsync(int id) =>
            Task.FromResult(_dealers.FirstOrDefault(d => d.DealerId == id));

        public Task AddDealerAsync(Dealer dealer)
        {
            dealer.DealerId = _nextDealerId++;
            _dealers.Add(dealer);
            return Task.CompletedTask;
        }

        // ==================== IPreOrderRepository ====================
        public Task<int> CreatePreOrderTransactionAsync(PreOrder preOrder)
        {
            // Mô phỏng Transaction trong memory:
            // Cả PreOrder và PreOrderLines được ghi nhận đồng thời
            preOrder.PreOrderId = _nextPreOrderId++;
            _preOrders.Add(preOrder);

            foreach (var line in preOrder.PreOrderLines)
            {
                line.PreOrderLineId = _nextPreOrderLineId++;
                line.PreOrderId = preOrder.PreOrderId;
                _preOrderLines.Add(line);
            }

            PopulateBatchRelations();
            return Task.FromResult(preOrder.PreOrderId);
        }

        public Task<IEnumerable<PreOrder>> GetPreOrdersByDealerAsync(int dealerId)
        {
            PopulatePreOrderRelations();
            return Task.FromResult<IEnumerable<PreOrder>>(
                _preOrders.Where(o => o.DealerId == dealerId).OrderByDescending(o => o.OrderDate).ToList());
        }

        public Task<IEnumerable<PreOrder>> GetAllPreOrdersAsync()
        {
            PopulatePreOrderRelations();
            return Task.FromResult<IEnumerable<PreOrder>>(
                _preOrders.OrderByDescending(o => o.OrderDate).ToList());
        }

        public Task<PreOrder?> GetPreOrderByIdAsync(int id)
        {
            PopulatePreOrderRelations();
            return Task.FromResult(_preOrders.FirstOrDefault(o => o.PreOrderId == id));
        }

        public Task<decimal> GetTotalOrderedQuantityForBatchAsync(int batchId)
        {
            decimal total = _preOrderLines
                .Where(l => l.HarvestBatchId == batchId)
                .Sum(l => l.Quantity);
            return Task.FromResult(total);
        }

        public Task<IEnumerable<PreOrderLine>> GetLinesByBatchIdAsync(int batchId)
        {
            PopulatePreOrderRelations();
            var lines = _preOrderLines.Where(l => l.HarvestBatchId == batchId).ToList();
            return Task.FromResult<IEnumerable<PreOrderLine>>(lines);
        }

        private void PopulatePreOrderRelations()
        {
            foreach (var o in _preOrders)
            {
                o.Dealer = _dealers.FirstOrDefault(d => d.DealerId == o.DealerId);
                o.PreOrderLines = _preOrderLines.Where(l => l.PreOrderId == o.PreOrderId).ToList();
                foreach (var line in o.PreOrderLines)
                {
                    line.HarvestBatch = _batches.FirstOrDefault(b => b.HarvestBatchId == line.HarvestBatchId);
                    if (line.HarvestBatch != null)
                    {
                        line.HarvestBatch.Product = _products.FirstOrDefault(p => p.ProductId == line.HarvestBatch.ProductId);
                    }
                    line.Allocation = _allocations.FirstOrDefault(a => a.PreOrderLineId == line.PreOrderLineId);
                }
            }
        }

        // ==================== IAllocationRepository ====================
        public Task SaveAllocationsTransactionAsync(int batchId, IEnumerable<Allocation> allocations)
        {
            // Xóa phân bổ cũ của lô này (nếu có tính toán lại)
            _allocations.RemoveAll(a => a.HarvestBatchId == batchId);

            foreach (var alloc in allocations)
            {
                alloc.AllocationId = _nextAllocationId++;
                alloc.HarvestBatchId = batchId;
                _allocations.Add(alloc);
            }

            PopulatePreOrderRelations();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Allocation>> GetAllocationsByBatchIdAsync(int batchId)
        {
            PopulatePreOrderRelations();
            var list = _allocations.Where(a => a.HarvestBatchId == batchId).ToList();
            foreach (var a in list)
            {
                a.PreOrderLine = _preOrderLines.FirstOrDefault(l => l.PreOrderLineId == a.PreOrderLineId);
                a.HarvestBatch = _batches.FirstOrDefault(b => b.HarvestBatchId == a.HarvestBatchId);
            }
            return Task.FromResult<IEnumerable<Allocation>>(list);
        }

        public Task<IEnumerable<Allocation>> GetAllocationsByDealerIdAsync(int dealerId)
        {
            PopulatePreOrderRelations();
            var dealerOrderIds = _preOrders.Where(o => o.DealerId == dealerId).Select(o => o.PreOrderId).ToHashSet();
            var dealerLineIds = _preOrderLines.Where(l => dealerOrderIds.Contains(l.PreOrderId)).Select(l => l.PreOrderLineId).ToHashSet();

            var list = _allocations.Where(a => dealerLineIds.Contains(a.PreOrderLineId)).ToList();
            foreach (var a in list)
            {
                a.PreOrderLine = _preOrderLines.FirstOrDefault(l => l.PreOrderLineId == a.PreOrderLineId);
                a.HarvestBatch = _batches.FirstOrDefault(b => b.HarvestBatchId == a.HarvestBatchId);
            }
            return Task.FromResult<IEnumerable<Allocation>>(list);
        }
    }
}
