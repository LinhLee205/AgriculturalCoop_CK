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

        public bool TryLoginAdmin(string username, string password, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return false;
            }

            var cleanUser = username.Trim().ToLowerInvariant();
            var cleanPass = password.Trim();

            // Mật khẩu và tài khoản quản trị hợp lệ
            bool isUserValid = cleanUser == "admin" || cleanUser == "admin@coop.vn" || cleanUser == "bqt";
            bool isPassValid = cleanPass == "admin123" || cleanPass == "admin" || cleanPass == "123456" || cleanPass == "admin@123";

            if (isUserValid && isPassValid)
            {
                CurrentRole = AppRole.Admin;
                CurrentUserName = cleanUser.Contains("@") ? cleanUser : "admin@coop.vn";
                errorMessage = string.Empty;
                OnSessionChanged?.Invoke();
                return true;
            }

            errorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác! (Gợi ý: admin / admin123)";
            return false;
        }

        public void LogoutAdmin()
        {
            // Trở về vai trò đại lý mặc định
            CurrentRole = AppRole.Dealer;
            CurrentDealerId = 1;
            CurrentDealerName = "Đại lý Nông sản An Phú (Đại lý A)";
            CurrentUserName = "dealer_a@anphu.vn";
            OnSessionChanged?.Invoke();
        }

        public void Logout()
        {
            LogoutAdmin();
        }
    }
}
