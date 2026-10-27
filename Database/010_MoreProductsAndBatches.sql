-- 010_MoreProductsAndBatches.sql
-- Bổ sung thêm vài chục sản phẩm phong phú chia đều cho các danh mục và các lô nông sản mùa vụ

-- 1. Bổ sung các sản phẩm mới
INSERT INTO dbo.Products (ProductName, Category, Unit, StandardPrice, Description, ImageUrl)
SELECT v.ProductName, v.Category, N'kg', v.Price, v.Description, v.ImageUrl
FROM (VALUES
    -- Danh mục 1: Trái cây đặc sản (Thêm 7 sản phẩm)
    (N'Xoài cát Hòa Lộc', N'Trái cây đặc sản', 75000, N'Xoài cát Hòa Lộc loại 1, thịt quả dẻo mịn, ngọt thanh đậm đà, hương thơm nồng nàn đặc trưng.', N'/images/products/xoaicat.svg'),
    (N'Mãng cầu xiêm Đắk Lắk', N'Trái cây đặc sản', 48000, N'Mãng cầu xiêm tuyển chọn từ vùng cao nguyên, múi dày, vị chua ngọt thanh mát, giàu vitamin C.', N'/images/products/thanhtra.svg'),
    (N'Thanh long ruột đỏ Bình Thuận', N'Trái cây đặc sản', 38000, N'Thanh long ruột đỏ vỏ mỏng, ruột đỏ thẫm mọng nước, vị ngọt đậm giàu chất chống oxy hóa.', N'/images/products/dautay.svg'),
    (N'Sầu riêng Ri6 Bến Tre', N'Trái cây đặc sản', 135000, N'Sầu riêng Ri6 cơm vàng hạt lép, múi khô ráo dẻo quánh, vị béo ngọt đậm đà danh tiếng.', N'/images/products/saurieng.svg'),
    (N'Vú sữa Lò Rèn Vĩnh Kim', N'Trái cây đặc sản', 58000, N'Vú sữa vỏ mỏng màu trắng ánh tím, dòng sữa trắng ngà thơm ngát, vị ngọt dịu thanh mát.', N'/images/products/buoi.svg'),
    (N'Nhãn xuồng cơm vàng Tiền Giang', N'Trái cây đặc sản', 55000, N'Nhãn xuồng quả to tròn, cơm dày giòn ráo, màu vàng ngà, ngọt thanh hấp dẫn.', N'/images/products/xoaicat.svg'),
    (N'Dưa hấu không hạt Mặt Trời Đỏ', N'Trái cây đặc sản', 28000, N'Dưa hấu ruột đỏ au, đặc ruột không hạt, nhiều nước, vị ngọt mát giải nhiệt mùa vụ.', N'/images/products/dualuoi.svg'),

    -- Danh mục 2: Rau củ quả (Thêm 7 sản phẩm)
    (N'Cà chua beef Đà Lạt', N'Rau củ quả', 35000, N'Cà chua beef quả to dày cùi, ít hạt, thịt quả chắc thơm, trồng chuẩn tiêu chuẩn VietGAP.', N'/images/products/cachua.svg'),
    (N'Bí ngô mật Nhật Bản', N'Rau củ quả', 32000, N'Bí ngô hạt đậu giống Nhật, ruột vàng cam đậm, vị dẻo ngọt bùi béo tự nhiên, rất bổ dưỡng.', N'/images/products/carot.svg'),
    (N'Bắp ngọt hữu cơ', N'Rau củ quả', 22000, N'Bắp ngọt vàng ươm mọng sữa, hạt đều tăm tắp, vị ngọt thanh tự nhiên từ nông trại xanh.', N'/images/products/khoailang.svg'),
    (N'Ớt chuông Đà Lạt 3 màu', N'Rau củ quả', 65000, N'Ớt chuông xanh, đỏ, vàng giòn ngọt tươi ngon, dồi dào vitamin A và C, dùng cho món xào và salad.', N'/images/products/mangtay.svg'),
    (N'Củ dền đỏ hữu cơ', N'Rau củ quả', 38000, N'Củ dền đỏ vỏ mỏng, ruột đỏ thẫm tươi giòn, giàu chất sắt và khoáng chất tự nhiên.', N'/images/products/carot.svg'),
    (N'Dưa leo baby giòn ngọt', N'Rau củ quả', 25000, N'Dưa leo baby thu hái sáng sớm, vỏ mỏng ruột đặc giòn tan, vị ngọt thanh mát.', N'/images/products/mangtay.svg'),
    (N'Khoai tây vàng Đà Lạt', N'Rau củ quả', 30000, N'Khoai tây ruột vàng tươi, vỏ mỏng cùi mịn bở bùi, thích hợp cho nhiều món ăn gia đình.', N'/images/products/khoailang.svg'),

    -- Danh mục 3: Rau ăn lá (Thêm 7 sản phẩm)
    (N'Cải xoăn Kale hữu cơ', N'Rau ăn lá', 55000, N'Siêu thực phẩm cải xoăn kale xoăn tít, xanh mướt, giàu canxi, sắt và chất chống lão hóa.', N'/images/products/caithia.svg'),
    (N'Cải bó xôi (Rau chân vịt)', N'Rau ăn lá', 42000, N'Cải bó xôi Đà Lạt lá xanh thẫm dày dặn, giàu dinh dưỡng, rất thích hợp cho súp và nước ép.', N'/images/products/xalach.svg'),
    (N'Xà lách Romaine giòn ngọt', N'Rau ăn lá', 38000, N'Xà lách Romaine tàu lá dài giòn rụm, vị ngọt thanh, là lựa chọn số một cho món Caesar salad.', N'/images/products/xalach.svg'),
    (N'Cải ngồng Lạng Sơn', N'Rau ăn lá', 32000, N'Cải ngồng ngọn mập mạp xanh mướt, thân giòn ngọt lịm xào thịt bò hoặc luộc chấm trứng dầm tương.', N'/images/products/caithia.svg'),
    (N'Rau muống tiến vua hữu cơ', N'Rau ăn lá', 22000, N'Rau muống cọng non xanh mềm mướt, giòn ngon, không hóa chất, trồng bằng nguồn nước sạch.', N'/images/products/xalach.svg'),
    (N'Rau mồng tơi vườn quê', N'Rau ăn lá', 20000, N'Rau mồng tơi lá dày xanh mướt mọng nước, nấu canh cua đồng hoặc canh tôm thơm mát thanh nhiệt.', N'/images/products/caithia.svg'),
    (N'Cải ngọt bông vàng', N'Rau ăn lá', 26000, N'Cải ngọt cọng giòn lá mướt điểm hoa vàng tươi, vị ngọt thanh mát, nấu canh hay luộc đều tuyệt hảo.', N'/images/products/caithia.svg'),

    -- Danh mục 4: Nấm tươi & Rau gia vị (Thêm 6 sản phẩm)
    (N'Nấm mối đen hữu cơ', N'Nấm tươi & Rau gia vị', 180000, N'Nấm mối đen thịt chắc giòn sần sật, vị ngọt thơm sâu đậm đặc biệt, giá trị dinh dưỡng thượng hạng.', N'/images/products/namhuong.svg'),
    (N'Nấm đùi gà baby', N'Nấm tươi & Rau gia vị', 85000, N'Nấm đùi gà thân trắng muốt, thịt nấm giòn ngọt thơm như hạnh nhân, dùng cho lẩu và xào.', N'/images/products/namhuong.svg'),
    (N'Tỏi cô đơn Lý Sơn', N'Nấm tươi & Rau gia vị', 220000, N'Tỏi một nhánh đảo Lý Sơn, củ tròn nhỏ tép chắc nịch, thơm dịu không hăng nồng, dược tính quý.', N'/images/products/namhuong.svg'),
    (N'Gừng sẻ đồi hữu cơ', N'Nấm tươi & Rau gia vị', 45000, N'Gừng sẻ củ nhỏ nhiều nhánh, cay thơm nồng đượm, ruột vàng óng đậm tinh dầu tự nhiên.', N'/images/products/namhuong.svg'),
    (N'Hành tây tím Đà Lạt', N'Nấm tươi & Rau gia vị', 36000, N'Hành tây vỏ tím củ tròn đều, vị ngọt giòn không cay gắt, rất thích hợp làm nộm và trang trí món ăn.', N'/images/products/carot.svg'),
    (N'Sả chanh Đắk Lắk', N'Nấm tươi & Rau gia vị', 24000, N'Sả chanh cọng mập củ trắng nhiều tinh dầu thơm ngát thanh lọc không khí và dậy vị ẩm thực.', N'/images/products/caithia.svg'),

    -- Danh mục 5: Đậu & Hạt dinh dưỡng (Thêm 5 sản phẩm)
    (N'Hạt sen tươi Tịnh Tâm Huế', N'Đậu & Hạt dinh dưỡng', 160000, N'Hạt sen hồ Tịnh Tâm hạt mẩy đều, bở tơi ngọt bùi ngát hương thơm thanh tao của cố đô Huế.', N'/images/products/khoailang.svg'),
    (N'Đậu xanh lòng xanh hữu cơ', N'Đậu & Hạt dinh dưỡng', 65000, N'Đậu xanh tiêu hạt nhỏ vỏ xanh ruột xanh, nấu chè nhanh nhừ thơm lừng béo ngậy.', N'/images/products/khoailang.svg'),
    (N'Đậu đen xanh lòng Tây Nguyên', N'Đậu & Hạt dinh dưỡng', 58000, N'Đậu đen lòng xanh giàu chất chống oxy hóa, nước đen sánh bùi đậm đà, rất tốt cho sức khỏe.', N'/images/products/khoailang.svg'),
    (N'Mè đen hữu cơ mùa mới', N'Đậu & Hạt dinh dưỡng', 72000, N'Mè đen nguyên hạt thơm lừng giàu khoáng chất, vừng đều hạt không lẫn tạp chất.', N'/images/products/khoailang.svg'),
    (N'Đậu phộng đỏ đất bazan', N'Đậu & Hạt dinh dưỡng', 48000, N'Đậu phộng củ chắc mẩy vỏ lụa đỏ sẫm, vị béo ngậy thơm nức, sấy khô tự nhiên.', N'/images/products/khoailang.svg')
) v(ProductName, Category, Price, Description, ImageUrl)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products p WHERE p.ProductName = v.ProductName);
GO

-- 2. Bổ sung các Lô nông sản mùa vụ mở đặt trước (HarvestBatches)
;WITH NewSeasonalBatches AS
(
    SELECT * FROM (VALUES
        (1, N'LO-XOAICAT-016', N'Xoài cát Hòa Lộc', 800, CONVERT(datetime2, '2026-11-15'), 75000),
        (2, N'LO-MANGCUAU-017', N'Mãng cầu xiêm Đắk Lắk', 1200, CONVERT(datetime2, '2026-11-18'), 48000),
        (3, N'LO-THANHLONG-018', N'Thanh long ruột đỏ Bình Thuận', 1500, CONVERT(datetime2, '2026-11-22'), 38000),
        (1, N'LO-SAURIENG-019', N'Sầu riêng Ri6 Bến Tre', 900, CONVERT(datetime2, '2026-11-25'), 135000),
        (2, N'LO-VUSUA-020', N'Vú sữa Lò Rèn Vĩnh Kim', 750, CONVERT(datetime2, '2026-11-28'), 58000),
        (3, N'LO-NHANXUONG-021', N'Nhãn xuồng cơm vàng Tiền Giang', 1100, CONVERT(datetime2, '2026-12-01'), 55000),
        (1, N'LO-DUAHAU-022', N'Dưa hấu không hạt Mặt Trời Đỏ', 2000, CONVERT(datetime2, '2026-12-05'), 28000),

        (2, N'LO-CACHUABEEF-023', N'Cà chua beef Đà Lạt', 1000, CONVERT(datetime2, '2026-11-12'), 35000),
        (3, N'LO-BINGO-024', N'Bí ngô mật Nhật Bản', 1600, CONVERT(datetime2, '2026-11-16'), 32000),
        (1, N'LO-BAPNGOT-025', N'Bắp ngọt hữu cơ', 1800, CONVERT(datetime2, '2026-11-19'), 22000),
        (2, N'LO-OTCHUONG-026', N'Ớt chuông Đà Lạt 3 màu', 850, CONVERT(datetime2, '2026-11-24'), 65000),
        (3, N'LO-CUDEN-027', N'Củ dền đỏ hữu cơ', 950, CONVERT(datetime2, '2026-11-27'), 38000),
        (1, N'LO-DUALEO-028', N'Dưa leo baby giòn ngọt', 1300, CONVERT(datetime2, '2026-12-02'), 25000),
        (2, N'LO-KHOAITAY-029', N'Khoai tây vàng Đà Lạt', 2200, CONVERT(datetime2, '2026-12-07'), 30000),

        (3, N'LO-KALE-030', N'Cải xoăn Kale hữu cơ', 600, CONVERT(datetime2, '2026-11-10'), 55000),
        (1, N'LO-CAIBOXOI-031', N'Cải bó xôi (Rau chân vịt)', 800, CONVERT(datetime2, '2026-11-14'), 42000),
        (2, N'LO-ROMAINE-032', N'Xà lách Romaine giòn ngọt', 900, CONVERT(datetime2, '2026-11-17'), 38000),
        (3, N'LO-CAINGONG-033', N'Cải ngồng Lạng Sơn', 1100, CONVERT(datetime2, '2026-11-21'), 32000),
        (1, N'LO-RAUMUONG-034', N'Rau muống tiến vua hữu cơ', 1500, CONVERT(datetime2, '2026-11-26'), 22000),
        (2, N'LO-MONGTOI-035', N'Rau mồng tơi vườn quê', 1200, CONVERT(datetime2, '2026-11-30'), 20000),

        (3, N'LO-NAMMOIDEN-036', N'Nấm mối đen hữu cơ', 500, CONVERT(datetime2, '2026-11-23'), 180000),
        (1, N'LO-NAMDUIGA-037', N'Nấm đùi gà baby', 750, CONVERT(datetime2, '2026-11-29'), 85000),
        (2, N'LO-TOILYSON-038', N'Tỏi cô đơn Lý Sơn', 600, CONVERT(datetime2, '2026-12-08'), 220000),
        (3, N'LO-GUNGSE-039', N'Gừng sẻ đồi hữu cơ', 800, CONVERT(datetime2, '2026-12-12'), 45000),

        (1, N'LO-HATSEN-040', N'Hạt sen tươi Tịnh Tâm Huế', 700, CONVERT(datetime2, '2026-11-20'), 160000),
        (2, N'LO-DAUXANH-041', N'Đậu xanh lòng xanh hữu cơ', 1500, CONVERT(datetime2, '2026-12-03'), 65000),
        (3, N'LO-DAUDEN-042', N'Đậu đen xanh lòng Tây Nguyên', 1400, CONVERT(datetime2, '2026-12-10'), 58000),
        (1, N'LO-DAUPHONG-043', N'Đậu phộng đỏ đất bazan', 1600, CONVERT(datetime2, '2026-12-15'), 48000)
    ) v(MemberId, BatchCode, ProductName, Quantity, HarvestDate, UnitPrice)
)
INSERT INTO dbo.HarvestBatches
    (BatchCode, MemberId, ProductId, ExpectedQuantity, ActualQuantity, ExpectedHarvestDate, ActualHarvestDate, UnitPrice, Status)
SELECT b.BatchCode, b.MemberId, p.ProductId, b.Quantity, NULL, b.HarvestDate, NULL, b.UnitPrice, 1
FROM NewSeasonalBatches b
JOIN dbo.Products p ON p.ProductName = b.ProductName
JOIN dbo.Members m ON m.MemberId = b.MemberId
WHERE NOT EXISTS (SELECT 1 FROM dbo.HarvestBatches existing WHERE existing.BatchCode = b.BatchCode);
GO

PRINT N'Đã thêm hàng chục sản phẩm phong phú và các lô thu hoạch mùa vụ mở bán thành công 100%!';

