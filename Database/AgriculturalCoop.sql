-- ============================================================================
-- ĐỒ ÁN CAPSTONE ECO2415 - ĐỀ TÀI 16: HTX NÔNG SẢN ĐẶT TRƯỚC THEO MÙA VỤ
-- SCRIPT CSDL SQL SERVER CHUẨN 3NF (K2.1 - 8 ĐIỂM)
-- ============================================================================

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AgriculturalCoopDB')
BEGIN
    CREATE DATABASE AgriculturalCoopDB;
END
GO

USE AgriculturalCoopDB;
GO

-- 1. BẢNG XÃ VIÊN (Members)
IF OBJECT_ID('dbo.Allocations', 'U') IS NOT NULL DROP TABLE dbo.Allocations;
IF OBJECT_ID('dbo.PreOrderLines', 'U') IS NOT NULL DROP TABLE dbo.PreOrderLines;
IF OBJECT_ID('dbo.PreOrders', 'U') IS NOT NULL DROP TABLE dbo.PreOrders;
IF OBJECT_ID('dbo.HarvestBatches', 'U') IS NOT NULL DROP TABLE dbo.HarvestBatches;
IF OBJECT_ID('dbo.Dealers', 'U') IS NOT NULL DROP TABLE dbo.Dealers;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Members', 'U') IS NOT NULL DROP TABLE dbo.Members;
GO

CREATE TABLE dbo.Members (
    MemberId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Email VARCHAR(100) NULL,
    AreaHectare DECIMAL(10,2) NOT NULL DEFAULT 1.0,
    JoinDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2. BẢNG NÔNG SẢN (Products)
CREATE TABLE dbo.Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(150) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(20) NOT NULL DEFAULT N'kg',
    StandardPrice DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL
);
GO

-- 3. BẢNG ĐẠI LÝ (Dealers)
CREATE TABLE dbo.Dealers (
    DealerId INT IDENTITY(1,1) PRIMARY KEY,
    DealerName NVARCHAR(150) NOT NULL,
    ContactPerson NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Email VARCHAR(100) NULL,
    Tier NVARCHAR(50) NOT NULL DEFAULT N'Bạc'
);
GO

-- 4. BẢNG LÔ THU HOẠCH MÙA VỤ (HarvestBatches)
CREATE TABLE dbo.HarvestBatches (
    HarvestBatchId INT IDENTITY(1,1) PRIMARY KEY,
    BatchCode VARCHAR(50) NOT NULL UNIQUE,
    MemberId INT NOT NULL,
    ProductId INT NOT NULL,
    ExpectedQuantity DECIMAL(12,2) NOT NULL,
    ActualQuantity DECIMAL(12,2) NULL,
    ExpectedHarvestDate DATETIME2 NOT NULL,
    ActualHarvestDate DATETIME2 NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 0: Planned, 1: OpenForPreOrder, 2: Harvested, 3: Allocated, 4: Completed
    CONSTRAINT FK_HarvestBatches_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberId),
    CONSTRAINT FK_HarvestBatches_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId),
    CONSTRAINT CK_ExpectedQuantity CHECK (ExpectedQuantity > 0)
);
GO

-- 5. BẢNG ĐƠN ĐẶT TRƯỚC (PreOrders - Header)
CREATE TABLE dbo.PreOrders (
    PreOrderId INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL UNIQUE,
    DealerId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status INT NOT NULL DEFAULT 1, -- 0: Pending, 1: Confirmed, 2: PartiallyAllocated, 3: FullyAllocated, 4: Cancelled
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_PreOrders_Dealers FOREIGN KEY (DealerId) REFERENCES dbo.Dealers(DealerId)
);
GO

-- 6. BẢNG CHI TIẾT ĐẶT TRƯỚC (PreOrderLines - Line)
CREATE TABLE dbo.PreOrderLines (
    PreOrderLineId INT IDENTITY(1,1) PRIMARY KEY,
    PreOrderId INT NOT NULL,
    HarvestBatchId INT NOT NULL,
    Quantity DECIMAL(12,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_PreOrderLines_PreOrders FOREIGN KEY (PreOrderId) REFERENCES dbo.PreOrders(PreOrderId) ON DELETE CASCADE,
    CONSTRAINT FK_PreOrderLines_HarvestBatches FOREIGN KEY (HarvestBatchId) REFERENCES dbo.HarvestBatches(HarvestBatchId),
    CONSTRAINT CK_PreOrderLine_Quantity CHECK (Quantity > 0)
);
GO

-- 7. BẢNG PHÂN BỔ SẢN LƯỢNG (Allocations - Nghiệp vụ đặc thù)
CREATE TABLE dbo.Allocations (
    AllocationId INT IDENTITY(1,1) PRIMARY KEY,
    PreOrderLineId INT NOT NULL,
    HarvestBatchId INT NOT NULL,
    RequestedQuantity DECIMAL(12,2) NOT NULL,
    AllocatedQuantity DECIMAL(12,2) NOT NULL,
    AllocationDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Reason NVARCHAR(255) NULL,
    CONSTRAINT FK_Allocations_PreOrderLines FOREIGN KEY (PreOrderLineId) REFERENCES dbo.PreOrderLines(PreOrderLineId),
    CONSTRAINT FK_Allocations_HarvestBatches FOREIGN KEY (HarvestBatchId) REFERENCES dbo.HarvestBatches(HarvestBatchId),
    CONSTRAINT CK_AllocatedQuantity CHECK (AllocatedQuantity >= 0)
);
GO

-- ============================================================================
-- DỮ LIỆU MẪU (SEED DATA CHUẨN ĐÚNG THEO VÍ DỤ ĐỀ BÀI)
-- ============================================================================

-- Thêm Xã viên
INSERT INTO dbo.Members (FullName, Phone, Address, Email, AreaHectare, JoinDate)
VALUES 
(N'Nguyễn Văn Hùng', '0905123456', N'Xã Hương Thọ, TP. Huế', 'hung.nv@htx.vn', 2.5, '2022-03-15'),
(N'Trần Thị Lan', '0914234567', N'Xã Thủy Bằng, TP. Huế', 'lan.tt@htx.vn', 1.8, '2022-06-20'),
(N'Lê Đình Phúc', '0983345678', N'Phường Hương Vân, TX. Hương Trà', 'phuc.ld@htx.vn', 3.2, '2023-01-10');
GO

-- Thêm Nông sản
INSERT INTO dbo.Products (ProductName, Category, Unit, StandardPrice, Description, ImageUrl)
VALUES 
(N'Thanh trà Thủy Biều (Đặc sản Huế)', N'Trái cây đặc sản', N'kg', 38000, N'Thanh trà vỏ mỏng, tép mọng nước, vị ngọt thanh mát nức tiếng Cố Đô.', 'https://images.unsplash.com/photo-1557800636-894a64c1696f?auto=format&fit=crop&w=600&q=80'),
(N'Cà chua bi hữu cơ VietGAP', N'Rau củ quả', N'kg', 28000, N'Trồng trong nhà kính công nghệ cao, vị chua ngọt đậm đà, giàu lycopene.', 'https://images.unsplash.com/photo-1592924357228-91a4daadcfea?auto=format&fit=crop&w=600&q=80'),
(N'Bưởi da xanh ruột hồng', N'Trái cây đặc sản', N'kg', 45000, N'Bưởi da xanh ngọt đậm, ráo múi, đạt tiêu chuẩn hữu cơ xuất khẩu.', 'https://images.unsplash.com/photo-1577234286642-fc512a5f8f11?auto=format&fit=crop&w=600&q=80'),
(N'Rau cải thìa thủy canh', N'Rau ăn lá', N'kg', 22000, N'Rau xanh giòn ngọt, tuyệt đối không dư lượng thuốc BVTV.', 'https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=600&q=80');
GO

-- Thêm Đại lý
UPDATE dbo.Products SET ImageUrl = CASE ProductId WHEN 1 THEN '/images/products/thanhtra.svg' WHEN 2 THEN '/images/products/cachua.svg' WHEN 3 THEN '/images/products/buoi.svg' WHEN 4 THEN '/images/products/caithia.svg' END WHERE ProductId BETWEEN 1 AND 4;
INSERT INTO dbo.Products (ProductName, Category, Unit, StandardPrice, Description, ImageUrl) VALUES
(N'Xoài cát chu',N'Trái cây đặc sản',N'kg',52000,N'Xoài chín thơm, thịt vàng ngọt đậm.','/images/products/xoaicat.svg'),
(N'Dâu tây',N'Trái cây đặc sản',N'kg',145000,N'Dâu tây đỏ mọng, trồng an toàn.','/images/products/dautay.svg'),
(N'Cam sành',N'Trái cây đặc sản',N'kg',36000,N'Cam mọng nước, vị chua ngọt tự nhiên.','/images/products/camsanh.svg'),
(N'Cà rốt',N'Rau củ quả',N'kg',24000,N'Cà rốt tươi giòn.','/images/products/carot.svg'),
(N'Dưa lưới',N'Trái cây đặc sản',N'kg',68000,N'Dưa lưới thơm ngọt.','/images/products/dualuoi.svg'),
(N'Khoai lang',N'Rau củ quả',N'kg',26000,N'Khoai lang bở ngọt.','/images/products/khoailang.svg'),
(N'Măng tây',N'Rau củ quả',N'kg',78000,N'Măng tây non xanh giòn.','/images/products/mangtay.svg'),
(N'Nấm hương',N'Rau củ quả',N'kg',95000,N'Nấm hương thơm tự nhiên.','/images/products/namhuong.svg'),
(N'Ổi nữ hoàng',N'Trái cây đặc sản',N'kg',32000,N'Ổi giòn, thơm nhẹ.','/images/products/oinuhoang.svg'),
(N'Sầu riêng',N'Trái cây đặc sản',N'kg',115000,N'Sầu riêng tuyển chọn.','/images/products/saurieng.svg'),
(N'Xà lách',N'Rau ăn lá',N'kg',30000,N'Xà lách xanh tươi.','/images/products/xalach.svg'),
(N'Bơ sáp',N'Trái cây đặc sản',N'kg',62000,N'Bơ sáp dẻo bùi.','/images/products/bosap.svg');
GO
INSERT INTO dbo.Dealers (DealerName, ContactPerson, Phone, Address, Email, Tier)
VALUES 
(N'Đại lý Nông sản An Phú (Đại lý A)', N'Trần Minh Phú', '0935111222', N'Quận Cẩm Lệ, TP. Đà Nẵng', 'anphu.dn@gmail.com', N'Kim Cương'),
(N'Chuỗi Thực phẩm Sạch Xanh Mart (Đại lý B)', N'Lê Hoàng Yến', '0946222333', N'Quận 1, TP. Hồ Chí Minh', 'xanhmart.hcm@gmail.com', N'Vàng'),
(N'Hệ thống Siêu thị Co.op Huế (Đại lý C)', N'Phạm Quốc Tuấn', '0977333444', N'TP. Huế, Thừa Thiên Huế', 'coopmart.hue@coop.vn', N'Bạc');
GO

-- Thêm Lô thu hoạch: Lô 1 dự kiến 1000kg; Lô 2 dự kiến 800kg
INSERT INTO dbo.HarvestBatches (BatchCode, MemberId, ProductId, ExpectedQuantity, ActualQuantity, ExpectedHarvestDate, ActualHarvestDate, UnitPrice, Status)
VALUES 
('LO-THANHTRA-001', 1, 1, 1000.0, 720.0, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -1, GETDATE()), 35000, 2),
('LO-CACHUA-002', 2, 2, 800.0, NULL, DATEADD(DAY, 7, GETDATE()), NULL, 25000, 1),
('LO-BUOI-003', 3, 3, 1500.0, NULL, DATEADD(DAY, 15, GETDATE()), NULL, 42000, 1);
GO

-- Thêm Đơn đặt trước: A đặt 400kg, B đặt 300kg, C đặt 200kg (Tổng = 900kg <= 1000kg)
INSERT INTO dbo.HarvestBatches (BatchCode, MemberId, ProductId, ExpectedQuantity, ActualQuantity, ExpectedHarvestDate, ActualHarvestDate, UnitPrice, Status) VALUES
('LO-MUA-VU-004',1,5,500,NULL,'2026-11-05',NULL,52000,1),
('LO-MUA-VU-005',2,6,575,NULL,'2026-11-08',NULL,145000,1),
('LO-MUA-VU-006',3,7,650,NULL,'2026-11-11',NULL,36000,1),
('LO-MUA-VU-007',1,8,725,NULL,'2026-11-14',NULL,24000,1),
('LO-MUA-VU-008',2,9,800,NULL,'2026-11-17',NULL,68000,1),
('LO-MUA-VU-009',3,10,875,NULL,'2026-11-20',NULL,26000,1),
('LO-MUA-VU-010',1,11,950,NULL,'2026-11-23',NULL,78000,1),
('LO-MUA-VU-011',2,12,1025,NULL,'2026-11-26',NULL,95000,1),
('LO-MUA-VU-012',3,13,1100,NULL,'2026-11-29',NULL,32000,1),
('LO-MUA-VU-013',1,14,1175,NULL,'2026-12-02',NULL,115000,1),
('LO-MUA-VU-014',2,15,1250,NULL,'2026-12-05',NULL,30000,1),
('LO-MUA-VU-015',3,16,1325,NULL,'2026-12-08',NULL,62000,1);
GO
INSERT INTO dbo.PreOrders (OrderNumber, DealerId, OrderDate, Status, TotalAmount, Notes)
VALUES 
('PRE-202610-001', 1, DATEADD(DAY, -10, GETDATE()), 1, 14000000, N'Đại lý A đặt trước 400kg'),
('PRE-202610-002', 2, DATEADD(DAY, -9, GETDATE()), 1, 10500000, N'Đại lý B đặt trước 300kg'),
('PRE-202610-003', 3, DATEADD(DAY, -8, GETDATE()), 1, 7000000, N'Đại lý C đặt trước 200kg');
GO

INSERT INTO dbo.PreOrderLines (PreOrderId, HarvestBatchId, Quantity, UnitPrice)
VALUES 
(1, 1, 400.0, 35000),
(2, 1, 300.0, 35000),
(3, 1, 200.0, 35000);
GO

PRINT N'Cơ sở dữ liệu AgriculturalCoopDB và dữ liệu mẫu đã được tạo thành công 100%!';
