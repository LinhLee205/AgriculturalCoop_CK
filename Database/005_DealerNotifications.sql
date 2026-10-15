USE [AgriculturalCoopDB];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.DealerNotifications', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DealerNotifications (
        DealerNotificationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DealerId INT NOT NULL REFERENCES dbo.Dealers(DealerId),
        HarvestBatchId INT NOT NULL REFERENCES dbo.HarvestBatches(HarvestBatchId),
        PreOrderLineId INT NOT NULL REFERENCES dbo.PreOrderLines(PreOrderLineId),
        Kind INT NOT NULL CHECK (Kind IN (0,1)),
        Subject NVARCHAR(160) NOT NULL,
        Body NVARCHAR(MAX) NOT NULL,
        IsAiGenerated BIT NOT NULL DEFAULT 0,
        IsSuperseded BIT NOT NULL DEFAULT 0,
        UpdatedAt DATETIME2 NOT NULL,
        ReadAt DATETIME2 NULL,
        CONSTRAINT UQ_DealerNotification_LineKind UNIQUE (PreOrderLineId,Kind)
    );
    CREATE INDEX IX_DealerNotifications_Dealer ON dbo.DealerNotifications(DealerId,UpdatedAt DESC);
END;
COMMIT TRANSACTION;
GO
