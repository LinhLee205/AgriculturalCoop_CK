# Rà soát và hoàn thiện quản trị AgriculturalCoop

Phạm vi: tiếp tục các thay đổi dang dở theo yêu cầu người dùng, giữ luồng đặt trước, phân bổ, banner, tài khoản và thông báo web đã có.

## Các lỗi và phần thiếu đã sửa

- Xóa khai báo interface và đăng ký DI bị lặp; bổ sung cài đặt InMemory, cập nhật constructor và nơi gọi trong bộ kiểm tra.
- Hoàn thiện đăng ký DI cho quản lý đơn và đại lý. Bổ sung các màn hình, liên kết và kiểm tra quyền Admin.
- Bỏ khung tiêu đề banner; tách `/products` khỏi trang chủ, thêm tìm kiếm/lọc/sắp xếp. Chỉ sản phẩm có lô đang mở, chưa lưu trữ mới xuất hiện.
- Bổ sung chi tiết đơn: người nhận, điện thoại, địa chỉ, các dòng hàng, lượng phân bổ và liên kết tới lô. Danh sách lọc theo đại lý, lô và trạng thái.
- Hủy đơn chưa phân bổ có lý do; trả hạn mức theo trạng thái đơn, không xóa lịch sử. SQL khóa các lô liên quan trong transaction để kiểm tra lại trước khi hủy.
- Quản lý hồ sơ và trạng thái đại lý; chặn đăng nhập, cookie ở yêu cầu HTTP tiếp theo và đặt đơn mới khi đã ngừng giao dịch. Thông tin nhận hàng của đơn cũ không bị đổi theo hồ sơ đại lý.
- Lưu trữ/khôi phục lô; giữ liên kết với đơn, phân bổ và thông báo. Lô đang mở bị đóng khi lưu trữ; khôi phục cần mở đặt lại nếu muốn tiếp tục nhận đơn.
- Hoàn tất lô đã phân bổ; chặn sửa sản lượng/chạy lại phân bổ khi hoàn tất hoặc đang lưu trữ.
- Cập nhật trạng thái đơn khi phân bổ đủ/một phần, xét toàn bộ các lô của đơn. Khi đọc đơn cũ, trạng thái hiển thị được đối chiếu với phân bổ thực tế mà không ghi đè dữ liệu tài chính.
- Số lượng trong lịch sử đơn hiển thị đến 0,01 kg; đơn hủy hiển thị lý do bằng tiếng Việt.

## Nguyên tắc giữ dữ liệu

`Database/006_AdminLifecycle.sql` chỉ thêm cột còn thiếu, không chạy lại seed hoặc xóa bảng. Các thao tác xóa chỉ dành cho hồ sơ trống; lô đã có đơn và đại lý đã có tài khoản/đơn dùng trạng thái lưu trữ hoặc ngừng giao dịch.

Không tự sửa trạng thái giao dịch thật trong quá trình kiểm thử. Các bản ghi SQL tạm và thay đổi dùng để kiểm tra được đặt trong transaction rồi rollback. Identity có thể có khoảng trống sau thử nghiệm hoàn tác; không ảnh hưởng khóa ngoại hoặc các đơn đang dùng.

## Đối chiếu đề tài 16

Hai luật vẫn nằm ở UseCases/CoreBusiness: tổng đặt không vượt sản lượng dự kiến; phân bổ hụt mùa theo tỷ lệ và phần dư lớn nhất đến 0,01 kg. Tổng phân bổ bằng giá trị nhỏ hơn giữa sản lượng thực và lượng đã đặt; không phân vượt nhu cầu khi thu hoạch dư.

Trang quản trị giữ Authorize Admin; truy vấn tham số hóa và transaction vẫn được dùng. Thông báo theo mẫu được lưu cùng phân bổ; nội dung AI chỉ đăng sau phê duyệt. Không bổ sung yêu cầu SMTP vào đề bài.

## Kiểm tra và giới hạn

- Build toàn solution sang `App_Data/preview-build` để tránh ghi đè DLL của phiên Visual Studio đang chạy.
- `AdminLifecycleChecks`: hủy/giải phóng hạn mức, giữ quan hệ đơn hủy, lưu trữ/khôi phục, hoàn tất, đơn nhiều lô và đại lý ngừng giao dịch.
- `AdminLifecycleSqlChecks`: chạy các thao tác SQL trên bản ghi tạm trong transaction, kiểm tra trạng thái/hạn mức/giữ lịch sử rồi rollback.
- Bộ kiểm tra cũ vẫn kiểm tra thuật toán, xác thực, phân quyền HTTP, giỏ/xác nhận, ảnh, banner và thông báo; thêm kiểm tra các route quản trị mới và danh mục sản phẩm riêng.
- Chưa kiểm thử trực quan thao tác chuột trên trình duyệt hoặc tải đồng thời nhiều người dùng. HTTP và render HTML được kiểm tra tự động.
- Không gọi AI thật hoặc gửi email trong lượt kiểm tra này. Lần chẩn đoán AI trước nhận Google 403 dự án bị từ chối truy cập; cần kiểm tra lại quyền ở Google để demo AI thật. Chức năng mẫu/thông báo web không phụ thuộc kết nối đó.
