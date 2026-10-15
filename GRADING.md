# BẢNG BẰNG CHỨNG CHẤM ĐỒ ÁN CAPSTONE (GRADING.md)
## Môn học: Lập trình Ứng dụng Web / Capstone ECO2415
## Đề tài 16: HTX Nông Sản: Đặt Trước Theo Mùa Vụ (Agricultural Co-op Pre-order)
### Độ khó: ★★★ (Nâng cao) | Nền tảng: .NET 8 LTS - Blazor Web App (Interactive Server)

---

> **QUY ĐỊNH CHẤM THI CỦA GIẢNG VIÊN**:  
> Mỗi mục chấm 3 mức: `1 = Đạt đủ`, `0.5 = Đạt một phần`, `0 = Không đạt hoặc không có bằng chứng`.  
> Dưới đây là bảng trích dẫn file và dòng code chính xác trong Solution `AgriculturalCoop.sln` để phục vụ vấn đáp và chấm điểm tự động.

---

### TIÊU CHÍ 1: KIẾN TRÚC CLEAN ARCHITECTURE & CODE QUALITY (25 Điểm)

| Mã KT | Điểm Tối Đa | Nội Dung Kiểm Tra | File Bằng Chứng & Số Dòng Code |
| :--- | :---: | :--- | :--- |
| **K1.1** | **8** | Đủ 4 project; `CoreBusiness` không tham chiếu project nào; `UseCases` chỉ tham chiếu `CoreBusiness`; `WebApp` chỉ biết Plugin cụ thể tại `Program.cs`. | • `AgriculturalCoop.CoreBusiness/AgriculturalCoop.CoreBusiness.csproj` (Không có `<ProjectReference>`)<br>• `AgriculturalCoop.UseCases/AgriculturalCoop.UseCases.csproj` (Chỉ reference `CoreBusiness`)<br>• `AgriculturalCoop.DataStore/AgriculturalCoop.DataStore.csproj` (Reference `CoreBusiness` và `UseCases`)<br>• `AgriculturalCoop.WebApp/AgriculturalCoop.WebApp.csproj` (Reference `UseCases`, `DataStore`, `CoreBusiness` cho Composition Root) |
| **K1.2** | **7** | Interface repository nằm ở `UseCases`, Plugin cài đặt; file `.razor` inject interface, không inject lớp cụ thể. | • Định nghĩa Interface: `AgriculturalCoop.UseCases/PluginInterfaces/IRepositories.cs` (dòng 7 - 56)<br>• Cài đặt Plugin: `AgriculturalCoop.DataStore/InMemory/InMemoryDataStore.cs` & `AgriculturalCoop.DataStore/Sql/SqlRepositories.cs`<br>• Đăng ký DI Composition Root: `AgriculturalCoop.WebApp/Program.cs` (dòng 23 - 30)<br>• File `.razor` chỉ inject Interface: `AgriculturalCoop.WebApp/Components/Pages/Home.razor` (dòng 2: `@inject IViewOpenBatchesUseCase`) |
| **K1.3** | **6** | 2 luật nghiệp vụ của đề cài trong `CoreBusiness` / `UseCases`, không nằm trong `.razor` (3 điểm mỗi luật). | • **Luật 1 (Không đặt vượt sản lượng dự kiến)**: `AgriculturalCoop.UseCases/PreOrders/PreOrderUseCases.cs` (dòng 37 - 65)<br>• **Luật 2 (Phân bổ tỷ lệ hụt mùa + Largest Remainder)**: `AgriculturalCoop.CoreBusiness/Services/AllocationService.cs` (dòng 32 - 165) và gọi qua `AgriculturalCoop.UseCases/Allocations/AllocationUseCases.cs` (dòng 46 - 69) |
| **K1.4** | **4** | DI đăng ký đủ; giỏ/lựa chọn tạm dùng Scoped. | • `AgriculturalCoop.WebApp/Program.cs` (dòng 71: `builder.Services.AddScoped<IPreOrderCart, PreOrderCart>();`)<br>• Lớp trạng thái: `AgriculturalCoop.WebApp/State/PreOrderCart.cs` (dòng 31 - 105) |

---

### TIÊU CHÍ 2: CƠ SỞ DỮ LIỆU & TÍNH TOÀN VẸN DỮ LIỆU (25 Điểm)

| Mã KT | Điểm Tối Đa | Nội Dung Kiểm Tra | File Bằng Chứng & Số Dòng Code |
| :--- | :---: | :--- | :--- |
| **K2.1** | **8** | Từ 5 bảng, chuẩn 3NF, có khóa ngoại; script tạo bảng + dữ liệu mẫu trong repo chạy được. | • Script CSDL SQL Server 7 bảng: `Database/AgriculturalCoop.sql`<br>• 7 Bảng: `Members`, `Products`, `Dealers`, `HarvestBatches`, `PreOrders`, `PreOrderLines`, `Allocations`<br>• Khóa ngoại và CHECK constraint: dòng 58 - 105<br>• Seed Data chuẩn theo ví dụ đề bài: dòng 112 - 168 |
| **K2.2** | **6** | Cặp Header–Line ghi trong 1 Transaction duy nhất. | • **Đơn đặt trước (PreOrder + PreOrderLines)**: `AgriculturalCoop.DataStore/Sql/SqlRepositories.cs` (dòng 193 - 245: `using var transaction = conn.BeginTransaction(); ... transaction.Commit();`)<br>• **Phân bổ sản lượng hàng loạt**: `AgriculturalCoop.DataStore/Sql/SqlRepositories.cs` (dòng 307 - 345) |
| **K2.3** | **5** | Mọi truy vấn tham số hóa, không nối chuỗi SQL. | • `AgriculturalCoop.DataStore/Sql/SqlRepositories.cs` (Toàn bộ truy vấn sử dụng tham số `@Param` của Dapper, tuyệt đối không có `$"SELECT ..."` hay cộng chuỗi) |
| **K2.4** | **6** | Luồng giao dịch chính chạy end-to-end và dữ liệu nằm trong SQL Server. | • Xem tại: `AgriculturalCoop.DataStore/Sql/SqlRepositories.cs` và màn hình Quản lý Phân bổ `AgriculturalCoop.WebApp/Components/Pages/Admin/AllocationManager.razor` |

---

### TIÊU CHÍ 3: GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (20 Điểm)

| Mã KT | Điểm Tối Đa | Nội Dung Kiểm Tra | File Bằng Chứng & Số Dòng Code |
| :--- | :---: | :--- | :--- |
| **K3.1** | **6** | Ít nhất 3 component tái sử dụng có `[Parameter]` / `EventCallback`. | 1. `AgriculturalCoop.WebApp/Components/Shared/HarvestBatchCard.razor` (`[Parameter] Batch`, `EventCallback<HarvestBatch> OnOrderClick`)<br>2. `AgriculturalCoop.WebApp/Components/Shared/PreOrderLineItemComponent.razor` (`[Parameter] Item`, `EventCallback OnRemove`, `EventCallback OnQuantityChanged`)<br>3. `AgriculturalCoop.WebApp/Components/Shared/AllocationResultCard.razor` (`[Parameter] Item`, `EventCallback OnGenerateNotice`)<br>4. `AgriculturalCoop.WebApp/Components/Shared/SearchBar.razor` (`[Parameter] SearchTerm`, `EventCallback SearchTermChanged`) |
| **K3.2** | **5** | `EditForm` + `DataAnnotations`; lỗi nhập liệu hiển thị cạnh ô nhập. | • `AgriculturalCoop.WebApp/Components/Pages/BatchDetails.razor` (dòng 92 - 148: `<EditForm Model="@orderModel">`, `<DataAnnotationsValidator />`, `<ValidationMessage For="@(() => orderModel.Quantity)" />`) |
| **K3.3** | **4** | Ca vi phạm 2 luật nghiệp vụ hiện thông báo thân thiện, không văng lỗi. | • Bắt lỗi Luật 1: `AgriculturalCoop.WebApp/Components/Pages/BatchDetails.razor` (dòng 97 - 107 & 195 - 200: bắt `ExceededExpectedYieldException` và render alert thân thiện) |
| **K3.4** | **5** | Dùng được trên màn hình điện thoại (DevTools bề rộng 390px). | • Top Navbar Responsive: `AgriculturalCoop.WebApp/Components/Layout/TopNavBar.razor` (dòng 18 - 20)<br>• Tối ưu mobile 390px, table cuộn ngang `table-responsive`: `AgriculturalCoop.WebApp/wwwroot/app.css` (dòng 58 - 72) |

---

### TIÊU CHÍ 4: BẢO MẬT, PHÂN QUYỀN & TÍNH NĂNG NÂNG CAO / AI (15 Điểm)

| Mã KT | Điểm Tối Đa | Nội Dung Kiểm Tra | File Bằng Chứng & Số Dòng Code |
| :--- | :---: | :--- | :--- |
| **K4.1** | **4** | Cookie authentication; hai vai trò Dealer/Admin, lối đăng nhập riêng cho từng vai trò. | • `AgriculturalCoop.WebApp/Security/AccountEndpoints.cs:15`: khai báo hai endpoint; dòng 120: xác thực mật khẩu và kiểm tra đúng vai trò trước khi tạo cookie.<br>• `AgriculturalCoop.WebApp/Program.cs:115`: chuyển khách chưa đăng nhập quản trị tới /admin/login; dòng 129: xác minh lại tài khoản từ cookie.<br>• `AgriculturalCoop.WebApp/State/UserSessionState.cs:8`: trạng thái chỉ đọc từ ClaimsPrincipal. |
| **K4.2** | **5** | Khách/đại lý gõ URL quản trị bị chặn; không cấp quyền qua giao diện. | • `AgriculturalCoop.WebApp/Components/Pages/Admin/Dashboard.razor:3` và `AgriculturalCoop.WebApp/Components/Pages/Admin/Banner.razor:3`: `[Authorize(Roles = "Admin")]`.<br>• `AgriculturalCoop.RegressionTests/Program.cs`: kiểm tra truy cập trực tiếp, đăng nhập sai vai trò và giao diện khách hàng không chứa liên kết admin. |
| **K4.3** | **6** | Theo PDF trang 13 mục 10 và trang 14: AI đạt đủ yêu cầu, hoặc xuất Excel/PDF từ dữ liệu thật. Không bắt buộc gửi email. Luồng hiện tại: thông báo web theo mẫu khi lưu phân bổ; AI soạn bổ sung cần admin duyệt trước khi lưu. | • `AgriculturalCoop.UseCases/AI/ComposeAllocationNoticeWordingUseCase.cs:9`: inject IChatClient trong UseCase riêng, xử lý lỗi/hết hạn mức.<br>• `AgriculturalCoop.UseCases/AI/DealerNotificationUseCases.cs:31`: kiểm tra Admin, phê duyệt và bản nháp còn khớp trước khi đăng.<br>• `AgriculturalCoop.WebApp/Services/NoticeAiConnection.cs:29`: đọc khóa từ cấu hình riêng.<br>• `AgriculturalCoop.RegressionTests/NotificationChecks.cs:13`: quyền sở hữu thông báo, bản nháp chưa duyệt, dữ liệu phân bổ thay đổi.<br>• `Docs/AI_Email_Setup.md`: đối chiếu PDF gốc và cấu hình. Cần demo AI thật thành công để khẳng định đạt đủ nhánh AI; lỗi Google 403 hiện tại chưa đáp ứng bằng chứng đó. |

---

### TIÊU CHÍ 5: HỒ SƠ THIẾT KẾ (UML) & THUYẾT TRÌNH VẤN ĐÁP (15 Điểm)

| Mã KT | Điểm Tối Đa | Nội Dung Kiểm Tra | File Bằng Chứng & Số Dòng Code |
| :--- | :---: | :--- | :--- |
| **K5.1** | **6** | Use Case, Class, Sequence (PlantUML) khớp tên lớp và luồng trong code. | • Use Case Diagram: `UML/UseCase.puml`<br>• Class Diagram: `UML/ClassDiagram.puml`<br>• Sequence Diagram: `UML/SequenceDiagram.puml` |
| **K5.2** | **6** | Vấn đáp: mỗi thành viên chỉ đúng dòng cài luật và giải thích Dependency Rule. | • Xem phần Hướng Dẫn Vấn Đáp chi tiết trong `README.md` |
| **K5.3** | **3** | Mọi thành viên có commit, rải trên ít nhất 3 tuần. | • Lịch sử commit theo 3 Sprint chuẩn: Tuần 5 (Architecture & In-Memory), Tuần 6 (SQL Dapper & Business Rules), Tuần 7 (UI Polish, AI & Defense). |
