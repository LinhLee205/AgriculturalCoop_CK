-- 009_OrderPaymentFields.sql
-- Thêm thông tin thanh toán: PaymentMethod (COD / BANK_TRANSFER), PaymentStatus (Chưa thanh toán / Đã thanh toán), TransferContent (Nội dung chuyển khoản)

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PreOrders') AND name = N'PaymentMethod')
BEGIN
    ALTER TABLE dbo.PreOrders ADD PaymentMethod NVARCHAR(50) NOT NULL CONSTRAINT DF_PreOrders_PaymentMethod DEFAULT N'COD';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PreOrders') AND name = N'PaymentStatus')
BEGIN
    ALTER TABLE dbo.PreOrders ADD PaymentStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_PreOrders_PaymentStatus DEFAULT N'Chưa thanh toán';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PreOrders') AND name = N'TransferContent')
BEGIN
    ALTER TABLE dbo.PreOrders ADD TransferContent NVARCHAR(100) NULL;
END
GO

PRINT N'Thêm các cột thanh toán vào bảng PreOrders thành công!';

