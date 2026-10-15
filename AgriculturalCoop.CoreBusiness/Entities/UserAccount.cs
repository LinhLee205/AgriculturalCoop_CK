namespace AgriculturalCoop.CoreBusiness.Entities;

public class UserAccount
{
    public int UserAccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Dealer";
    public int? DealerId { get; set; }
}

public class MemberRevenue
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int BatchCount { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal CommittedAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
}
