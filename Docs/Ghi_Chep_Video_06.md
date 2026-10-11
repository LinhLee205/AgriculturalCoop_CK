# Ghi Chép Chi Tiết: Video 06 - View Shopping Cart Use Case & Components
**Khóa học**: Part 3 - Build Customer Portal  
**Thời lượng video**: 47 phút 22 giây  
**Công nghệ sử dụng**: C#, ASP.NET Core Blazor, Bootstrap 4, Clean Architecture, JS Interop  

---

> [!NOTE]  
> Tài liệu này ghi lại toàn bộ lời giảng của thầy, mốc thời gian, vị trí thư mục/file trong Visual Studio Solution Explorer và code chi tiết theo từng bước triển khai chức năng Giỏ hàng (**Shopping Cart**).

---

## 1. Mục tiêu bài học
1. **Xây dựng Use Case Xem giỏ hàng**:
   - Tạo class `ViewShoppingCartUseCase` và trích xuất interface `IViewShoppingCartUseCase`.
   - Tương tác với abstraction `IShoppingCart` (được triển khai bằng LocalStorage plugin).
2. **Xây dựng Trang giỏ hàng (`ShoppingCartComponent.razor`)**:
   - Cấu hình route `@page "/cart"`.
   - Bố cục lưới 2 cột: Cột trái chứa danh sách sản phẩm, cột phải chứa bảng tóm tắt đơn hàng.
   - Xử lý vòng đời Blazor với `OnAfterRenderAsync` và JS Interop (`LocalStorage`).
3. **Áp dụng nguyên lý phân tách thành phần (Separation of Concerns)**:
   - Tạo **`LineItemComponent.razor`**: Hiển thị từng dòng sản phẩm (ảnh, tên, số lượng, giá, nút xóa).
   - Tạo **`OrderSummaryComponent.razor`**: Hiển thị bảng tổng kết tiền hàng, phí ship, thuế và nút thanh toán (Place Order).
4. **Đăng ký Dependency Injection** trong `Program.cs` và xử lý các lỗi runtime (`InvalidOperationException`, `NullReferenceException`).

---

## 2. Bảng tiến trình chi tiết theo từng phút

| Mốc thời gian | Lời giảng & Giải thích của Thầy | Vị trí thao tác trong Visual Studio | Thao tác & Nội dung Code |
| :--- | :--- | :--- | :--- |
| **00:00 - 00:55** | Thầy mở đầu: Tiếp tục bài học Blazor, hôm nay sẽ thực hành tạo các component và màn hình cho tính năng Giỏ hàng (Shopping Cart). Trước đó ta đã có màn hình tìm kiếm và xem chi tiết sản phẩm. | Visual Studio (Solution Explorer) | Quan sát cấu trúc dự án hiện tại (`eShop.UseCases`, `eShop.Web.CustomerPortal`). |
| **00:56 - 02:30** | Thầy giải thích: Khi khách hàng chọn mua sản phẩm, ta cần một màn hình giỏ hàng để họ quản lý đơn mua. Bắt đầu bằng việc tạo thư mục và file Use Case tương ứng trong tầng Core/UseCases. | **Project: `eShop.UseCases`**<br>📁 Tạo folder: `ShoppingCartScreen` | Chuột phải `ShoppingCartScreen` ➔ **Add** ➔ **Class...** ➔ Đặt tên: **`ViewShoppingCartUseCase.cs`**. |
| **02:31 - 04:30** | Thầy viết phương thức `Execute()` trả về một `Task<Order>`. Thầy giải thích vì sao phải dùng `Task`: Do thao tác lấy giỏ hàng từ LocalStorage chạy qua JavaScript Interop bất đồng bộ (Asynchronous). | File: **`ViewShoppingCartUseCase.cs`** | Viết method `public Task<Order> Execute()`. Import namespace `eShop.CoreBusiness.Models`. |
| **04:31 - 06:00** | Thầy tạo Constructor và Dependency Injection cho `IShoppingCart` vào `ViewShoppingCartUseCase`. Phương thức `Execute()` sẽ gọi `shoppingCart.GetOrderAsync()`. | File: **`ViewShoppingCartUseCase.cs`** | Khai báo `private readonly IShoppingCart shoppingCart;`<br>Tạo constructor gán tham số và return `shoppingCart.GetOrderAsync()`. |
| **06:01 - 07:27** | Thầy sử dụng tính năng **Extract Interface** của Visual Studio (Quick Actions `Ctrl + .`) để tạo interface `IViewShoppingCartUseCase`. Thầy tạo thêm thư mục con `Interfaces` để gom gọn interface lại. | **Project: `eShop.UseCases`**<br>📁 `ShoppingCartScreen/Interfaces` | Tạo folder `Interfaces` ➔ Kéo file `IViewShoppingCartUseCase.cs` vào đây ➔ Cập nhật lại namespace `eShop.UseCases.ShoppingCartScreen.Interfaces`. |
| **07:28 - 08:35** | Thầy chuyển sang tầng giao diện `CustomerPortal` để tạo màn hình Razor Component hiển thị giỏ hàng. | **Project: `eShop.Web.CustomerPortal`**<br>📁 Thư mục: `Pages` | Chuột phải `Pages` ➔ **Add** ➔ **Razor Component...** ➔ Đặt tên: **`ShoppingCartComponent.razor`**. |
| **08:36 - 09:50** | Thầy mở trình duyệt, vào trang thương mại điện tử thực tế (**Shopee**) để minh họa cho học viên thấy một trang Giỏ hàng tiêu chuẩn gồm những thông tin gì: Ảnh, tên sản phẩm, đơn giá, số lượng (tăng/giảm), nút xóa và bảng tổng tiền bên cạnh. | Trình duyệt Web (Shopee.vn) | Phân tích bố cục giỏ hàng của Shopee. |
| **09:51 - 11:30** | Thầy quay lại code file `ShoppingCartComponent.razor`. Khai báo directive `@page "/cart"` và tiêu đề `<h3>Shopping Cart</h3>`. Khai báo biến trạng thái nội bộ `private Order order;`. | File: **`ShoppingCartComponent.razor`** | Thêm `@page "/cart"` và biến `private Order order;` trong `@code`. |
| **11:31 - 13:10** | Thầy giải thích về vòng đời component: Vì dữ liệu giỏ hàng lưu ở LocalStorage của trình duyệt (thông qua JS Interop), ta **không thể** gọi nó ở `OnInitialized` mà bắt buộc phải chạy ở **`OnAfterRenderAsync`** sau khi trang đã render xong lần đầu (`firstRender == true`). | File: **`ShoppingCartComponent.razor`** | Viết phương thức `protected override async Task OnAfterRenderAsync(bool firstRender)`. |
| **13:11 - 15:58** | Thầy inject interface `IViewShoppingCartUseCase` vào trang. Trong `OnAfterRenderAsync`, nếu là `firstRender`, thầy gọi `order = await viewShoppingCartUseCase.Execute();` rồi gọi tiếp **`StateHasChanged()`** để cập nhật lại UI. | File: **`ShoppingCartComponent.razor`** | Thêm `@inject IViewShoppingCartUseCase viewShoppingCartUseCase`<br>Viết logic nạp dữ liệu và gọi `StateHasChanged()`. |
| **15:59 - 18:35** | Thầy mở bảng trắng (**Whiteboard**) vẽ sơ đồ bố cục: Bố cục gồm 1 hàng (`class="row"`) chia thành 2 cột: Cột 1 (bên trái) hiển thị danh sách các `LineItem`, Cột 2 (bên phải) hiển thị bảng tóm tắt `Summary`. Các tính năng cần có: Xem, đổi số lượng, xóa và nút Place Order. | Bảng vẽ Whiteboard | Vẽ sơ đồ layout giỏ hàng (Row & Columns). |
| **18:36 - 21:00** | Thầy viết mã HTML Bootstrap cho layout: Thẻ bao `<div class="row">`, cột trái là `<div class="col-6">`, cột phải là `<div class="col">`. | File: **`ShoppingCartComponent.razor`** | Tạo khung `row` và 2 `col` theo phân tích bảng trắng. |
| **21:01 - 24:20** | Thầy dựng vòng lặp `for (int i = 0; i < order.LineItems.Count; i++)` trong thẻ `<ul>` với `list-style: none;` để chuẩn bị lặp qua từng mặt hàng trong đơn. | File: **`ShoppingCartComponent.razor`** | Dùng vòng lặp `for` để duyệt qua từng `order.LineItems`. |
| **24:21 - 25:10** | Thầy áp dụng nguyên lý *Separation of Concerns*: Không nhồi nhét toàn bộ HTML của từng sản phẩm vào trang cha, mà tạo một component con riêng tên là `LineItemComponent.razor`. | **Project: `eShop.Web.CustomerPortal`**<br>📁 Thư mục: `Controls` | Chuột phải `Controls` ➔ **Add** ➔ **Razor Component...** ➔ Đặt tên: **`LineItemComponent.razor`**. |
| **25:11 - 26:30** | Trong `LineItemComponent.razor`, thầy khai báo `[Parameter] public OrderLineItem LineItem { get; set; }`. Viết điều kiện kiểm tra `@if (LineItem != null && LineItem.Product != null)` để chống lỗi NullReference. | File: **`LineItemComponent.razor`** | Khai báo Parameter `LineItem` và điều kiện kiểm tra null. |
| **26:31 - 28:40** | Thầy xây dựng giao diện cột cho mặt hàng: Cột 1 chứa ảnh sản phẩm `<img src="@LineItem.Product.ImageLink" />`. Cột 2 chứa tên sản phẩm in đậm trong thẻ `<h5>`. | File: **`LineItemComponent.razor`** | Viết HTML hiển thị ảnh và tên mặt hàng. |
| **28:41 - 31:00** | Thầy viết phần hiển thị số lượng với thẻ `<input type="number" min="0" value="@LineItem.Quantity" />` và giá tiền sản phẩm `LineItem.Product.Price.ToString("c")`. | File: **`LineItemComponent.razor`** | Thêm input điều chỉnh số lượng và hiển thị giá tiền. |
| **31:01 - 33:36** | Thầy thêm nút xóa sản phẩm `<button class="btn btn-light">Delete</button>`. Thầy viết sự kiện `@onclick` sử dụng **Lambda Expression** `@(() => DeleteProduct(LineItem.ProductId))` vì cần truyền ID sản phẩm vào phương thức xóa. | File: **`LineItemComponent.razor`** | Thêm nút Delete và khai báo phương thức `DeleteProduct(int productId)`. |
| **33:37 - 36:30** | Thầy quay lại `ShoppingCartComponent.razor`, thay thế mã tạm trong vòng lặp bằng component con vừa tạo: `<LineItemComponent LineItem="order.LineItems[i]" />`. Thêm thẻ `<hr />` ngăn cách giữa các sản phẩm. | File: **`ShoppingCartComponent.razor`** | Nhúng `<LineItemComponent>` vào vòng lặp và truyền property `LineItem`. |
| **36:31 - 37:20** | Thầy tiếp tục tạo component con thứ 2 cho bảng tóm tắt đơn hàng bên cột phải: `OrderSummaryComponent.razor`. | **Project: `eShop.Web.CustomerPortal`**<br>📁 Thư mục: `Controls` | Chuột phải `Controls` ➔ **Add** ➔ **Razor Component...** ➔ Đặt tên: **`OrderSummaryComponent.razor`**. |
| **37:21 - 39:50** | Trong `OrderSummaryComponent.razor`, thầy khai báo parameter `[Parameter] public Order order { get; set; }`. Khai báo biến `int itemsCount` và `double itemsTotalPrice`. Viết override `OnParametersSet()` để duyệt qua danh sách mặt hàng và tính tổng tiền: `itemsTotalPrice += x.Price * x.Quantity`. | File: **`OrderSummaryComponent.razor`** | Khai báo Parameter, các biến lưu số lượng, tổng tiền và thuật toán tính tổng trong `OnParametersSet`. |
| **39:51 - 42:35** | Thầy hoàn thiện giao diện Card Bootstrap cho `OrderSummaryComponent`: Tiêu đề "Order Summary", dòng hiển thị số món (`Items (n)`), dòng `Shipping And Handling ($0.00)`, dòng `Estimated Tax ($0.00)`, dòng `Order Total`, và nút bấm lớn `<button class="btn btn-primary">Place your order</button>`. | File: **`OrderSummaryComponent.razor`** | Viết toàn bộ thẻ HTML Card hiển thị thông tin hóa đơn và nút Place Order. |
| **42:36 - 44:00** | Thầy quay lại `ShoppingCartComponent.razor`, đưa component `<OrderSummaryComponent order="order" />` vào cột bên phải. Sau đó mở file `Program.cs` của `eShop.Web` để đăng ký DI cho `IViewShoppingCartUseCase`. | **Project: `eShop.Web`**<br>File: **`Program.cs`** | Nhúng `<OrderSummaryComponent>` vào trang và thêm dòng DI trong `Program.cs`. |
| **44:01 - 45:30** | Thầy chạy thử (F5) và gõ URL `/cart` trên trình duyệt: Gặp lỗi **InvalidOperationException** (dịch vụ chưa được đăng ký). Thầy phát hiện trong `Program.cs` đã gõ nhầm class khác. Thầy sửa lại đúng `builder.Services.AddTransient<IViewShoppingCartUseCase, ViewShoppingCartUseCase>();`. | File: **`Program.cs`** | Sửa lỗi đăng ký DI trong `Program.cs` và import namespace cần thiết. |
| **45:31 - 46:30** | Chạy lại lần 2: Vẫn còn lỗi do biến `order` ban đầu là null trước khi JS Interop trả về kết quả. Thầy bổ sung điều kiện `@if (order != null && order.LineItems != null && order.LineItems.Count > 0)` bọc toàn bộ khối giao diện. | File: **`ShoppingCartComponent.razor`** | Thêm khối kiểm tra null safety cho `order`. |
| **46:31 - 47:22** | Chạy lại ứng dụng: Trang `/cart` tải hoàn chỉnh và hiển thị danh sách sản phẩm cùng bảng tính tiền Order Summary rất chuẩn xác. Thầy lưu ý ảnh sản phẩm sẽ được tinh chỉnh CSS ở video tiếp theo và kết thúc bài học. | Trình duyệt Web | Xác nhận giao diện giỏ hàng hoạt động thành công và dặn dò bài học kế tiếp. |

---

## 3. Toàn bộ mã nguồn chi tiết của các file

### 1. `eShop.UseCases/ShoppingCartScreen/ViewShoppingCartUseCase.cs` *(Tạo mới)*
> **Vị trí**: Project `eShop.UseCases` ➔ Thư mục `ShoppingCartScreen`
```csharp
using eShop.CoreBusiness.Models;
using eShop.UseCases.PluginInterfaces.UI;
using eShop.UseCases.ShoppingCartScreen.Interfaces;
using System.Threading.Tasks;

namespace eShop.UseCases.ShoppingCartScreen
{
    public class ViewShoppingCartUseCase : IViewShoppingCartUseCase
    {
        private readonly IShoppingCart shoppingCart;

        public ViewShoppingCartUseCase(IShoppingCart shoppingCart)
        {
            this.shoppingCart = shoppingCart;
        }

        public Task<Order> Execute()
        {
            return shoppingCart.GetOrderAsync();
        }
    }
}
```

---

### 2. `eShop.UseCases/ShoppingCartScreen/Interfaces/IViewShoppingCartUseCase.cs` *(Tạo mới)*
> **Vị trí**: Project `eShop.UseCases` ➔ Thư mục `ShoppingCartScreen/Interfaces`
```csharp
using eShop.CoreBusiness.Models;
using System.Threading.Tasks;

namespace eShop.UseCases.ShoppingCartScreen.Interfaces
{
    public interface IViewShoppingCartUseCase
    {
        Task<Order> Execute();
    }
}
```

---

### 3. `eShop.Web.CustomerPortal/Controls/LineItemComponent.razor` *(Tạo mới)*
> **Vị trí**: Project `eShop.Web.CustomerPortal` ➔ Thư mục `Controls`
```razor
@if (LineItem != null && LineItem.Product != null)
{
    <li class="row">
        <div class="col">
            <img src="@LineItem.Product.ImageLink" />
        </div>
        <div class="col">
            <h5>@LineItem.Product.Name</h5>
            <div>
                Quantity:&nbsp;&nbsp;
                <input type="number" min="0" value="@LineItem.Quantity" @onchange="HandleQuantityChange" />
            </div>
            <div>
                Price:&nbsp;&nbsp;@LineItem.Product.Price.ToString("c")
            </div>
        </div>
        <div class="col">
            <button class="btn btn-light" @onclick="@(() => DeleteProduct(LineItem.ProductId))">Delete</button>
        </div>
    </li>
}

@code {
    [Parameter]
    public OrderLineItem LineItem { get; set; }

    void HandleQuantityChange()
    {
        // Sẽ hiện thực cập nhật số lượng ở các bài sau
    }

    void DeleteProduct(int productId)
    {
        // Sẽ hiện thực xóa sản phẩm ở các bài sau
    }
}
```

---

### 4. `eShop.Web.CustomerPortal/Controls/OrderSummaryComponent.razor` *(Tạo mới)*
> **Vị trí**: Project `eShop.Web.CustomerPortal` ➔ Thư mục `Controls`
```razor
<div class="card" style="width: 25rem;">
    <div class="card-body">
        <h5 class="card-title">Order Summary</h5>
        <div class="card-text d-flex justify-content-between">
            <div>Items (@itemsCount):</div>
            <div>@itemsTotalPrice.ToString("c")</div>
        </div>
        <div class="card-text d-flex justify-content-between">
            <div>Shipping And Handling:</div>
            <div>$0.00</div>
        </div>
        <div class="card-text d-flex justify-content-between">
            <div>Estimated Tax:</div>
            <div>$0.00</div>
        </div>
        <hr />
        <div class="card-text d-flex justify-content-between" style="color: darkred">
            <div><b>Order Total:</b></div>
            <div><b>@itemsTotalPrice.ToString("c")</b></div>
        </div>
        <br />
        <button class="btn btn-primary" @onclick="PlaceOrder">Place your order</button>
    </div>
</div>

@code {
    int itemsCount = 0;
    double itemsTotalPrice = 0;

    [Parameter]
    public Order order { get; set; }

    protected override void OnParametersSet()
    {
        if (order != null && order.LineItems != null)
        {
            itemsCount = order.LineItems.Count;
            itemsTotalPrice = 0;
            order.LineItems.ForEach(x => itemsTotalPrice += x.Price * x.Quantity);
        }
    }

    private void PlaceOrder()
    {
        // Sẽ hiện thực đặt hàng ở bài học sau
    }
}
```

---

### 5. `eShop.Web.CustomerPortal/Pages/ShoppingCartComponent.razor` *(Tạo mới)*
> **Vị trí**: Project `eShop.Web.CustomerPortal` ➔ Thư mục `Pages`
```razor
@page "/cart"

@using eShop.UseCases.ShoppingCartScreen.Interfaces
@inject IViewShoppingCartUseCase viewShoppingCartUseCase

<h3>Shopping Cart</h3>
<br />

@if (order != null && order.LineItems != null && order.LineItems.Count > 0)
{
    <div class="row">
        <div class="col-6">
            <ul style="list-style: none;">
                @for (int i = 0; i < order.LineItems.Count; i++)
                {
                    <LineItemComponent LineItem="order.LineItems[i]" />
                    @if (i < order.LineItems.Count - 1)
                    {
                        <hr />
                    }
                }
            </ul>
        </div>
        <div class="col">
            <OrderSummaryComponent order="order" />
        </div>
    </div>
}

@code {
    private Order order;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            order = await viewShoppingCartUseCase.Execute();
            StateHasChanged();
        }
    }
}
```

---

### 6. `eShop.Web/Program.cs` *(Bổ sung Dependency Injection)*
> **Vị trí**: Project `eShop.Web` ➔ File `Program.cs`
```csharp
// Thêm namespace này ở đầu file nếu chưa có
using eShop.UseCases.ShoppingCartScreen;
using eShop.UseCases.ShoppingCartScreen.Interfaces;

// Đăng ký dịch vụ cho ViewShoppingCartUseCase
builder.Services.AddTransient<IViewShoppingCartUseCase, ViewShoppingCartUseCase>();
```

---

## 4. Các điểm kỹ thuật quan trọng cần ghi nhớ

> [!IMPORTANT]  
> 1. **Vòng đời `OnAfterRenderAsync` & JS Interop (LocalStorage)**:  
>    LocalStorage chỉ tồn tại ở phía trình duyệt (Client-side). Trong mô hình Blazor Server, `OnInitializedAsync` chạy trên Server trước khi HTML đầu tiên được trả về, lúc đó trình duyệt chưa sẵn sàng kết nối JS Interop. Do đó, việc đọc LocalStorage **bắt buộc** phải đặt trong `OnAfterRenderAsync(bool firstRender)` khi `firstRender == true`, sau đó gọi `StateHasChanged()` để báo cho Blazor render lại giao diện.
>
> 2. **Kiểm tra Null Safety cho UI Model**:  
>    Vì `order` được nạp bất đồng bộ sau khi component render lần đầu, trong tích tắc đầu tiên `order` sẽ có giá trị `null`. Nếu không có điều kiện `@if (order != null ...)`, Blazor sẽ quăng ngoại lệ `NullReferenceException` khi cố truy cập `order.LineItems`.
>
> 3. **Lambda Expression trong sự kiện Blazor**:  
>    Khi một sự kiện như `@onclick` cần truyền tham số (như ID sản phẩm `LineItem.ProductId`), ta phải dùng cú pháp Lambda: `@onclick="@(() => DeleteProduct(LineItem.ProductId))"`.
>
> 4. **`OnParametersSet`**:  
>    Được gọi mỗi khi component nhận hoặc thay đổi giá trị từ `[Parameter]`. Rất thích hợp để thực hiện các phép tính toán phụ thuộc vào dữ liệu đầu vào (như tính tổng số lượng `itemsCount` và tổng số tiền `itemsTotalPrice`).
