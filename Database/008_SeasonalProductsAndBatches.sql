-- Adds the supplied white-background product illustrations and 12 upcoming
-- seasonal products/batches without deleting or changing existing orders.
UPDATE p SET ImageUrl = v.ImageUrl
FROM dbo.Products p
JOIN (VALUES
    (N'Thanh trà Thủy Biều (Đặc sản Huế)', N'/images/products/thanhtra.svg'),
    (N'Cà chua bi hữu cơ VietGAP', N'/images/products/cachua.svg'),
    (N'Bưởi da xanh ruột hồng', N'/images/products/buoi.svg'),
    (N'Rau cải thìa thủy canh', N'/images/products/caithia.svg')
) v(ProductName, ImageUrl) ON p.ProductName = v.ProductName;

INSERT INTO dbo.Products (ProductName, Category, Unit, StandardPrice, Description, ImageUrl)
SELECT v.ProductName, v.Category, N'kg', v.Price, v.Description, v.ImageUrl
FROM (VALUES
    (N'Xoài cát chu',N'Trái cây đặc sản',52000,N'Xoài chín thơm, thịt vàng ngọt đậm.','/images/products/xoaicat.svg'),
    (N'Dâu tây',N'Trái cây đặc sản',145000,N'Dâu tây đỏ mọng, trồng an toàn.','/images/products/dautay.svg'),
    (N'Cam sành',N'Trái cây đặc sản',36000,N'Cam mọng nước, vị chua ngọt tự nhiên.','/images/products/camsanh.svg'),
    (N'Cà rốt',N'Rau củ quả',24000,N'Cà rốt tươi giòn.','/images/products/carot.svg'),
    (N'Dưa lưới',N'Trái cây đặc sản',68000,N'Dưa lưới thơm ngọt.','/images/products/dualuoi.svg'),
    (N'Khoai lang',N'Rau củ quả',26000,N'Khoai lang bở ngọt.','/images/products/khoailang.svg'),
    (N'Măng tây',N'Rau củ quả',78000,N'Măng tây non xanh giòn.','/images/products/mangtay.svg'),
    (N'Nấm hương',N'Rau củ quả',95000,N'Nấm hương thơm tự nhiên.','/images/products/namhuong.svg'),
    (N'Ổi nữ hoàng',N'Trái cây đặc sản',32000,N'Ổi giòn, thơm nhẹ.','/images/products/oinuhoang.svg'),
    (N'Sầu riêng',N'Trái cây đặc sản',115000,N'Sầu riêng tuyển chọn.','/images/products/saurieng.svg'),
    (N'Xà lách',N'Rau ăn lá',30000,N'Xà lách xanh tươi.','/images/products/xalach.svg'),
    (N'Bơ sáp',N'Trái cây đặc sản',62000,N'Bơ sáp dẻo bùi.','/images/products/bosap.svg')
) v(ProductName, Category, Price, Description, ImageUrl)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products p WHERE p.ProductName = v.ProductName);

;WITH SeasonalBatches AS
(
    SELECT * FROM (VALUES
        (1,N'LO-MUA-VU-004',N'Xoài cát chu',500,CONVERT(datetime2,'2026-11-05'),52000),
        (2,N'LO-MUA-VU-005',N'Dâu tây',575,CONVERT(datetime2,'2026-11-08'),145000),
        (3,N'LO-MUA-VU-006',N'Cam sành',650,CONVERT(datetime2,'2026-11-11'),36000),
        (1,N'LO-MUA-VU-007',N'Cà rốt',725,CONVERT(datetime2,'2026-11-14'),24000),
        (2,N'LO-MUA-VU-008',N'Dưa lưới',800,CONVERT(datetime2,'2026-11-17'),68000),
        (3,N'LO-MUA-VU-009',N'Khoai lang',875,CONVERT(datetime2,'2026-11-20'),26000),
        (1,N'LO-MUA-VU-010',N'Măng tây',950,CONVERT(datetime2,'2026-11-23'),78000),
        (2,N'LO-MUA-VU-011',N'Nấm hương',1025,CONVERT(datetime2,'2026-11-26'),95000),
        (3,N'LO-MUA-VU-012',N'Ổi nữ hoàng',1100,CONVERT(datetime2,'2026-11-29'),32000),
        (1,N'LO-MUA-VU-013',N'Sầu riêng',1175,CONVERT(datetime2,'2026-12-02'),115000),
        (2,N'LO-MUA-VU-014',N'Xà lách',1250,CONVERT(datetime2,'2026-12-05'),30000),
        (3,N'LO-MUA-VU-015',N'Bơ sáp',1325,CONVERT(datetime2,'2026-12-08'),62000)
    ) v(MemberId, BatchCode, ProductName, Quantity, HarvestDate, UnitPrice)
)
INSERT INTO dbo.HarvestBatches
    (BatchCode, MemberId, ProductId, ExpectedQuantity, ActualQuantity, ExpectedHarvestDate, ActualHarvestDate, UnitPrice, Status)
SELECT b.BatchCode, b.MemberId, p.ProductId, b.Quantity, NULL, b.HarvestDate, NULL, b.UnitPrice, 1
FROM SeasonalBatches b
JOIN dbo.Products p ON p.ProductName = b.ProductName
JOIN dbo.Members m ON m.MemberId = b.MemberId
WHERE NOT EXISTS (SELECT 1 FROM dbo.HarvestBatches existing WHERE existing.BatchCode = b.BatchCode);
