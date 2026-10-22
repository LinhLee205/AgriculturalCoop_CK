# COOPAGRI — Hợp tác xã nông sản đặt trước theo mùa vụ

Đề tài 16, Capstone ECO2415. Ứng dụng dùng .NET 8, Blazor Interactive Server, Dapper và SQL Server; kiến trúc gồm CoreBusiness → UseCases → DataStore → WebApp.

## Chạy với SQL Server hiện có

Cấu hình Development đã trỏ tới máy `LAPTOP-1C71MGVB`, database `AgriculturalCoopDB`, sử dụng tài khoản Windows đang chạy ứng dụng:

```json
{
  "DataStore": { "Provider": "SqlServer" },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=LAPTOP-1C71MGVB;Initial Catalog=AgriculturalCoopDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

Chuỗi kết nối phát triển nằm trong `AgriculturalCoop.WebApp/appsettings.Development.json`. Có thể ghi đè bằng Manage User Secrets trong Visual Studio hoặc biến môi trường `ConnectionStrings__DefaultConnection`. Không lưu mật khẩu SQL thật trong repository. Khi triển khai, cấu hình kết nối và chứng chỉ phù hợp trên máy chủ.

Ứng dụng đọc/ghi trực tiếp SQL Server, không tự tạo database, không chạy lại script seed và không tự chuyển sang dữ liệu mẫu khi SQL lỗi. Database cần các bảng `Members`, `Products`, `HarvestBatches`, `Dealers`, `PreOrders`, `PreOrderLines`, `Allocations`, `UserAccounts`.

Nếu database chỉ thiếu bảng tài khoản, chạy `Database/002_Accounts.sql` trong `AgriculturalCoopDB`. Script này có thể chạy lại, chỉ bổ sung bảng còn thiếu, không tạo sẵn tài khoản. **Không chạy `Database/AgriculturalCoop.sql` trên database đang sử dụng:** đó là script demo cũ có lệnh xóa bảng và nạp dữ liệu mẫu.

Chạy thêm `Database/004_OrderDeliveryContact.sql` để bổ sung người nhận, điện thoại và địa chỉ vào đơn đặt. Script chỉ thêm ba cột còn thiếu, không sửa đơn cũ. Đơn mới bắt buộc nhập thông tin nhận hàng trước khi xác nhận.

Tại thư mục chứa solution:

```powershell
dotnet restore AgriculturalCoop.sln
dotnet build AgriculturalCoop.sln
dotnet run --project AgriculturalCoop.WebApp --launch-profile http
```

Hoặc mở solution trong Visual Studio, đặt `AgriculturalCoop.WebApp` làm Startup Project và chọn profile `http`/`https` trong Development.

1. Mở `http://localhost:5189/account/setup` để tự tạo admin đầu tiên. Trang và endpoint thiết lập chỉ hoạt động qua địa chỉ loopback trong Development, khi chưa có admin.
2. Ban quản lý đăng nhập tại `/admin/login`, sau đó được chuyển tới `/admin`. Đại lý đăng nhập riêng tại `/account/login`.
3. Đại lý đăng ký tại `/account/register`, sau đó đăng nhập để đặt hàng. Không có tài khoản/mật khẩu mặc định và không có nút tự đổi vai trò.

Mật khẩu cần 10–128 ký tự, gồm chữ và số, được lưu dưới dạng hash. Email đăng ký tối đa 100 ký tự, điện thoại tối đa 20 ký tự để tương thích bảng Dealers hiện có.

## Chức năng đã bổ sung

- Xác thực cookie, phân quyền Admin/Dealer ở tuyến truy cập, chống CSRF và giới hạn số lần gọi endpoint tài khoản.
- Admin có thanh quản lý riêng, dashboard và liên kết trở về trang chủ.
- Thêm/sửa/xóa xã viên, nông sản và lô thu hoạch. Chặn xóa hồ sơ có giao dịch; giữ xã viên, nông sản, đơn giá đã chốt và khóa sửa lô đã thu hoạch.
- Đặt trước kiểm tra tổng các dòng cùng lô; SQL kiểm tra lại trong transaction Serializable để tránh đặt vượt khi có nhiều đơn đồng thời. Đơn giá được lấy từ database.
- Phân bổ tỷ lệ theo phần dư lớn nhất đến 0,01 kg. Tổng phân bổ bằng giá trị nhỏ hơn giữa sản lượng thực tế và tổng lượng đặt; lượng dư được giữ lại khi thu hoạch vượt nhu cầu.
- Báo cáo `/admin/revenue` lọc theo ngày đặt đơn, tách giá trị đặt trước và giá trị đã phân bổ, loại đơn hủy. Giá trị phân bổ chưa phải tiền đã thu.
- Tải ảnh PNG/JPEG/WebP tối đa 5 MB; lưu bản sao trên máy chủ và dùng ảnh thay thế khi đường dẫn ảnh lỗi.

Ảnh nằm trong `AgriculturalCoop.WebApp/App_Data/product-images`; SQL chỉ lưu URL. Khóa cookie nằm trong `App_Data/keys`. Sao lưu database và thư mục `App_Data` khi chuyển máy; có thể chỉ định nơi lưu qua `Storage:Root`. Thư mục này được loại khỏi Git.

Khi sửa SQL trực tiếp trong SSMS, tải lại trang để xem dữ liệu mới. Giao diện chưa có đồng bộ đẩy tức thời.

## Kiểm tra

```powershell
dotnet run --project AgriculturalCoop.RegressionTests
# Kiểm tra SQL và lưu banner trong giao dịch hoàn tác:
dotnet run --project AgriculturalCoop.RegressionTests -- --sql
```

Bộ kiểm tra chạy 1.000 tình huống phân bổ, kiểm tra quy tắc đặt hàng, bảo vệ hồ sơ, ảnh, đăng ký/đăng nhập/đăng xuất và phân quyền qua HTTP. Máy chủ thử nghiệm dùng InMemory trong Development, không ghi tài khoản thử vào SQL. Lựa chọn `--sql` đọc cấu hình Development hiện tại; phép thử lưu banner chạy trong transaction và được rollback, không thay đổi dữ liệu đang dùng.

## Phạm vi còn lại

Lưu phân bổ tạo thông báo trên website cho từng đại lý trong cùng transaction với kết quả và trạng thái lô. Hộp thông báo nằm tại `/my-notifications`, có trạng thái đã đọc/chưa đọc và chuông trên thanh tài khoản. SQL cần `Database/005_DealerNotifications.sql` (bổ sung bảng, không sửa dữ liệu cũ). Admin có thể soạn thêm bằng mẫu hoặc AI; chỉ sau khi duyệt, nội dung bổ sung mới được đăng trên web. Không cần SMTP cho luồng này. AI được inject qua IChatClient vào `ComposeAllocationNoticeWordingUseCase`, xử lý lỗi mà vẫn giữ ứng dụng hoạt động. Xem [hướng dẫn và đối chiếu PDF gốc](Docs/AI_Email_Setup.md). Xuất Excel/PDF chưa được triển khai.

AI mặc định chọn Google Gemini, cấu hình bằng `AI:Gemini:ApiKey` trong User Secrets hoặc `GEMINI_API_KEY`. Mẫu hiện dùng `gemini-3.5-flash-lite`, có Free tier khi đối chiếu bảng giá Google ngày 15/10/2026. Cần dùng khóa thuộc dự án Free tier chưa liên kết thanh toán để tránh phí; ứng dụng không kiểm tra được gói Google. Hết hạn mức thì báo lỗi và cho dùng mẫu, không tự chuyển sang OpenAI. Muốn dùng OpenAI phải chủ động đặt `AI:Provider` thành `OpenAI`; email hoạt động độc lập với nhà cung cấp AI. Chi tiết và cấu hình mẫu nằm trong hướng dẫn kết nối ở trên.

Các sơ đồ nằm trong `UML/`; tài liệu báo cáo cũ nằm trong `Docs/` và cần đối chiếu với mã nguồn hiện tại khi chuẩn bị bảo vệ.

## Giao diện cửa hàng và banner

Trang khách hàng chỉ liệt kê lô đang mở đặt và nông sản có ít nhất một lô đang mở. Sản phẩm chưa có lô hoặc chỉ có lô đang ẩn/đã thu hoạch không xuất hiện. Admin → Lô thu hoạch có nút mở đặt hoặc tạm ẩn cho lô chưa thu hoạch; dữ liệu cũ và đơn đã đặt được giữ nguyên.

Quản lý banner tại /admin/banner: thay tiêu đề, mô tả, ảnh và nút liên kết. Banner lưu trong bảng SQL HomeBanner; chạy Database/003_HomeBanner.sql khi cài đặt trên database khác. Ảnh dùng cùng kho lưu trữ App_Data/product-images.

Nút Xem trang khách hàng trong admin mở tab mới. Tab khách hàng dùng giao diện cửa hàng; phiên đăng nhập vẫn dùng chung giữa các tab của cùng trình duyệt.

Khi đang đăng nhập đại lý mà nhập `/admin` hoặc đường dẫn quản trị con, ứng dụng đưa tới đăng nhập Admin và giữ đường dẫn cần mở. Chỉ sau khi xác thực tài khoản Admin thành công mới vào được trang đó; đăng nhập sai không thay đổi phiên đại lý. Đăng nhập Admin thành công chuyển phiên chung của trình duyệt sang Admin.

Giao diện khách hàng dùng màu thương hiệu #80B155; logo và nội dung dùng chung khung căn lề. Thanh điều hướng khách hàng chỉ hiển thị đăng nhập/đăng ký đại lý hoặc thông tin đại lý đã đăng nhập, không hiển thị liên kết về admin. Hai trang đăng nhập kiểm tra vai trò từ tài khoản đã xác thực trước khi tạo cookie; xem cửa hàng không tự đăng xuất admin.

Trang sản phẩm có chọn lô, số lượng, thêm giỏ và tiếp tục đặt trước; bên dưới là mô tả, các lô đang mở của sản phẩm và lô mở đặt khác được gợi ý. Tất cả cách đặt đều qua giỏ/xác nhận, bắt buộc nhập người nhận, điện thoại, địa chỉ rồi mới tạo đơn. Admin thấy thông tin này tại danh sách đơn và trong màn hình phân bổ của từng lô. Màn hình phân bổ liệt kê đơn ngay cả khi chưa chạy phân bổ; đơn đã hủy không tham gia tính hạn mức hoặc phân bổ.

Menu tài khoản chỉ hiện khi bấm biểu tượng người dùng; đại lý đã đăng nhập có avatar chữ cái đầu.

## Hoàn thiện quản trị và vòng đời giao dịch

Chạy `Database/006_AdminLifecycle.sql` khi triển khai trên database khác. Script chỉ bổ sung trạng thái lưu trữ lô, hoạt động đại lý và lý do hủy đơn; không xóa dữ liệu. Database AgriculturalCoopDB trên máy phát triển đã được bổ sung.

Chạy `Database/008_SeasonalProductsAndBatches.sql` trên database đã có dữ liệu để thêm 12 nông sản và các lô mùa vụ từ 05/11/2026 đến 08/12/2026. Script có thể chạy lại mà không nhân bản sản phẩm hoặc lô, và cập nhật ảnh 4 nông sản ban đầu sang hình minh họa nền trắng.

Trang `/` giới thiệu mùa vụ và tối đa bốn sản phẩm gợi ý. `/products` là danh mục riêng có tìm kiếm, lọc và sắp xếp, không lặp banner. `/batches` xem theo từng lô. Tiêu đề banner không còn khung viền.

Admin quản lý đại lý tại `/admin/dealers`, xem đơn liên quan và trạng thái giao dịch. Hồ sơ đại lý và tài khoản đăng nhập được giữ riêng: thêm hồ sơ không tạo mật khẩu, sửa email liên hệ không đổi email đăng nhập. Ngừng giao dịch chặn đăng nhập và đặt đơn mới, giữ đơn cũ.

Tại `/admin/orders`, tìm theo mã đơn, đại lý, lô hoặc trạng thái; mở `/admin/orders/{id}` để xem địa chỉ, số điện thoại, các dòng đặt và phân bổ. Đơn chờ có thể xác nhận; hủy chỉ áp dụng khi chưa có lô phân bổ/hoàn tất và phải ghi lý do. Đơn hủy không chiếm hạn mức, nhưng vẫn giữ quan hệ để đối chiếu lịch sử.

Lô có giao dịch dùng Lưu trữ/Khôi phục thay vì xóa cả chuỗi dữ liệu. Mặc định danh sách ẩn lô lưu trữ; chọn bộ lọc để xem lại. Lưu trữ lô đang mở sẽ đóng nhận đặt; khôi phục không tự mở đặt. Hoàn tất giao hàng chỉ sau khi đã phân bổ, sau đó không thay đổi sản lượng/phân bổ. Xóa chỉ dành cho lô trống chưa thu hoạch; đơn và tài khoản liên quan cũng chặn xóa đại lý.

Chi tiết phạm vi và các kiểm tra: [Rà soát quản trị](Docs/Admin_Review.md).
