-- Additive migration: keeps all existing orders and business data.
USE [AgriculturalCoopDB];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.PreOrders', 'RecipientName') IS NULL
    ALTER TABLE dbo.PreOrders ADD RecipientName NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.PreOrders', 'ContactPhone') IS NULL
    ALTER TABLE dbo.PreOrders ADD ContactPhone NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.PreOrders', 'DeliveryAddress') IS NULL
    ALTER TABLE dbo.PreOrders ADD DeliveryAddress NVARCHAR(255) NULL;
COMMIT TRANSACTION;
GO
