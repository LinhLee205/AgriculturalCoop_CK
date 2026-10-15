USE [AgriculturalCoopDB];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.HarvestBatches','IsArchived') IS NULL
    ALTER TABLE dbo.HarvestBatches ADD IsArchived BIT NOT NULL CONSTRAINT DF_HarvestBatch_IsArchived DEFAULT 0;
IF COL_LENGTH('dbo.Dealers','IsActive') IS NULL
    ALTER TABLE dbo.Dealers ADD IsActive BIT NOT NULL CONSTRAINT DF_Dealer_IsActive DEFAULT 1;
IF COL_LENGTH('dbo.PreOrders','CancellationReason') IS NULL
    ALTER TABLE dbo.PreOrders ADD CancellationReason NVARCHAR(500) NULL;
COMMIT TRANSACTION;
GO
