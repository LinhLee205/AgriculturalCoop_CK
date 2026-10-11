# 🌾 HỢP TÁC XÃ NÔNG SẢN: ĐẶT TRƯỚC THEO MÙA VỤ
### Hệ Thống Quản Lý Đặt Trước & Phân Bổ Sản Lượng Nông Nghiệp
**Môn học:** Lập trình Ứng dụng Web / Capstone ECO2415  
**Đề tài 16:** Agricultural Co-op Pre-order (Độ khó: ★★★)  
**Công nghệ:** .NET 8 LTS, Blazor Web App (Interactive Server), Clean Architecture 4 Tầng, Dapper / In-Memory, SQL Server 3NF.

---

## 📖 1. Giới Thiệu & Bối Cảnh Nghiệp Vụ
Mô hình sản xuất nông nghiệp tại Hợp tác xã (HTX) thường xuyên đối mặt với tính bấp bênh của thời tiết và sâu bệnh, dẫn đến **sản lượng thu hoạch thực tế thường xuyên biến động (thiếu hụt) so với dự kiến ban đầu**.

Hệ thống giải quyết 2 bài toán sống còn:
1. **Tiếp nhận đơn đặt trước (Pre-order):** Đại lý bao tiêu đặt mua số lượng nông sản trước ngày thu hoạch. Tổng đặt trước không được vượt quá năng suất dự kiến của lô mùa vụ.
2. **Phân bổ sản lượng công bằng (Allocation):** Đến ngày thu hoạch, nếu sản lượng thực tế bị hụt mùa so với tổng lượng đại lý đã đặt, hệ thống kích hoạt **thuật toán phân bổ theo tỷ lệ** kết hợp giải thuật phần dư lớn nhất (Largest Remainder Method), bảo đảm:
   $$\sum \text{AllocatedQuantity} \equiv \text{ActualQuantity}$$
   tuyệt đối không thất thoát, không thiên vị và không vượt quá năng suất vườn.

---

## 🏛️ 2. Bảng Ánh Xạ Clean Architecture Từ eShop (35 Video)

| Khái niệm trong eShop (35 Video) | Đồ án HTX Nông Sản (Đề tài 16) | Lớp / Entity Cụ Thể |
| :--- | :--- | :--- |
| **Product** | Lô thu hoạch mùa vụ / Nông sản | `HarvestBatch.cs`, `Product.cs`, `Member.cs` |
| **Customer** | Đại lý thu mua / bao tiêu sỉ | `Dealer.cs` |
| **ShoppingCart** | Giỏ đặt trước tạm thời (Scoped State) | `PreOrderCart.cs` (`IPreOrderCart`) |
| **Order & OrderLineItem** | Đơn đặt trước & Dòng đặt nông sản | `PreOrder.cs` & `PreOrderLine.cs` |
| **Admin Xử lý đơn** | Nhập sản lượng thực & Chạy phân bổ | `AllocationService.cs` & `Allocation.cs` |

### Quan hệ phụ thuộc (Dependency Rule - K1.1 = 8 Điểm):
```
CoreBusiness (Độc lập tuyệt đối, không reference project nào)
     ↑
UseCases (Chỉ reference CoreBusiness)
     ↑
DataStore (Reference CoreBusiness + UseCases)
     ↑
WebApp (Composition Root: Tham chiếu UseCases, DataStore để đăng ký DI tại Program.cs)
```

---

## ⚙️ 3. Cấu Trúc Solution & 4 Project Chuẩn

```
AgriculturalCoop
│
├── AgriculturalCoop.CoreBusiness          [Class Library - .NET 8]
│   ├── Entities                           (Member, Product, HarvestBatch, Dealer, PreOrder, PreOrderLine, Allocation)
│   ├── Exceptions                         (ExceededExpectedYieldException, PreOrderDomainException)
│   └── Services                           (AllocationService: Thuật toán phân bổ tỷ lệ + Largest Remainder)
│
├── AgriculturalCoop.UseCases              [Class Library - .NET 8]
│   ├── PluginInterfaces                   (IMemberRepository, IProductRepository, IHarvestBatchRepository...)
│   ├── Members / Products / HarvestBatches / Dealers
│   ├── PreOrders                          (CreatePreOrderUseCase: Cài đặt LUẬT 1)
│   ├── Allocations                        (AllocateHarvestUseCase: Cài đặt LUẬT 2)
│   └── AI                                 (GenerateAllocationNoticeUseCase)
│
├── AgriculturalCoop.DataStore             [Class Library - .NET 8]
│   ├── InMemory                           (InMemoryDataStore: Dữ liệu mẫu test Sprint 1)
│   └── Sql                                (SqlDataAccess, SqlRepositories dùng Dapper + Transaction K2.2)
│
├── AgriculturalCoop.WebApp                [Blazor Web App Interactive Server - .NET 8]
│   ├── Components/Shared                  (HarvestBatchCard, PreOrderLineItemComponent, AllocationResultCard, SearchBar)
│   ├── Components/Pages                   (Home, Batches, BatchDetails, Cart, MyPreOrders, MyAllocations)
│   ├── Components/Pages/Admin             (Dashboard, HarvestBatches, AllocationManager, Members, Products, PreOrders)
│   ├── State                              (PreOrderCart Scoped State, UserSessionState Role Switcher)
│   └── Program.cs                         (Composition Root đăng ký DI Container)
│
├── Database
│   └── AgriculturalCoop.sql               (Script tạo 7 bảng chuẩn 3NF, Khóa ngoại & Seed Data)
│
├── UML
│   ├── UseCase.puml
│   ├── ClassDiagram.puml
│   └── SequenceDiagram.puml
│
├── GRADING.md                             (Bảng đối soát chi tiết từng tiêu chí K1.1 - K5.3)
└── README.md
```

---

## 🚀 4. Hướng Dẫn Mở & Chạy Dự Án (Trong Visual Studio 2022)

### Cách 1: Chạy bằng Visual Studio 2022 (Bản màu tím)
1. Khởi động **Visual Studio 2022**.
2. Chọn **Open a project or solution**.
3. Trỏ tới file: `AgriculturalCoop.sln`.
4. Chuột phải vào project **`AgriculturalCoop.WebApp`** $\rightarrow$ Chọn **Set as Startup Project**.
5. Nhấn **F5** (hoặc `Ctrl + F5`) để khởi chạy.
6. Trình duyệt tự động mở ứng dụng tại `https://localhost:7xxx`.

### Cách 2: Chạy bằng Terminal / Command Prompt
```powershell
cd AgriculturalCoop.WebApp
dotnet run
```

---

## 🔀 5. Chuyển Đổi Từ In-Memory (Sprint 1) Sang SQL Server (Sprint 2)

Hệ thống được thiết kế hoàn hảo theo Clean Architecture. Để chuyển đổi nguồn dữ liệu:
1. Mở SQL Server Management Studio (SSMS), chạy file `Database/AgriculturalCoop.sql` để tạo Database `AgriculturalCoopDB` và nạp dữ liệu mẫu.
2. Mở file `AgriculturalCoop.WebApp/Program.cs`:
   - Comment khối đăng ký `InMemoryDataStore` (dòng 23 - dòng 30).
   - Bỏ comment khối đăng ký `SqlRepositories` (dòng 33 - dòng 44).
3. **Kết quả:** Toàn bộ Use Cases và giao diện Blazor `.razor` **không cần sửa một dòng code nào**, minh chứng trọn vẹn cho nguyên lý Dependency Inversion!

---

## 🎯 6. Kịch Bản Vấn Đáp & Demo 2 Luật Nghiệp Vụ (K5.2)

### 🔴 Ca 1: Demo Luật 1 – Không Đặt Vượt Sản Lượng Dự Kiến (K1.3 / K3.3)
1. Đăng nhập với vai trò **Đại lý A**.
2. Mở trang chi tiết Lô 002 (Cà chua VietGAP) - Sản lượng dự kiến: 800 kg, Hiện còn trống: 500 kg.
3. Nhập số lượng đặt: **600 kg** ($300 + 600 = 900 > 800$).
4. Bấm **Đặt trước ngay**: Hệ thống lập tức hiển thị thông báo lỗi màu đỏ cạnh form:  
   *`"Số lượng đặt trước (600 kg) vượt quá sản lượng dự kiến còn lại của lô (Còn lại: 500 kg)."`*
5. Ứng dụng không hề bị văng lỗi (`Unhandled Exception`).
6. **Chỉ vị trí code:** File `AgriculturalCoop.UseCases/PreOrders/PreOrderUseCases.cs`, dòng 48 - 63.

---

### 🟡 Ca 2: Demo Luật 2 – Thuật Toán Phân Bổ Hụt Mùa (K1.3 / K5.2)
1. Trên Top Navbar, chuyển vai trò sang **Ban Quản Trị HTX (Admin)**.
2. Vào menu **Quản Trị HTX** $\rightarrow$ **Lô Thu Hoạch & Phân Bổ** $\rightarrow$ Chọn **Lô 001 (Thanh trà Huế)**.
3. Quan sát các tham số thực tế:
   - Dự kiến: **1.000 kg**
   - Đại lý A đặt: **400 kg** | Đại lý B đặt: **300 kg** | Đại lý C đặt: **200 kg** (Tổng = **900 kg**).
   - Sản lượng thu hoạch thực tế cân được: **720 kg** (Hụt 180 kg).
4. Nhấn nút **⚡ Chạy Phân Bổ Ngay**:
   - Tỷ lệ phân bổ: $\frac{720}{900} = 80.0\%$.
   - Đại lý A nhận: $400 \times 80\% =$ **320 kg**.
   - Đại lý B nhận: $300 \times 80\% =$ **240 kg**.
   - Đại lý C nhận: $200 \times 80\% =$ **160 kg**.
   - Tổng phân bổ: $320 + 240 + 160 = \mathbf{720\ kg}$ **(Khớp chính xác 100% sản lượng thực tế, không dư không thiếu 1 kg nào!)**.
5. Bấm nút **Soạn AI**: Mở modal sinh thư thông báo lý do hụt mùa cho từng đại lý (K4.3).
6. **Chỉ vị trí code:** File `AgriculturalCoop.CoreBusiness/Services/AllocationService.cs`, dòng 32 - 165.
