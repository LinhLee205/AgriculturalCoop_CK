using System.ComponentModel.DataAnnotations;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Accounts;

public interface IPasswordService
{
    string Hash(UserAccount account, string password);
    bool Verify(UserAccount account, string password);
}

public interface IAccountUseCases
{
    Task<UserAccount?> LoginAsync(string email, string password);

    Task RegisterAsync(
        string email, string name, string phone,
        string password, string confirmation,
        bool initialAdmin = false);

    Task<bool> HasAdminAsync();
}

public class AccountUseCases(
    IUserAccountRepository repository,
    IPasswordService passwords, IDealerRepository dealers) : IAccountUseCases
{
    public Task<bool> HasAdminAsync() => repository.HasAdminAsync();

    public async Task<UserAccount?> LoginAsync(string email, string password)
    {
        var account = await repository.FindByEmailAsync(
            email.Trim().ToLowerInvariant());

        if (account?.Role == "Dealer" && (!account.DealerId.HasValue ||
            (await dealers.GetDealerByIdAsync(account.DealerId.Value)) is not { IsActive: true })) return null;

        return account != null && passwords.Verify(account, password)
            ? account : null;
    }

    public async Task RegisterAsync(
        string email, string name, string phone,
        string password, string confirmation,
        bool initialAdmin = false)
    {
        email = email.Trim().ToLowerInvariant();
        name = name.Trim();

        if (email.Length > 100 ||
            !new EmailAddressAttribute().IsValid(email) ||
            name.Length is < 2 or > 150 ||
            phone.Length > 20)
        {
            throw new PreOrderDomainException(
                "Nhập email hợp lệ tối đa 100 ký tự, tên từ 2 đến 150 ký tự và số điện thoại tối đa 20 ký tự.");
        }

        if (password.Length is < 10 or > 128 ||
            !password.Any(char.IsLetter) ||
            !password.Any(char.IsDigit))
        {
            throw new PreOrderDomainException(
                "Mật khẩu cần 10–128 ký tự, gồm chữ và số.");
        }

        if (password != confirmation)
            throw new PreOrderDomainException("Mật khẩu xác nhận chưa khớp.");

        var account = new UserAccount
        {
            Email = email,
            DisplayName = name
        };

        account.PasswordHash = passwords.Hash(account, password);
        await repository.CreateAsync(account, phone.Trim(), initialAdmin);
    }
}
