USE AgriculturalCoopDB;
GO
IF OBJECT_ID('dbo.HomeBanner','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.HomeBanner', 'IntroImageUrl') IS NULL
        ALTER TABLE dbo.HomeBanner ADD IntroImageUrl NVARCHAR(500) NULL;
END;
GO

