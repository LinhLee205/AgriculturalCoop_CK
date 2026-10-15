IF OBJECT_ID('dbo.UserAccounts','U') IS NULL
BEGIN
    CREATE TABLE dbo.UserAccounts (
        UserAccountId INT IDENTITY PRIMARY KEY,
        Email NVARCHAR(254) NOT NULL UNIQUE,
        DisplayName NVARCHAR(150) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role VARCHAR(16) NOT NULL
            CHECK (Role IN ('Admin','Dealer')),
        DealerId INT NULL
            REFERENCES dbo.Dealers(DealerId),
        CONSTRAINT CK_UserAccounts_RoleDealer CHECK
            ((Role='Admin' AND DealerId IS NULL)
             OR (Role='Dealer' AND DealerId IS NOT NULL))
    );
END;
