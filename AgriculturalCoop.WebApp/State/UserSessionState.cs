using System;

namespace AgriculturalCoop.WebApp.State
{
    public enum AppRole
    {
        Dealer,
        Admin
    }

    /// <summary>
    /// Quản lý phiên làm việc, hỗ trợ 2 vai trò: Đại lý (Dealer) và Ban quản lý HTX (Admin) (K4.1).
    /// </summary>
    public class UserSessionState
    {
        public AppRole CurrentRole { get; private set; } = AppRole.Dealer;
        public int CurrentDealerId { get; private set; } = 1;
        public string CurrentDealerName { get; private set; } = "Đại lý Nông sản An Phú (Đại lý A)";
        public string CurrentUserName { get; private set; } = "dealer_a@anphu.vn";

        public bool IsAdmin => CurrentRole == AppRole.Admin;
        public bool IsDealer => CurrentRole == AppRole.Dealer;

        public event Action? OnSessionChanged;

        public void SwitchToDealer(int dealerId, string dealerName, string userName)
        {
            CurrentRole = AppRole.Dealer;
            CurrentDealerId = dealerId;
            CurrentDealerName = dealerName;
            CurrentUserName = userName;
            OnSessionChanged?.Invoke();
        }

        public void SwitchToAdmin()
        {
            CurrentRole = AppRole.Admin;
            CurrentUserName = "admin@coop.vn";
            OnSessionChanged?.Invoke();
        }

        public void Logout()
        {
            // Reset to default dealer demo
            CurrentRole = AppRole.Dealer;
            CurrentDealerId = 1;
            CurrentDealerName = "Đại lý Nông sản An Phú (Đại lý A)";
            CurrentUserName = "dealer_a@anphu.vn";
            OnSessionChanged?.Invoke();
        }
    }
}
