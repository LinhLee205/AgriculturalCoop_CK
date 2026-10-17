USE AgriculturalCoopDB;
GO
IF OBJECT_ID('dbo.HomeBanner','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.HomeBanner', 'LogoUrl') IS NULL
        ALTER TABLE dbo.HomeBanner ADD LogoUrl NVARCHAR(500) NULL;
    IF COL_LENGTH('dbo.HomeBanner', 'BrandName') IS NULL
        ALTER TABLE dbo.HomeBanner ADD BrandName NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.HomeBanner', 'BrandTagline') IS NULL
        ALTER TABLE dbo.HomeBanner ADD BrandTagline NVARCHAR(150) NULL;
END;
GO