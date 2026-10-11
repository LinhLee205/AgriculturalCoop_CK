# Ghi Chép Chi Tiết: Video 05 - Improve UI Layout
**Khóa học**: Part 3 - Build Customer Portal  
**Thời lượng video**: 21 phút 25 giây  
**Công nghệ sử dụng**: C#, ASP.NET Core Blazor, Bootstrap 4  

---

> [!NOTE]  
> Tài liệu này ghi lại toàn bộ lời giảng, mốc thời gian, vị trí thư mục/file thao tác trên Solution Explorer và code chi tiết tương ứng theo từng bước của người hướng dẫn.

---

## 1. Mục tiêu bài học
1. **Loại bỏ Sidebar mặc định**: Thanh điều hướng dọc bên trái không phù hợp với website bán hàng (e-commerce).
2. **Tạo Top Navigation Bar**: Sử dụng Bootstrap Navbar màu tối (`navbar-dark bg-dark`) nằm ngang trên đầu trang.
3. **Cấu hình Route trang chủ**: Chuyển hướng route mặc định (`/`) về trang tìm kiếm sản phẩm `SearchProductComponent`.
4. **Dọn dẹp code mẫu**: Xóa các file demo mặc định của template Blazor (`Counter.razor`, `FetchData.razor`, `Index.razor`).
5. **Hiển thị sản phẩm dạng Card**: Thay thế thẻ `<table>` thô bằng lưới các thẻ sản phẩm `ProductCardComponent` (ảnh, tên, link chi tiết, giá tiền).

---

## 2. Bảng tiến trình chi tiết theo từng phút

| Mốc thời gian | Lời giảng & Giải thích của Thầy | Vị trí thao tác trong Visual Studio | Thao tác & Nội dung Code |
| :--- | :--- | :--- | :--- |
| **00:00 - 00:59** | Thầy mở đầu: Tinh chỉnh lại UI cho thân thiện và tăng trải nghiệm người dùng. Menu dọc bên trái hiện tại giống trang admin/quản lý hơn là web bán hàng. Cần đổi sang thanh ngang ở trên đầu (Top Navigation Bar). | Trình duyệt Web | Quan sát UI hiện tại và tìm kiếm mẫu Top Navigation Bar trên Google. |
| **01:00 - 02:00** | Thầy hướng dẫn tạo component mới cho thanh điều hướng trên đầu trang trong thư mục `Shared`. | **Project: `eShop.Web`**<br>📁 Thư mục: `Shared` | Chuột phải `Shared` ➔ **Add** ➔ **Razor Component...** ➔ Đặt tên: **`TopNavBar.razor`**. |
| **02:01 - 04:30** | Thầy mở tài liệu Bootstrap (mục Navbar), chọn một mẫu navbar đơn giản có Brand và Nav Links, copy về file `TopNavBar.razor` rồi lược bỏ các phần thừa (form search, dropdown, toggler). | Trình duyệt (Bootstrap docs) & File **`TopNavBar.razor`** | Dán code HTML Bootstrap Navbar và xóa bớt các thẻ không dùng. |
| **04:31 - 06:08** | Thầy chỉnh sửa theme sang màu tối (`navbar-dark bg-dark`), thêm link thương hiệu tên `eShop` với đường dẫn `#`. | File: **`TopNavBar.razor`** | Hoàn thiện code cho thẻ `navbar` và link thương hiệu `eShop`. |
| **06:09 - 06:27** | Thầy nhúng component `TopNavBar` vừa tạo vào layout chung của trang web. | **Project: `eShop.Web`**<br>📁 `Shared` ➔ File: **`MainLayout.razor`** | Thay thế `<NavMenu />` thành `<TopNavBar />`. |
| **06:28 - 07:15** | Thầy giải thích về route: Trang sản phẩm đang là `/products`. Thầy muốn vào trang chủ (`/`) là thấy ngay sản phẩm. Thầy lưu ý một Component có thể khai báo nhiều directive `@page`. | **Project: `eShop.Web.CustomerPortal`**<br>📁 `Pages` ➔ File: **`SearchProductComponent.razor`** | Thêm `@page "/"` lên dòng đầu tiên. |
| **07:16 - 08:05** | Thầy dọn dẹp các trang mẫu không dùng của template Blazor. | **Project: `eShop.Web`**<br>📁 `Pages` | Tạm dừng Debug (Stop) ➔ Chọn và **Delete** 3 file:<br>- `Counter.razor`<br>- `FetchData.razor`<br>- `Index.razor`. |
| **08:06 - 08:20** | Thầy nhấn F5 chạy thử để xem kết quả. Thấy Top Bar đã hiện nhưng thanh màu tím bên trái (class `.sidebar`) vẫn còn chiếm chỗ. | Trình duyệt Web | Nhận diện lỗi layout: Cần bỏ khối CSS `.sidebar`. |
| **08:21 - 09:25** | Thầy quay lại Visual Studio xử lý triệt để: Xóa toàn bộ khối `<div class="sidebar">`, xóa `<div class="page">`, đưa `<TopNavBar />` vào đầu thẻ `<main>`. Chạy lại thì sidebar biến mất hoàn toàn. | **Project: `eShop.Web`**<br>📁 `Shared` ➔ File: **`MainLayout.razor`** | Cập nhật cấu trúc layout chuẩn chỉ có thẻ `<main>`. |
| **09:26 - 10:34** | Thầy nhận xét: Danh sách sản phẩm đang dùng thẻ `<table>` nhìn rất thô. Trang bán hàng cần hiển thị mỗi sản phẩm là một ô Card có hình ảnh, giá và nút bấm. | **Project: `eShop.Web.CustomerPortal`**<br>📁 `Pages/SearchProductComponent.razor` | Chuẩn bị thay thế thẻ `<table>`. |
| **10:35 - 11:30** | Thầy tạo một component riêng đại diện cho từng thẻ Card sản phẩm để dễ tái sử dụng. | **Project: `eShop.Web.CustomerPortal`**<br>📁 Thư mục: `Controls` | Chuột phải `Controls` ➔ **Add** ➔ **Razor Component...** ➔ Đặt tên: **`ProductCardComponent.razor`**. |
| **11:31 - 14:00** | Thầy mở file `ViewProductComponent.razor` cũ để xem lại cấu trúc CSS Card, sau đó gõ từng dòng từ đầu trong `ProductCardComponent.razor` để học viên dễ theo dõi. | File: **`ProductCardComponent.razor`** | Viết thẻ `<div class="card m-2 p-2" style="width: 12rem;">` và ảnh `<img src="@product.ImageLink" class="card-img-top" alt="" />`. |
| **14:01 - 16:50** | Thầy khai báo parameter nhận đối tượng `Product`, tạo `card-body`, đưa tên sản phẩm vào thẻ `<NavLink>` dẫn tới trang `/product/{id}`, và hiển thị giá tiền màu đỏ đậm. | File: **`ProductCardComponent.razor`** | Viết block `@code` với `[Parameter] public Product product { get; set; }` và các phần tử trong card. |
| **16:51 - 17:27** | Thầy kiểm tra lại lỗi cú pháp, xóa thẻ `<div class="card-body">` bị thừa do dán code, căn chỉnh lề ngay ngắn. | File: **`ProductCardComponent.razor`** | Hoàn thiện code file `ProductCardComponent.razor`. |
| **17:28 - 18:35** | Thầy quay lại trang tìm kiếm, xóa bỏ hoàn toàn khối `<table>...</table>`. Thêm khối kiểm tra điều kiện `@if (products != null && products.Count() > 0)`. | **Project: `eShop.Web.CustomerPortal`**<br>📁 `Pages` ➔ File: **`SearchProductComponent.razor`** | Xóa `<table>`, viết cấu trúc điều kiện `@if ... @else`. |
| **18:36 - 20:15** | Thầy thêm thẻ `<div class="row">`, bên trong dùng vòng lặp `@foreach (var product in products)` gọi component `<ProductCardComponent product="product" />`. | File: **`SearchProductComponent.razor`** | Hoàn thiện việc render danh sách Card trong vòng lặp. |
| **20:16 - 21:25** | Thầy nhấn F5 chạy chương trình. Trang web hiển thị danh sách sản phẩm thành lưới các ô Card có ảnh mỹ phẩm Maybelline rất đẹp mắt. Thầy gợi ý học viên có thể tùy biến các kiểu Card Bootstrap khác và kết thúc video. | Trình duyệt Web & Kết thúc bài học | Kiểm tra kết quả hiển thị dạng Grid Card và dặn dò bài tập. |

---

## 3. Toàn bộ mã nguồn chi tiết từng file

### File 1: `eShop.Web/Shared/TopNavBar.razor` *(Tạo mới)*
> **Vị trí**: Project `eShop.Web` ➔ Thư mục `Shared` ➔ Tạo file `TopNavBar.razor`.
```razor
<div class="navbar navbar-expand-lg navbar-dark bg-dark">
    <ul class="navbar-nav">
        <li class="nav-item">
            <a class="navbar-brand" href="#">eShop</a>
        </li>
    </ul>
</div>
```

---

### File 2: `eShop.Web/Shared/MainLayout.razor` *(Chỉnh sửa)*
> **Vị trí**: Project `eShop.Web` ➔ Thư mục `Shared` ➔ Mở file `MainLayout.razor`.  
> **Nội dung thay đổi**: Xóa toàn bộ khối `<div class="sidebar">` và `<NavMenu />`, cấu trúc lại layout trong thẻ `<main>`.
```razor
@inherits LayoutComponentBase

<main>
    <TopNavBar />

    <div class="top-row px-4">
        <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
    </div>

    <article class="content px-4">
        @Body
    </article>
</main>
```

---

### File 3, 4, 5: Các file cần XÓA BỎ
> **Vị trí**: Project `eShop.Web` ➔ Thư mục `Pages`.  
- ❌ `Counter.razor`
- ❌ `FetchData.razor`
- ❌ `Index.razor`

---

### File 6: `eShop.Web.CustomerPortal/Controls/ProductCardComponent.razor` *(Tạo mới)*
> **Vị trí**: Project `eShop.Web.CustomerPortal` ➔ Thư mục `Controls` ➔ Tạo file `ProductCardComponent.razor`.
```razor
<div class="card m-2 p-2" style="width: 12rem;">
    <img src="@product.ImageLink" class="card-img-top" alt="" />
    <div class="card-body">
        <h5 class="card-title">
            <NavLink href="@($"product/{product.ProductId}")">@product.Name</NavLink>
        </h5>
        <p class="card-text" style="color: darkred">
            Price: $@product.Price.ToString()
        </p>
    </div>
</div>

@code {
    [Parameter]
    public Product product { get; set; }
}
```

---

### File 7: `eShop.Web.CustomerPortal/Pages/SearchProductComponent.razor` *(Chỉnh sửa)*
> **Vị trí**: Project `eShop.Web.CustomerPortal` ➔ Thư mục `Pages` ➔ Mở file `SearchProductComponent.razor`.  
> **Nội dung thay đổi**: Thêm `@page "/"`, thay thế bảng `<table>` bằng cấu trúc lưới Bootstrap Card `<div class="row">`.
```razor
@page "/"
@page "/products"

@inject ISearchProductUseCase searchProduct

<h3>Search Product Component</h3>

<SearchBarComponent OnSearch="HandleSearch"></SearchBarComponent>

<br />

@if (products != null && products.Count() > 0)
{
    <div class="row">
        @foreach (var product in products)
        {
            <ProductCardComponent product="product" />
        }
    </div>
}
else
{
    <text>No products found.</text>
}

@code {
    private IEnumerable<Product> products;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        products = searchProduct.Execute();
    }

    private void HandleSearch(string filter)
    {
        products = searchProduct.Execute(filter);
    }
}
```

---

## 4. Các điểm kỹ thuật quan trọng cần ghi nhớ

> [!TIP]  
> - **Đa Route cho một Razor Component**: Bạn có thể đặt nhiều dòng `@page` ở đầu component (ví dụ `@page "/"` và `@page "/products"`). Khi người dùng truy cập trang chủ hay `/products` đều sẽ hiển thị component này.  
> - **Khai báo `[Parameter]`**: Khi component con cần nhận dữ liệu từ component cha, property trong `@code` bắt buộc phải có annotation `[Parameter]`.  
> - **Bố cục lưới (Grid Layout)**: Bọc thẻ cha bằng `class="row"`, các thẻ con bên trong dùng `class="card m-2 p-2"` kèm `width: 12rem` giúp các sản phẩm tự động xếp thành hàng ngang và tự ngắt xuống dòng khi hết khoảng trống.
