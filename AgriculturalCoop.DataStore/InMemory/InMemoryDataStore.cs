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
        IAllocationRepository, IUserAccountRepository, IRevenueRepository, IDealerNotificationRepository, IOrderManagementRepository
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

        private readonly List<UserAccount> _accounts = new();
        private readonly List<DealerNotification> _notifications = new();
        private readonly object _gate = new();

        public Task<UserAccount?> FindByEmailAsync(string email) =>
            Task.FromResult(_accounts.FirstOrDefault(a => a.Email == email));

        public Task<bool> HasAdminAsync() =>
            Task.FromResult(_accounts.Any(a => a.Role == "Admin"));

        public Task CreateAsync(
            UserAccount account, string phone = "", bool initialAdmin = false)
        {
            lock (_gate)
            {
                if (_accounts.Any(a => a.Email == account.Email) ||
                    (initialAdmin && _accounts.Any(a => a.Role == "Admin")))
                {
                    throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                        "Tài khoản đã tồn tại hoặc quản trị viên đã được tạo.");
                }

                account.Role = initialAdmin ? "Admin" : "Dealer";

                if (!initialAdmin)
                {
                    var dealer = new Dealer
                    {
                        DealerId = _nextDealerId++,
                        DealerName = account.DisplayName,
                        Email = account.Email,
                        Phone = phone
                    };

                    _dealers.Add(dealer);
                    account.DealerId = dealer.DealerId;
                }

                account.UserAccountId = _accounts.Count + 1;
                _accounts.Add(account);
            }

            return Task.CompletedTask;
        }

        public Task<bool> DeleteMemberAsync(int id) =>
            Task.FromResult(
                !_batches.Any(b => b.MemberId == id) &&
                _members.RemoveAll(m => m.MemberId == id) == 1);

        public Task UpdateProductAsync(Product product)
        {
            var old = _products.FirstOrDefault(p => p.ProductId == product.ProductId);

            if (old != null)
            {
                old.ProductName = product.ProductName;
                old.Category = product.Category;
                old.Unit = product.Unit;
                old.StandardPrice = product.StandardPrice;
                old.Description = product.Description;
                old.ImageUrl = product.ImageUrl;
            }

            return Task.CompletedTask;
        }

        public Task<bool> DeleteProductAsync(int id) =>
            Task.FromResult(
                !_batches.Any(b => b.ProductId == id) &&
                _products.RemoveAll(p => p.ProductId == id) == 1);

        public Task UpdateBatchAsync(HarvestBatch batch)
        {
            if (_batches.Any(b =>
                b.HarvestBatchId != batch.HarvestBatchId &&
                b.BatchCode == batch.BatchCode))
            {
                throw new AgriculturalCoop.CoreBusiness.Exceptions.PreOrderDomainException(
                    "Mã lô đã được sử dụng.");
            }

            var old = _batches.First(b =>
                b.HarvestBatchId == batch.HarvestBatchId);

            old.BatchCode = batch.BatchCode;
            old.MemberId = batch.MemberId;
            old.ProductId = batch.ProductId;
            old.ExpectedQuantity = batch.ExpectedQuantity;
            old.ExpectedHarvestDate = batch.ExpectedHarvestDate;
            old.UnitPrice = batch.UnitPrice;

            return Task.CompletedTask;
        }

        public Task<bool> DeleteBatchAsync(int id) =>
            Task.FromResult(
                !_preOrderLines.Any(l => l.HarvestBatchId == id) &&
                _batches.RemoveAll(b =>
                    b.HarvestBatchId == id &&
                    b.Status < HarvestBatchStatus.Harvested) == 1);

        public Task<IEnumerable<MemberRevenue>> GetMemberRevenueAsync(
            DateTime? from, DateTime? to)
        {
            var orders = _preOrders
                .Where(o =>
                    o.Status != PreOrderStatus.Cancelled &&
                    (!from.HasValue || o.OrderDate >= from) &&
                    (!to.HasValue || o.OrderDate < to))
                .Select(o => o.PreOrderId)
                .ToHashSet();

            var rows = _members.Select(m =>
            {
                var batches = _batches
                    .Where(b => b.MemberId == m.MemberId)
                    .Select(b => b.HarvestBatchId)
                    .ToHashSet();

                var lines = _preOrderLines
                    .Where(l =>
                        batches.Contains(l.HarvestBatchId) &&
                        orders.Contains(l.PreOrderId))
                    .ToList();

                decimal Quantity(PreOrderLine line) =>
                    _allocations
                        .Where(a => a.PreOrderLineId == line.PreOrderLineId)
                        .Sum(a => a.AllocatedQuantity);

                return new MemberRevenue
                {
                    MemberId = m.MemberId,
                    FullName = m.FullName,
                    BatchCount = batches.Count,
                    OrderedQuantity = lines.Sum(l => l.Quantity),
                    AllocatedQuantity = lines.Sum(Quantity),
                    CommittedAmount = lines.Sum(l => l.LineTotal),
                    AllocatedAmount = lines.Sum(l => Quantity(l) * l.UnitPrice)
                };
            }).ToList();

            return Task.FromResult<IEnumerable<MemberRevenue>>(rows);
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
                new Product { ProductId = _nextProductId++, ProductName = "Thanh trà Thủy Biều (Đặc sản Huế)", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 38000, Description = "Thanh trà vỏ mỏng, tép mọng nước, vị ngọt thanh mát nức tiếng Cố Đô.", ImageUrl = "/images/products/thanhtra.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Cà chua bi hữu cơ VietGAP", Category = "Rau củ quả", Unit = "kg", StandardPrice = 28000, Description = "Trồng trong nhà kính công nghệ cao, vị chua ngọt đậm đà, giàu lycopene.", ImageUrl = "/images/products/cachua.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Bưởi da xanh ruột hồng", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 45000, Description = "Bưởi da xanh ngọt đậm, ráo múi, đạt tiêu chuẩn hữu cơ xuất khẩu.", ImageUrl = "/images/products/buoi.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Rau cải thìa thủy canh", Category = "Rau ăn lá", Unit = "kg", StandardPrice = 22000, Description = "Rau xanh giòn ngọt, tuyệt đối không dư lượng thuốc BVTV, thu hoạch trong ngày.", ImageUrl = "/images/products/caithia.svg" }
            });

            _products.AddRange(new[]
            {
                new Product { ProductId = _nextProductId++, ProductName = "Xoài cát chu", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 52000, Description = "Xoài chín thơm, thịt vàng ngọt đậm, tuyển chọn từ vườn xã viên.", ImageUrl = "/images/products/xoaicat.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Dâu tây", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 145000, Description = "Dâu tây đỏ mọng, trồng theo quy trình an toàn.", ImageUrl = "/images/products/dautay.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Cam sành", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 36000, Description = "Cam sành mọng nước, vị chua ngọt tự nhiên.", ImageUrl = "/images/products/camsanh.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Cà rốt", Category = "Rau củ quả", Unit = "kg", StandardPrice = 24000, Description = "Cà rốt tươi giòn, giàu màu sắc và dinh dưỡng.", ImageUrl = "/images/products/carot.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Dưa lưới", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 68000, Description = "Dưa lưới thơm ngọt, thu hoạch khi đạt độ chín phù hợp.", ImageUrl = "/images/products/dualuoi.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Khoai lang", Category = "Rau củ quả", Unit = "kg", StandardPrice = 26000, Description = "Khoai lang bở ngọt, canh tác tại vườn xã viên.", ImageUrl = "/images/products/khoailang.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Măng tây", Category = "Rau củ quả", Unit = "kg", StandardPrice = 78000, Description = "Măng tây non, xanh giòn, thu hoạch theo đơn đặt trước.", ImageUrl = "/images/products/mangtay.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Nấm hương", Category = "Rau củ quả", Unit = "kg", StandardPrice = 95000, Description = "Nấm hương thơm tự nhiên, được sơ tuyển sau thu hoạch.", ImageUrl = "/images/products/namhuong.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Ổi nữ hoàng", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 32000, Description = "Ổi giòn, thơm nhẹ, chăm sóc theo quy trình an toàn.", ImageUrl = "/images/products/oinuhoang.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Sầu riêng", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 115000, Description = "Sầu riêng tuyển chọn từ vườn, dự kiến thu hoạch theo mùa.", ImageUrl = "/images/products/saurieng.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Xà lách", Category = "Rau ăn lá", Unit = "kg", StandardPrice = 30000, Description = "Xà lách xanh tươi, trồng sạch và thu hoạch trong ngày.", ImageUrl = "/images/products/xalach.svg" },
                new Product { ProductId = _nextProductId++, ProductName = "Bơ sáp", Category = "Trái cây đặc sản", Unit = "kg", StandardPrice = 62000, Description = "Bơ sáp dẻo bùi, dự kiến thu hoạch vào vụ cuối năm.", ImageUrl = "/images/products/bosap.svg" }
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

            var seasonalPrices = new[] { 52000m, 145000m, 36000m, 24000m, 68000m, 26000m, 78000m, 95000m, 32000m, 115000m, 30000m, 62000m };
            for (var i = 0; i < seasonalPrices.Length; i++)
            {
                _batches.Add(new HarvestBatch
                {
                    HarvestBatchId = _nextBatchId++,
                    BatchCode = $"LO-MUA-VU-{i + 4:000}",
                    MemberId = i % 3 + 1,
                    ProductId = i + 5,
                    ExpectedQuantity = 500 + i * 75,
                    ExpectedHarvestDate = new DateTime(2026, 11, 5).AddDays(i * 3),
                    UnitPrice = seasonalPrices[i],
                    Status = HarvestBatchStatus.OpenForPreOrder
                });
            }

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
                _batches.Where(b => !b.IsArchived && b.Status == HarvestBatchStatus.OpenForPreOrder).ToList());
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

        public Task<bool> SetPreOrderAvailabilityAsync(int batchId, bool isOpen)
        {
            var batch = _batches.FirstOrDefault(b => b.HarvestBatchId == batchId);
            if (batch == null || batch.IsArchived || batch.Status >= HarvestBatchStatus.Harvested || batch.ActualQuantity != null)
                return Task.FromResult(false);
            batch.Status = isOpen ? HarvestBatchStatus.OpenForPreOrder : HarvestBatchStatus.Planned;
            return Task.FromResult(true);
        }

        public Task<bool> SetArchivedAsync(int batchId, bool archived)
        {
            var batch = _batches.FirstOrDefault(b => b.HarvestBatchId == batchId);
            if (batch == null) return Task.FromResult(false);
            batch.IsArchived = archived;
            if (archived && batch.Status == HarvestBatchStatus.OpenForPreOrder) batch.Status = HarvestBatchStatus.Planned;
            return Task.FromResult(true);
        }
        public Task<bool> CompleteBatchAsync(int batchId)
        {
            var batch = _batches.FirstOrDefault(b => b.HarvestBatchId == batchId);
            if (batch == null || batch.IsArchived || batch.Status != HarvestBatchStatus.Allocated || !_allocations.Any(a => a.HarvestBatchId == batchId))
                return Task.FromResult(false);
            batch.Status = HarvestBatchStatus.Completed;
            return Task.FromResult(true);
        }

        private void PopulateBatchRelations()
        {
            foreach (var b in _batches)
            {
                b.Member = _members.FirstOrDefault(m => m.MemberId == b.MemberId);
                b.Product = _products.FirstOrDefault(p => p.ProductId == b.ProductId);
                b.LinkedOrderCount = _preOrderLines.Where(l => l.HarvestBatchId == b.HarvestBatchId).Select(l => l.PreOrderId).Distinct().Count();
                b.TotalOrderedQuantity = _preOrderLines
                    .Where(l => l.HarvestBatchId == b.HarvestBatchId && _preOrders.Any(o => o.PreOrderId == l.PreOrderId && o.Status != PreOrderStatus.Cancelled))
                    .Sum(l => l.Quantity);
            }
        }

        // ==================== IDealerRepository ====================
        public Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            foreach (var d in _dealers) d.HasAccount = _accounts.Any(a => a.DealerId == d.DealerId);
            return Task.FromResult<IEnumerable<Dealer>>(_dealers.ToList());
        }

        public Task<Dealer?> GetDealerByIdAsync(int id) =>
            Task.FromResult(_dealers.FirstOrDefault(d => d.DealerId == id));

        public Task<bool> UpdateDealerAsync(Dealer dealer)
        {
            var old = _dealers.FirstOrDefault(d => d.DealerId == dealer.DealerId);
            if (old == null) return Task.FromResult(false);
            old.DealerName = dealer.DealerName; old.ContactPerson = dealer.ContactPerson; old.Phone = dealer.Phone;
            old.Address = dealer.Address; old.Email = dealer.Email; old.Tier = dealer.Tier; old.IsActive = dealer.IsActive;
            return Task.FromResult(true);
        }
        public Task<bool> DeleteDealerAsync(int dealerId) => Task.FromResult(
            !_preOrders.Any(o => o.DealerId == dealerId) && !_accounts.Any(a => a.DealerId == dealerId) && _dealers.RemoveAll(d => d.DealerId == dealerId) == 1);

        public Task<bool> ConfirmOrderAsync(int orderId)
        {
            var order = _preOrders.FirstOrDefault(o => o.PreOrderId == orderId);
            if (order?.Status != PreOrderStatus.Pending || !_dealers.Any(d => d.DealerId == order.DealerId && d.IsActive) || !CanChangeOrder(orderId)) return Task.FromResult(false);
            order.Status = PreOrderStatus.Confirmed;
            return Task.FromResult(true);
        }
        private bool CanChangeOrder(int orderId) => _preOrderLines.Any(l => l.PreOrderId == orderId) &&
            !_preOrderLines.Where(l => l.PreOrderId == orderId).Any(l => _allocations.Any(a => a.PreOrderLineId == l.PreOrderLineId) ||
                _batches.Any(b => b.HarvestBatchId == l.HarvestBatchId && b.Status >= HarvestBatchStatus.Allocated));
        public Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            var order = _preOrders.FirstOrDefault(o => o.PreOrderId == orderId);
            if (order == null || order.Status is not (PreOrderStatus.Pending or PreOrderStatus.Confirmed) || !CanChangeOrder(orderId)) return Task.FromResult(false);
            order.Status = PreOrderStatus.Cancelled; order.CancellationReason = reason;
            return Task.FromResult(true);
        }

        public Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = _preOrders.FirstOrDefault(o => o.PreOrderId == orderId);
            if (order == null) return Task.FromResult(false);
            order.PaymentStatus = paymentStatus;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateOrderStatusAsync(int orderId, PreOrderStatus status)
        {
            var order = _preOrders.FirstOrDefault(o => o.PreOrderId == orderId);
            if (order == null) return Task.FromResult(false);
            order.Status = status;
            return Task.FromResult(true);
        }

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
                .Where(l => l.HarvestBatchId == batchId && _preOrders.Any(o => o.PreOrderId == l.PreOrderId && o.Status != PreOrderStatus.Cancelled))
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
                    line.PreOrder = o;
                    line.HarvestBatch = _batches.FirstOrDefault(b => b.HarvestBatchId == line.HarvestBatchId);
                    if (line.HarvestBatch != null)
                    {
                        line.HarvestBatch.Product = _products.FirstOrDefault(p => p.ProductId == line.HarvestBatch.ProductId);
                    }
                    line.Allocation = _allocations.FirstOrDefault(a => a.PreOrderLineId == line.PreOrderLineId);
                }
                o.RefreshAllocationStatus();
            }
        }

        // ==================== IAllocationRepository ====================
        public Task SaveAllocationsTransactionAsync(int batchId, IEnumerable<Allocation> allocations,
            decimal actualQuantity, DateTime harvestDate, IEnumerable<DealerNotification> notifications)
        {
            // Xóa phân bổ cũ của lô này (nếu có tính toán lại)
            _allocations.RemoveAll(a => a.HarvestBatchId == batchId);

            foreach (var alloc in allocations)
            {
                alloc.AllocationId = _nextAllocationId++;
                alloc.HarvestBatchId = batchId;
                _allocations.Add(alloc);
            }

            var batch = _batches.Single(b => b.HarvestBatchId == batchId);
            batch.ActualQuantity = actualQuantity;
            batch.ActualHarvestDate = harvestDate;
            batch.Status = HarvestBatchStatus.Allocated;
            foreach (var n in _notifications.Where(n => n.HarvestBatchId == batchId && n.Kind == 1)) n.IsSuperseded = true;
            foreach (var n in notifications) UpsertNotification(n);
            foreach (var order in _preOrders.Where(o => o.Status != PreOrderStatus.Cancelled && _preOrderLines.Any(l => l.PreOrderId == o.PreOrderId && l.HarvestBatchId == batchId)))
            {
                var orderLines = _preOrderLines.Where(l => l.PreOrderId == order.PreOrderId);
                order.Status = orderLines.All(l => _allocations.Any(a => a.PreOrderLineId == l.PreOrderLineId && a.AllocatedQuantity >= l.Quantity))
                    ? PreOrderStatus.FullyAllocated : PreOrderStatus.PartiallyAllocated;
            }
            PopulatePreOrderRelations();
            return Task.CompletedTask;
        }

        private bool OwnNotification(DealerNotification n, int dealerId) => n.DealerId == dealerId &&
            _preOrderLines.Any(l => l.PreOrderLineId == n.PreOrderLineId &&
                _preOrders.Any(o => o.PreOrderId == l.PreOrderId && o.DealerId == dealerId && o.Status != PreOrderStatus.Cancelled));
        public Task<IEnumerable<DealerNotification>> GetByDealerAsync(int dealerId) =>
            Task.FromResult<IEnumerable<DealerNotification>>(_notifications.Where(n => OwnNotification(n, dealerId)).OrderByDescending(n => n.UpdatedAt).ToList());
        public Task<int> GetUnreadCountAsync(int dealerId) => Task.FromResult(_notifications.Count(n => OwnNotification(n, dealerId) && n.ReadAt == null && !n.IsSuperseded));
        public Task<bool> MarkReadAsync(int notificationId, int dealerId)
        {
            var n = _notifications.SingleOrDefault(n => n.DealerNotificationId == notificationId && OwnNotification(n, dealerId));
            if (n == null) return Task.FromResult(false);
            n.ReadAt ??= DateTime.Now;
            return Task.FromResult(true);
        }
        public Task SaveReviewedAsync(DealerNotification notification)
        {
            UpsertNotification(notification);
            return Task.CompletedTask;
        }
        private void UpsertNotification(DealerNotification n)
        {
            var old = _notifications.SingleOrDefault(x => x.PreOrderLineId == n.PreOrderLineId && x.Kind == n.Kind);
            if (old == null)
            {
                n.DealerNotificationId = _notifications.Count + 1;
                _notifications.Add(n);
                return;
            }
            if (old.Body != n.Body || old.Subject != n.Subject || old.IsSuperseded)
            {
                old.ReadAt = null;
                old.UpdatedAt = n.UpdatedAt;
            }
            old.Subject = n.Subject; old.Body = n.Body; old.IsAiGenerated = n.IsAiGenerated; old.IsSuperseded = false;
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
            var dealerOrderIds = _preOrders.Where(o => o.DealerId == dealerId && o.Status != PreOrderStatus.Cancelled).Select(o => o.PreOrderId).ToHashSet();
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
