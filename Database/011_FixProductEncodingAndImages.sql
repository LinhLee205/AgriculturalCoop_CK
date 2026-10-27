-- 011_FixProductEncodingAndImages.sql
-- Sửa triệt để lỗi bảng mã tiếng Việt UTF-8 cho tất cả sản phẩm
-- Đồng bộ toàn bộ hình ảnh sản phẩm về dạng SVG nền trắng sạch (chỉ có sản phẩm là có màu)
-- Viết lại mô tả chi tiết, nhiều thông tin hữu ích về nguồn gốc, quy trình, dinh dưỡng và hương vị.

SET NOCOUNT ON;

-- 1. Cập nhật nhóm 5 sản phẩm đầu tiên (ID: 1 - 5)
UPDATE dbo.Products
SET ProductName = N'Thanh trà Thủy Biều (Đặc sản Huế)',
    Category = N'Trái cây đặc sản',
    StandardPrice = 9000,
    Description = N'Thanh trà Thủy Biều trứ danh Cố Đô Huế, tép mọng nước, vị ngọt thanh mát tao nhã cùng hương thơm dịu nhẹ. Canh tác tự nhiên ven bờ sông Hương, giàu vitamin C và khoáng chất thanh nhiệt bồi bổ cơ thể.',
    ImageUrl = N'/images/products/thanhtra.svg'
WHERE ProductId = 1;

UPDATE dbo.Products
SET ProductName = N'Cà chua bi hữu cơ VietGAP',
    Category = N'Rau củ quả',
    StandardPrice = 25000,
    Description = N'Cà chua bi giống Hà Lan trồng trong nhà kính công nghệ cao đạt chuẩn VietGAP. Trái tròn đều, sắc đỏ tươi căng bóng, vị ngọt đậm đà xen chút chua thanh tự nhiên, dồi dào lycopene và vitamin A.',
    ImageUrl = N'/images/products/cachua.svg'
WHERE ProductId = 2;

UPDATE dbo.Products
SET ProductName = N'Bưởi da xanh ruột hồng',
    Category = N'Trái cây đặc sản',
    StandardPrice = 45000,
    Description = N'Bưởi da xanh Bến Tre vỏ mỏng dễ bóc, tép bưởi hồng tươi ráo múi mọng nước, vị ngọt thanh mát không đắng the. Đạt chuẩn xuất khẩu GlobalGAP, rất thích hợp dùng tráng miệng hoặc ép nước giải khát.',
    ImageUrl = N'/images/products/buoi.svg'
WHERE ProductId = 3;

UPDATE dbo.Products
SET ProductName = N'Rau cải thìa thủy canh',
    Category = N'Rau ăn lá',
    StandardPrice = 18000,
    Description = N'Cải thìa trồng theo mô hình thủy canh hồi lưu khép kín, bẹ lá xanh mướt mập mạp giòn ngọt, tuyệt đối không tồn dư chất bảo vệ thực vật. Giàu chất xơ, vitamin C, K và canxi tự nhiên cho mọi bữa cơm.',
    ImageUrl = N'/images/products/caithia.svg'
WHERE ProductId = 4;

UPDATE dbo.Products
SET ProductName = N'Rau Xà lách thủy canh',
    Category = N'Rau ăn lá',
    StandardPrice = 22000,
    Description = N'Xà lách thủy canh sạch mơn mởn, tàu lá xoăn mềm mại giòn ngọt thanh tao. Thu hoạch sáng sớm giữ nguyên dưỡng chất và độ tươi non, là nguyên liệu hoàn hảo cho các món salad trộn và món cuốn.',
    ImageUrl = N'/images/products/xalach.svg'
WHERE ProductId = 5;

-- 2. Cập nhật nhóm sản phẩm mở rộng (ID: 1005 - 1036)
UPDATE dbo.Products
SET ProductName = N'Xoài cát Hòa Lộc',
    Category = N'Trái cây đặc sản',
    StandardPrice = 75000,
    Description = N'Xoài cát Hòa Lộc loại 1 tuyển chọn từ vùng Cái Bè - Tiền Giang. Trái thuôn dài, vỏ vàng ươm khi chín, thịt quả dẻo mịn không xơ, vị ngọt lịm đậm đà ngát hương thơm quý phái.',
    ImageUrl = N'/images/products/xoaicat.svg'
WHERE ProductId = 1005;

UPDATE dbo.Products
SET ProductName = N'Mãng cầu xiêm Đắk Lắk',
    Category = N'Trái cây đặc sản',
    StandardPrice = 48000,
    Description = N'Mãng cầu xiêm tuyển chọn từ vùng đất đỏ bazan Đắk Lắk. Quả to tròn đều gai, thịt trắng phau dày múi, vị chua ngọt thanh mát giàu vitamin C, canxi và chất chống oxy hóa tự nhiên.',
    ImageUrl = N'/images/products/bosap.svg'
WHERE ProductId = 1006;

UPDATE dbo.Products
SET ProductName = N'Thanh long ruột đỏ Bình Thuận',
    Category = N'Trái cây đặc sản',
    StandardPrice = 38000,
    Description = N'Thanh long ruột đỏ vỏ mỏng tai xanh, thịt quả đỏ thẫm mọng nước, vị ngọt thanh đậm đà. Chứa hàm lượng anthocyanin cao hỗ trợ tim mạch và ngăn ngừa lão hóa tế bào.',
    ImageUrl = N'/images/products/dautay.svg'
WHERE ProductId = 1007;

UPDATE dbo.Products
SET ProductName = N'Sầu riêng Ri6 Bến Tre',
    Category = N'Trái cây đặc sản',
    StandardPrice = 135000,
    Description = N'Sầu riêng Ri6 chín tự nhiên tại Chợ Lách - Bến Tre, cơm vàng ươm hạt lép, từng múi khô ráo dẻo quánh, vị béo ngậy ngọt đậm ngào ngạt không bị sượng hay nhão nước.',
    ImageUrl = N'/images/products/saurieng.svg'
WHERE ProductId = 1008;

UPDATE dbo.Products
SET ProductName = N'Vú sữa Lò Rèn Vĩnh Kim',
    Category = N'Trái cây đặc sản',
    StandardPrice = 58000,
    Description = N'Vú sữa Lò Rèn chính gốc Tiền Giang, vỏ mỏng bóng loáng ánh tím nhạt, dòng sữa trắng ngà thơm phức tan trên đầu lưỡi, vị ngọt dịu thanh tao mát lạnh.',
    ImageUrl = N'/images/products/oinuhoang.svg'
WHERE ProductId = 1009;

UPDATE dbo.Products
SET ProductName = N'Nhãn xuồng cơm vàng Tiền Giang',
    Category = N'Trái cây đặc sản',
    StandardPrice = 55000,
    Description = N'Nhãn xuồng quả to tròn ráo nước, cùi thịt dày giòn sần sật vàng ngà óng ả, hạt nhỏ hạt tiêu, vị ngọt thanh tự nhiên và hương thơm ngạt ngào đặc trưng Nam Bộ.',
    ImageUrl = N'/images/products/oinuhoang.svg'
WHERE ProductId = 1010;

UPDATE dbo.Products
SET ProductName = N'Dưa hấu không hạt Mặt Trời Đỏ',
    Category = N'Trái cây đặc sản',
    StandardPrice = 28000,
    Description = N'Dưa hấu Mặt Trời Đỏ ruột đỏ au rực rỡ, thịt dưa chắc nịch không hạt, mọng nước, độ ngọt Brix cao 12-13%, là thức quả giải nhiệt thanh mát tuyệt vời mùa vụ hè thu.',
    ImageUrl = N'/images/products/dualuoi.svg'
WHERE ProductId = 1011;

UPDATE dbo.Products
SET ProductName = N'Cà chua beef Đà Lạt',
    Category = N'Rau củ quả',
    StandardPrice = 35000,
    Description = N'Cà chua beef Đà Lạt quả to chắc nịch, thịt quả dày cùi nhiều nạc, ít hạt, vị chua ngọt hài hòa. Rất được các nhà hàng cao cấp ưa chuộng làm món salad, burger và nước sốt pasta.',
    ImageUrl = N'/images/products/cachua.svg'
WHERE ProductId = 1012;

UPDATE dbo.Products
SET ProductName = N'Bí ngô mật Nhật Bản',
    Category = N'Rau củ quả',
    StandardPrice = 32000,
    Description = N'Bí ngô hạt đậu giống Nhật canh tác hữu cơ, ruột vàng cam đậm đà, khi hấp hoặc nấu súp cho độ dẻo quánh bùi béo tự nhiên, dồi dào beta-carotene bổ mắt và dưỡng chất.',
    ImageUrl = N'/images/products/carot.svg'
WHERE ProductId = 1013;

UPDATE dbo.Products
SET ProductName = N'Bắp ngọt hữu cơ',
    Category = N'Rau củ quả',
    StandardPrice = 22000,
    Description = N'Bắp ngọt hạt căng mọng sữa vàng ươm, xếp đều tăm tắp, vị ngọt thanh giòn tan khi luộc chín, không sử dụng chất biến đổi gen, giàu chất xơ hòa tan tốt cho tiêu hóa.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1014;

UPDATE dbo.Products
SET ProductName = N'Ớt chuông Đà Lạt 3 màu',
    Category = N'Rau củ quả',
    StandardPrice = 65000,
    Description = N'Ớt chuông đỏ, vàng, xanh thu hoạch từ cao nguyên mát mẻ, thịt quả dày giòn ngọt không cay hăng, dồi dào vitamin A và C gấp nhiều lần trái cây thông thường.',
    ImageUrl = N'/images/products/mangtay.svg'
WHERE ProductId = 1015;

UPDATE dbo.Products
SET ProductName = N'Củ dền đỏ hữu cơ',
    Category = N'Rau củ quả',
    StandardPrice = 38000,
    Description = N'Củ dền đỏ ruột tím thẫm mọng nước, vị ngọt dịu tự nhiên, trồng bằng phân hữu cơ sinh học, giàu khoáng chất sắt, folate và kali giúp bổ máu và thanh lọc cơ thể.',
    ImageUrl = N'/images/products/carot.svg'
WHERE ProductId = 1016;

UPDATE dbo.Products
SET ProductName = N'Dưa leo baby giòn ngọt',
    Category = N'Rau củ quả',
    StandardPrice = 25000,
    Description = N'Dưa leo baby thu hái sáng sớm khi còn đọng sương, vỏ mỏng ruột đặc giòn rụm không hạt, không bị đắng, ăn trực tiếp thanh mát hoặc làm salad giòn tan.',
    ImageUrl = N'/images/products/mangtay.svg'
WHERE ProductId = 1017;

UPDATE dbo.Products
SET ProductName = N'Khoai tây vàng Đà Lạt',
    Category = N'Rau củ quả',
    StandardPrice = 30000,
    Description = N'Khoai tây ruột vàng tươi xuất xứ vùng đất mùn Đà Lạt, củ tròn nhẵn vỏ mỏng, khi chế biến có độ bở bùi dẻo thơm ngào ngạt, không dư lượng thuốc bảo vệ thực vật.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1018;

UPDATE dbo.Products
SET ProductName = N'Cải xoăn Kale hữu cơ',
    Category = N'Rau ăn lá',
    StandardPrice = 55000,
    Description = N'Siêu thực phẩm xanh cải xoăn kale xoăn tít xanh đậm, dồi dào diệp lục, lutein, canxi và vitamin K. Là lựa chọn số một cho nước ép detox thanh lọc cơ thể và tăng cường miễn dịch.',
    ImageUrl = N'/images/products/caithia.svg'
WHERE ProductId = 1019;

UPDATE dbo.Products
SET ProductName = N'Cải bó xôi (Rau chân vịt)',
    Category = N'Rau ăn lá',
    StandardPrice = 42000,
    Description = N'Cải bó xôi Đà Lạt lá to dày xanh biếc mượt mà, chứa lượng sắt, canxi và axit folic vượt trội. Rất tốt cho sức khỏe mọi lứa tuổi, thích hợp nấu canh tôm thịt hoặc làm súp kem.',
    ImageUrl = N'/images/products/xalach.svg'
WHERE ProductId = 1020;

UPDATE dbo.Products
SET ProductName = N'Xà lách Romaine giòn ngọt',
    Category = N'Rau ăn lá',
    StandardPrice = 38000,
    Description = N'Xà lách Romaine tàu lá dài giòn rụm, sống lá mọng nước vị ngọt thanh dịu. Lựa chọn số một cho món Caesar salad trứ danh, giàu vitamin A và chất xơ thanh mát.',
    ImageUrl = N'/images/products/xalach.svg'
WHERE ProductId = 1021;

UPDATE dbo.Products
SET ProductName = N'Cải ngồng Lạng Sơn',
    Category = N'Rau ăn lá',
    StandardPrice = 32000,
    Description = N'Cải ngồng ngọn mập mạp xanh tươi, thân non giòn ngọt lịm. Thích hợp xào tỏi, xào thịt bò hoặc luộc chấm trứng dầm nước tương thơm lừng chuẩn phong vị miền bắc.',
    ImageUrl = N'/images/products/caithia.svg'
WHERE ProductId = 1022;

UPDATE dbo.Products
SET ProductName = N'Rau muống tiến vua hữu cơ',
    Category = N'Rau ăn lá',
    StandardPrice = 22000,
    Description = N'Rau muống cọng non xanh giòn ngọt, trồng bằng nguồn nước sạch mát tự nhiên, không hóa chất kích thích. Khi luộc nước rau trong vắt xanh tươi, giòn ngon chuẩn vị.',
    ImageUrl = N'/images/products/xalach.svg'
WHERE ProductId = 1023;

UPDATE dbo.Products
SET ProductName = N'Rau mồng tơi vườn quê',
    Category = N'Rau ăn lá',
    StandardPrice = 20000,
    Description = N'Mồng tơi cọng tía lá dày bóng mượt mọng nước, tính mát giải nhiệt mùa hè. Rất thơm ngon khi nấu canh cua đồng, canh ngao hoặc tôm băm, mang lại cảm giác thanh mát.',
    ImageUrl = N'/images/products/caithia.svg'
WHERE ProductId = 1024;

UPDATE dbo.Products
SET ProductName = N'Cải ngọt bông vàng',
    Category = N'Rau ăn lá',
    StandardPrice = 26000,
    Description = N'Cải ngọt cọng giòn lá mướt điểm hoa vàng tươi rực rỡ, vị ngọt thanh lành tính, dễ chế biến các món canh sườn non, xào thịt bò hay nấu mì thanh đạm.',
    ImageUrl = N'/images/products/caithia.svg'
WHERE ProductId = 1025;

UPDATE dbo.Products
SET ProductName = N'Nấm mối đen hữu cơ',
    Category = N'Nấm tươi & Rau gia vị',
    StandardPrice = 180000,
    Description = N'Nấm mối đen nuôi trồng công nghệ vi sinh hiện đại, thân nấm chắc giòn sần sật, vị ngọt thơm sâu lắng đặc trưng thượng hạng, giàu protein thực vật và khoáng chất quý.',
    ImageUrl = N'/images/products/namhuong.svg'
WHERE ProductId = 1026;

UPDATE dbo.Products
SET ProductName = N'Nấm đùi gà baby',
    Category = N'Nấm tươi & Rau gia vị',
    StandardPrice = 85000,
    Description = N'Nấm đùi gà baby mũ tròn thân trắng muốt, thịt nấm giòn ngọt thơm thoang thoảng vị hạnh nhân, chuyên dùng cho các món lẩu, nướng mỡ hành hoặc kho tiêu chay đậm vị.',
    ImageUrl = N'/images/products/namhuong.svg'
WHERE ProductId = 1027;

UPDATE dbo.Products
SET ProductName = N'Tỏi cô đơn Lý Sơn',
    Category = N'Nấm tươi & Rau gia vị',
    StandardPrice = 220000,
    Description = N'Tỏi một nhánh đặc sản đảo núi lửa Lý Sơn, củ tròn nhỏ tép chắc nịch, mùi thơm cay dịu tinh tế không hăng nồng, chứa hàm lượng allicin kháng sinh tự nhiên dồi dào.',
    ImageUrl = N'/images/products/namhuong.svg'
WHERE ProductId = 1028;

UPDATE dbo.Products
SET ProductName = N'Gừng sẻ đồi hữu cơ',
    Category = N'Nấm tươi & Rau gia vị',
    StandardPrice = 45000,
    Description = N'Gừng sẻ củ nhỏ nhiều nhánh trồng trên đất đồi dốc tự nhiên, cay nồng đượm, ruột vàng óng dồi dào tinh dầu ấm nóng, là gia vị quý cho ẩm thực và bài thuốc dưỡng sinh.',
    ImageUrl = N'/images/products/namhuong.svg'
WHERE ProductId = 1029;

UPDATE dbo.Products
SET ProductName = N'Hành tây tím Đà Lạt',
    Category = N'Nấm tươi & Rau gia vị',
    StandardPrice = 36000,
    Description = N'Hành tây vỏ tím đậm củ tròn đều, mọng nước, vị ngọt giòn thanh nhẹ không cay hăng mắt, tạo màu sắc bắt mắt và hương vị độc đáo cho mọi đĩa salad tươi ngon.',
    ImageUrl = N'/images/products/carot.svg'
WHERE ProductId = 1030;

UPDATE dbo.Products
SET ProductName = N'Sả chanh Đắk Lắk',
    Category = N'Nấm tươi & Rau gia vị',
    StandardPrice = 24000,
    Description = N'Sả chanh Đắk Lắk cọng mập củ trắng nõn, dồi dào tinh dầu hương chanh the mát quyến rũ, dùng tẩm ướp món ăn hoặc đun nước xông phòng thanh lọc không khí.',
    ImageUrl = N'/images/products/caithia.svg'
WHERE ProductId = 1031;

UPDATE dbo.Products
SET ProductName = N'Hạt sen tươi Tịnh Tâm Huế',
    Category = N'Đậu & Hạt dinh dưỡng',
    StandardPrice = 160000,
    Description = N'Hạt sen tươi hồ Tịnh Tâm hạt mẩy tròn đều, bóc vỏ bỏ tim tỉ mỉ, khi nấu chín hạt bở tơi ngọt bùi thơm lừng hương hoa sen thanh tao, an thần định tâm dễ ngủ.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1032;

UPDATE dbo.Products
SET ProductName = N'Đậu xanh lòng xanh hữu cơ',
    Category = N'Đậu & Hạt dinh dưỡng',
    StandardPrice = 65000,
    Description = N'Đậu xanh tiêu hạt nhỏ vỏ xanh thẫm ruột xanh óng ả, không pha lẫn tạp chất, nấu chè hay đồ xôi nhanh nhừ sánh mịn thơm bùi ngậy béo tự nhiên.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1033;

UPDATE dbo.Products
SET ProductName = N'Đậu đen xanh lòng Tây Nguyên',
    Category = N'Đậu & Hạt dinh dưỡng',
    StandardPrice = 58000,
    Description = N'Đậu đen ruột xanh hạt mẩy bóng chắc, nấu nước đen nhánh sánh bùi, dồi dào anthocyanin và khoáng chất quý giúp thanh lọc cơ thể và nuôi dưỡng mái tóc khỏe đẹp.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1034;

UPDATE dbo.Products
SET ProductName = N'Mè đen hữu cơ mùa mới',
    Category = N'Đậu & Hạt dinh dưỡng',
    StandardPrice = 72000,
    Description = N'Mè đen nguyên chất mùa vụ mới thu hoạch, hạt đều mẩy đen láy không lẫn cát bụi, rang thơm lừng ngào ngạt, giàu canxi thực vật, vitamin E và omega-3 có lợi cho tim mạch.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1035;

UPDATE dbo.Products
SET ProductName = N'Đậu phộng đỏ đất bazan',
    Category = N'Đậu & Hạt dinh dưỡng',
    StandardPrice = 48000,
    Description = N'Đậu phộng giống củ chắc mẩy vỏ lụa đỏ au, trồng trên đất bazan màu mỡ, hạt to tròn béo ngậy thơm nức khi rang giòn, là nguồn năng lượng dồi dào tự nhiên mỗi ngày.',
    ImageUrl = N'/images/products/khoailang.svg'
WHERE ProductId = 1036;

PRINT N'Hoàn tất cập nhật 37 sản phẩm với bảng mã UTF-8 tiếng Việt chuẩn, ảnh nền trắng và mô tả giàu chi tiết!';

