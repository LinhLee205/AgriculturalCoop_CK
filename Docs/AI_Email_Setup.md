# Thông báo trên website và AI hỗ trợ

Phần này chỉ cấu hình ứng dụng; không cài mô hình AI, dịch vụ nền, thay SQL Server hay thay cấu hình hệ thống của máy.

## Nếu bạn chưa biết API hay SMTP

- **API** là đường kết nối để ứng dụng nhờ dịch vụ AI bên ngoài viết lời văn. **Khóa API** giống mật khẩu riêng cho kết nối đó; **Model** là tên mô hình xử lý yêu cầu.
- **SMTP** là đường gửi email qua nhà cung cấp email. Nó độc lập với AI.
- **Manage User Secrets** là nơi Visual Studio lưu riêng cấu hình bí mật của ứng dụng, tách khỏi mã nguồn.

Bạn có thể đặt hàng, phân bổ và nhận thông báo trên website mà không cấu hình hai dịch vụ. Lưu phân bổ sẽ tạo thông báo theo mẫu từ dữ liệu thật, không gọi AI.

AI chỉ hỗ trợ nội dung bổ sung; admin duyệt trước khi đăng. Dự án ưu tiên Google Gemini; OpenAI chỉ được dùng khi chọn rõ `Provider: OpenAI`. Adapter email cũ vẫn có trong mã nguồn để mở rộng sau, nhưng giao diện hiện tại đăng thông báo trên web, không gửi email.

## Đối chiếu yêu cầu của thầy

Nguồn: `C:\Users\Admin\Downloads\capstone-topics-2026.pdf`.

- Trang 1: AI là mở rộng tùy chọn; công nghệ cho phép Gemini qua Microsoft.Extensions.AI.
- Trang 11, đề tài 16: đại lý theo dõi số lượng phân bổ; mở rộng AI là “IChatClient soạn thông báo cho đại lý về số lượng được phân bổ và lý do”. Không quy định phải gửi email hoặc dùng SMTP.
- Trang 13, mục 10: khóa trong User Secrets/biến môi trường; IChatClient inject vào UseCase riêng; có xử lý lỗi/hết hạn mức; người dùng xác nhận nội dung AI trước khi ghi CSDL.
- Trang 14, K4.3: tính năng nâng cao có thể là AI đạt đủ mục 10 hoặc xuất Excel/PDF từ dữ liệu thật. Thông báo theo mẫu không được ghi nhận là AI đã chạy thành công.

SQL dùng thêm bảng `DealerNotifications`: chạy `Database/005_DealerNotifications.sql` trên database khác khi triển khai. Script chỉ tạo bảng/chỉ mục còn thiếu, không phát thông báo cho các phân bổ cũ.

## Luồng sử dụng

1. Admin lưu phân bổ. Kết quả, trạng thái lô và thông báo theo mẫu của từng đại lý lưu cùng transaction.
2. Đại lý mở biểu tượng chuông → **Thông báo của tôi** (`/my-notifications`). Chỉ thấy thông báo thuộc tài khoản mình; có thể đánh dấu đã đọc. Số chưa đọc trên chuông cập nhật khoảng 30 giây; có nút làm mới ở hộp thông báo.
3. Admin có thể chọn **Soạn thông báo**, nhập lý do đã xác minh và chọn **Soạn bằng AI** hoặc mẫu. Bản nháp chưa được lưu thành thông báo.
4. Admin kiểm tra, chỉnh sửa, đánh dấu xác nhận rồi chọn **Duyệt và đăng trên web**. UseCase xác minh lại dữ liệu và quyền Admin trước khi lưu.
5. Nếu phân bổ lại, thông báo kết quả được cập nhật thay vì nhân đôi; nội dung bổ sung cũ được đánh dấu đã có lần phân bổ mới. Khi kết quả thay đổi, thông báo được đánh dấu chưa đọc lại.

Không gọi AI tự động khi phân bổ. Không tự đăng dữ liệu AI chưa duyệt. Không cần SMTP hoặc email đại lý cho luồng này. `ComposeAllocationNoticeWordingUseCase` inject IChatClient; adapter nhà cung cấp chỉ cấu hình trong WebApp.

## Gemini Free tier — lựa chọn cho demo

Theo [bảng giá Google](https://ai.google.dev/gemini-api/docs/pricing), một số mô hình có Free tier, gồm mô hình mẫu `gemini-3.5-flash-lite` tại thời điểm đối chiếu 15/10/2026. Đây là miễn phí trong hạn mức của dự án, không phải miễn phí không giới hạn. Mô hình và hạn mức có thể thay đổi; xem trực tiếp trong AI Studio trước khi dùng.

1. Mở [Google AI Studio](https://aistudio.google.com/), đăng nhập Google, chọn **Get API key** và tạo khóa.
2. Dùng dự án **Free tier**, không liên kết tài khoản thanh toán nếu chỉ muốn dùng miễn phí. Khóa gắn với dự án trả phí vẫn có thể bị tính phí khi gọi cùng mô hình. Ứng dụng không xác minh được gói thanh toán của Google.
3. Trong Visual Studio: bấm phải **AgriculturalCoop.WebApp → Manage User Secrets**. Thêm hoặc thay mục `AI` bên dưới, giữ nguyên các mục khác:

```json
{
  "AI": {
    "Provider": "Gemini",
    "Gemini": {
      "ApiKey": "DAN_KHOA_GEMINI_CUA_BAN_TAI_DAY",
      "Model": "gemini-3.5-flash-lite"
    }
  }
}
```

4. Khởi động lại app; mở **Admin → AI & email**, kiểm tra nhà cung cấp là **Google Gemini**. Trạng thái có cấu hình không đồng nghĩa khóa, kết nối hay Free tier đã được xác thực.
5. Lưu phân bổ → **Soạn thông báo → Soạn bằng AI**. Chỉ thao tác này mới gửi yêu cầu soạn lên dịch vụ ngoài. Kiểm tra bản nháp trước khi **Duyệt và đăng trên web**; không cần cấu hình Email.

Có thể dùng biến môi trường `GEMINI_API_KEY` thay cho `AI:Gemini:ApiKey`. Gemini không đọc `OPENAI_API_KEY` hoặc khóa OpenAI cũ trong `AI:ApiKey`. Khóa không nằm trong mã nguồn hoặc database.

Hạn mức áp dụng theo dự án và mô hình; xem [hạn mức đang có trong AI Studio](https://ai.google.dev/gemini-api/docs/rate-limits). Khi nhận lỗi 429, ứng dụng báo quá tải/hết hạn mức, giữ bản nháp cũ và cho dùng mẫu. Không tự thử lại, không đổi mô hình, không chuyển sang OpenAI hoặc bật thanh toán. Đổi nhiều khóa trong cùng dự án không tạo thêm hạn mức.

Ở Free tier, Google có thể dùng nội dung để cải thiện sản phẩm theo [điều khoản dịch vụ](https://ai.google.dev/gemini-api/terms). Dùng dữ liệu giả khi demo; không điền thông tin cá nhân hoặc bí mật vào lý do mùa vụ. Payload chỉ gồm thông tin lô/sản phẩm, các số liệu phân bổ và lý do; ứng dụng không tự gửi tên, email, điện thoại hoặc địa chỉ đại lý tới AI.

Kết nối dùng [endpoint tương thích do Google cung cấp](https://ai.google.dev/gemini-api/docs/openai) tại `https://generativelanguage.googleapis.com/v1beta/openai/`. Dự án giữ `IChatClient` và các UseCase hiện có; thư viện kết nối hiện tại gửi yêu cầu trực tiếp tới Google. Không cài mô hình hoặc dịch vụ nền trên máy.

## Cấu hình email riêng và lựa chọn OpenAI

Trong Visual Studio, bấm phải `AgriculturalCoop.WebApp` → **Manage User Secrets**. Giữ nguyên các mục hiện có và bổ sung các mục sau với thông tin của bạn:

```json
{
  "AI": {
    "Provider": "OpenAI",
    "OpenAI": {
      "ApiKey": "KHOA_OPENAI_CUA_BAN",
      "Model": "TEN_MO_HINH_TRONG_TAI_KHOAN_CUA_BAN"
    }
  },
  "Email": {
    "Host": "SMTP_CUA_NHA_CUNG_CAP",
    "Port": 587,
    "EnableSsl": true,
    "Username": "TAI_KHOAN_SMTP",
    "Password": "MAT_KHAU_UNG_DUNG_HOAC_SMTP",
    "FromAddress": "EMAIL_GUI_CUA_BAN",
    "FromName": "COOPAGRI"
  }
}
```

Nếu dùng Gemini, giữ mục `AI` theo mẫu Gemini và chỉ thêm mục `Email` từ ví dụ này. Không tạo hai mục `AI` trong cùng file. Ví dụ OpenAI chỉ dành cho người chủ động chọn dịch vụ đó; dịch vụ có thể phát sinh phí. Khi đã chọn Provider OpenAI, cấu hình cũ `AI:ApiKey`, `AI:Model` và biến `OPENAI_API_KEY` vẫn được hỗ trợ. Đặt `AI:Provider` thành `Disabled` để tắt hoàn toàn AI và dùng bản nháp theo mẫu.

SMTP dùng STARTTLS, thường là cổng 587. Cổng 465 dùng implicit TLS không được adapter hiện tại hỗ trợ. Nếu dùng Gmail và tài khoản cho phép, tạo mật khẩu ứng dụng của Google thay cho mật khẩu đăng nhập thông thường. Tên miền/địa chỉ gửi cần được nhà cung cấp cho phép.

Khởi động lại app rồi vào `/admin/integrations`. Trạng thái **Đã có cấu hình** chỉ cho biết đủ các trường bắt buộc, không phải phép thử kết nối hay gửi thư.

Không lưu khóa API/mật khẩu thật trong `appsettings.json`, Git hoặc chat. Khi triển khai, đặt chúng trong kho bí mật hoặc cấu hình của máy chủ.

## Xử lý khi chưa cấu hình hoặc dịch vụ lỗi

Nếu phản hồi 403 có nội dung `Your project has been denied access. Please contact support.`, Google đã từ chối quyền gọi AI của dự án. Kiểm tra đúng dự án ở AI Studio và liên hệ hỗ trợ Google. Việc đọc được danh sách mô hình không chứng minh quyền sinh nội dung. Không đổi SMTP hoặc bật thanh toán để “sửa” lỗi này. Thông báo theo mẫu trên web vẫn hoạt động; chưa thể lấy bản nháp theo mẫu làm bằng chứng đã chạy AI thật.

- Không có khóa/mô hình AI: nút AI bị vô hiệu hóa; vẫn tạo và sửa bản nháp theo mẫu được.
- AI lỗi, hết thời gian hoặc trả lời không hợp lệ: hiển thị lỗi, giữ nguyên bản nháp cũ; không giả nhận đây là thư do AI tạo.
- Chưa cấu hình SMTP: vô hiệu hóa gửi email, không báo gửi thành công.
- Đại lý thiếu email: chặn gửi; cập nhật hồ sơ đại lý rồi tạo lại bản nháp.
- SMTP không xác nhận hoặc mất kết nối: không tự gửi lại để tránh gửi trùng. Kiểm tra hộp thư/nhà cung cấp trước khi bấm gửi lại.

## Kiểm tra

Bộ `AgriculturalCoop.RegressionTests` kiểm tra phân quyền, dữ liệu đã lưu, từ chối bản nháp cũ, bắt buộc phê duyệt, địa chỉ người nhận và lỗi AI qua dịch vụ giả lập. Kiểm tra Gemini sử dụng HTTP giả để xác nhận endpoint Google, khóa riêng, định dạng JSON và xử lý lỗi 429/401/403/404 mà không tự thử lại hoặc đổi nhà cung cấp. Không có email thử hoặc API thật nào được gọi trong các kiểm tra này. Kết nối dịch vụ thật cần kiểm thử riêng sau khi chủ tài khoản cấu hình.

## Tài liệu chính thức

- [OpenAI: SDK .NET và thư viện](https://developers.openai.com/api/docs/libraries)
- [Microsoft.Extensions.AI.OpenAI: adapter IChatClient, tương thích .NET 8](https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI/10.11.0)
- [Microsoft: SmtpClient.SendMailAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient.sendmailasync?view=net-8.0)
