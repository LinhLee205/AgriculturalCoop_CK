# 🌾 BÁO CÁO ĐỒ ÁN CAPSTONE ECO2415
## ĐỀ TÀI 16: HTX NÔNG SẢN – ĐẶT TRƯỚC THEO MÙA VỤ
*(Agricultural Co-op Pre-order — Độ khó: ★★★)*

---

## 📌 SLIDE 1: TỔNG QUAN ĐỀ TÀI & BỐI CẢNH
### 1. Bối cảnh thực tiễn
* Hợp tác xã (HTX) nông nghiệp sản xuất và phân phối các loại nông sản sạch (thanh trà, bưởi da xanh, rau hữu cơ) cho hệ thống đại lý bao tiêu sỉ.
* Thu hoạch nông nghiệp chịu tác động lớn bởi thời tiết, dẫn đến sản lượng thực tế thường xuyên biến động (thiếu hụt) so với dự kiến khi nhận đơn.

### 2. Mục tiêu hệ thống
* Số hóa kênh tiếp nhận đơn đặt trước (Pre-order) từ các đại lý.
* Xây dựng cơ chế **phân bổ tự động, công bằng và minh bạch** khi sản lượng thực tế hụt mùa.
* Tự động tổng hợp doanh thu và sản lượng của từng hộ xã viên (`Member`).

### 3. Nền tảng công nghệ
* **Framework:** .NET 8 / .NET 10 LTS, Blazor Web App (Interactive Server).
* **Kiến trúc:** Clean Architecture 4 tầng chuẩn (`CoreBusiness`, `UseCases`, `Plugins/DataStore`, `WebApp`).
* **CSDL:** SQL Server + Dapper / EF Core; giao dịch với Transaction.

---

## 📌 SLIDE 2: MÔ HÌNH DỮ LIỆU & ÁNH XẠ ESHOP CHUẨN
### 1. Bảng ánh xạ Clean Architecture eShop
* **Product** $\rightarrow$ Lô thu hoạch mùa vụ (`HarvestBatch`), Nông sản (`Product`), Xã viên (`Member`).
* **Customer** $\rightarrow$ Đại lý bao tiêu / thu mua (`Dealer`).
* **ShoppingCart** $\rightarrow$ Giỏ đặt trước tạm thời (Scoped Service: `PreOrderCartService`).
* **Order & OrderLine** $\rightarrow$ Đơn đặt trước & Dòng đặt (`PreOrder` & `PreOrderLine`).
* **Admin Xử lý đơn** $\rightarrow$ Nhập sản lượng thực & Chạy phân bổ (`Allocation`).

### 2. Hai vai trò người dùng (K4.1)
* **Đại lý (Dealer):** Xem danh mục lô sắp thu hoạch; đặt trước số lượng; xem lịch sử và số lượng hàng thực tế được phân bổ.
* **Ban Quản Lý HTX (Admin):** Bảo vệ bằng `[Authorize(Roles="Admin")]`. CRUD xã viên, tạo lô mùa vụ, nhập sản lượng thực thu hoạch, kích hoạt thuật toán phân bổ.

---

## 📌 SLIDE 3: 2 LUẬT NGHIỆP VỤ BẮT BUỘC (CORE BUSINESS RULES)
> *Toàn bộ cài đặt tại tầng `CoreBusiness` / `UseCases`, không đặt trong UI (.razor).*

### 🟢 Luật 1: Kiểm soát hạn mức đặt trước theo dự kiến (K1.3)
* **Quy tắc:** Tổng khối lượng đặt trước của các đại lý không được vượt quá sản lượng dự kiến của lô.
$$\sum \text{PreOrderLine.Quantity} \le \text{HarvestBatch.ExpectedYield}$$
* **Xử lý vi phạm:** Ném lỗi Domain Exception hiển thị cạnh ô nhập trên `EditForm` khi đại lý cố tình đặt quá số lượng khả dụng còn lại của lô.

### 🟡 Luật 2: Thuật toán phân bổ hụt sản lượng (K2.2 / K5.2)
* **Quy tắc:** Khi thu hoạch thực tế < tổng lượng đặt trước:
  * Phân bổ theo tỷ lệ hoặc FIFO (ưu tiên đại lý đặt trước).
  * Làm tròn số nguyên không vượt quá sản lượng thực tế; số dư bù trừ vào đơn ưu tiên.
$$\sum \text{Allocation.ActualQuantity} \equiv \text{HarvestBatch.ActualYield}$$
* **Kỹ thuật Transaction:** Bản ghi phân bổ được ghi trong **1 Transaction duy nhất** đảm bảo dữ liệu toàn vẹn tuyệt đối.

---

## 📌 SLIDE 4: KẾ HOẠCH PHÂN BỔ 3 TUẦN (05/10 – 24/10)

| Giai đoạn | Thời gian | Trọng tâm & Nhiệm vụ chính | Sản phẩm nộp (Deliverables) |
| :--- | :--- | :--- | :--- |
| **Sprint 1** | **05/10 – 11/10** | • Thiết kế ERD 3NF (7 bảng: `Member`, `Product`, `HarvestBatch`, `Dealer`, `PreOrder`, `PreOrderLine`, `Allocation`).<br>• Dựng Solution 4 Project Clean Architecture.<br>• Viết Mock In-Memory Repository & sơ đồ Use Case + Class. | • Solution build thành công.<br>• Tài liệu thiết kế ban đầu.<br>• Chạy thử luồng In-Memory. |
| **Sprint 2** | **12/10 – 18/10** | • Script SQL Server tạo bảng, khóa ngoại và nạp dữ liệu mẫu.<br>• Cài đặt Plugin Dapper/EF Core với Transaction cho Header–Line (K2.2).<br>• Cài đặt 2 Luật nghiệp vụ vào `CoreBusiness` & `UseCases`.<br>• Cấu hình Cookie Authentication & phân quyền `[Authorize]`. | • Luồng giao dịch chính chạy end-to-end trên SQL Server.<br>• Demo vi phạm 2 luật bị chặn. |
| **Sprint 3** | **19/10 – 24/10** | • Tối ưu giao diện Responsive đạt chuẩn mobile 390px (K3.4).<br>• Tách 3 component dùng `[Parameter]` & `EventCallback`.<br>• Tích hợp AI: `IChatClient` hỗ trợ soạn thông báo phân bổ cho đại lý.<br>• Hoàn thành file `GRADING.md` trích dẫn file và dòng code, tổng duyệt vấn đáp. | • Đồ án hoàn chỉnh 100%.<br>• File `GRADING.md` khớp code.<br>• Sơ đồ PlantUML đầy đủ. |

---

## 📌 SLIDE 5: TÍNH NĂNG MỞ RỘNG (AI) & TIÊU CHÍ NGHIỆM THU
### 1. Tính năng AI: Trợ lý thông báo phân bổ (Mục 10)
* Dùng `Microsoft.Extensions.AI (IChatClient)` inject qua UseCase riêng (`GenerateAllocationNoticeUseCase`).
* Tự động tổng hợp số liệu hụt mùa từ `HarvestBatch` và `Allocation` để soạn thảo email/tin nhắn thông báo lý do minh bạch cho đại lý.
* Bản nháp hiển thị cho Ban quản lý duyệt trước khi lưu/gửi; API key lưu an toàn trong User Secrets.

### 2. Tiêu chuẩn đánh giá điểm tối đa (Phiếu chấm K1.1 – K5.3)
* **K1.1 – K1.4:** 4 project tuân thủ nghiêm ngặt Dependency Rule, Cart dùng Scoped Service.
* **K2.1 – K2.4:** 3NF $\ge$ 5 bảng, Transaction cho Header–Line, 100% tham số hóa SQL chống SQL Injection.
* **K3.1 – K3.4:** Đủ 3 reusable components, `EditForm` + `DataAnnotations`, responsive tốt ở độ rộng 390px.
* **K4.1 – K4.3:** Phân quyền 2 vai trò, chặn gõ URL quản trị, tính năng AI hoạt động thực tế.
* **K5.1 – K5.3:** Sơ đồ PlantUML khớp tên lớp, vấn đáp giải thích chuẩn dòng code, Git commit rải đều 3 tuần.
