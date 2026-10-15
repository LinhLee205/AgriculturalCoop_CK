USE AgriculturalCoopDB;
GO
IF OBJECT_ID('dbo.HomeBanner','U') IS NULL
BEGIN
    CREATE TABLE dbo.HomeBanner (
        Id INT NOT NULL PRIMARY KEY CHECK (Id=1),
        Eyebrow NVARCHAR(60) NOT NULL,
        Title NVARCHAR(140) NOT NULL,
        Description NVARCHAR(400) NOT NULL,
        ButtonText NVARCHAR(40) NOT NULL,
        ButtonUrl NVARCHAR(200) NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL
    );
END;
