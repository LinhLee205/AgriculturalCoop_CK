using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.Accounts;
using Microsoft.AspNetCore.Identity;

namespace AgriculturalCoop.WebApp.Security;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<UserAccount> hasher = new();

    public string Hash(UserAccount account, string password) =>
        hasher.HashPassword(account, password);

    public bool Verify(UserAccount account, string password) =>
        hasher.VerifyHashedPassword(
            account, account.PasswordHash, password)
        != PasswordVerificationResult.Failed;
}
