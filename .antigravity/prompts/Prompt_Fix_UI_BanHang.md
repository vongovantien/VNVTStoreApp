# Hướng dẫn Tối ưu Giao diện Bán hàng (POS / Checkout / List)

File này đóng vai trò là prompt mẫu để định hình phong cách thiết kế và chuyển đổi code giao diện bán hàng sang React + Tailwind CSS.

## 1. Nguyên tắc Thiết kế Yêu cầu

*   **Đơn giản & Trực quan (Minimalist & Clean):** Sử dụng khoảng trắng hợp lý, gom nhóm các chức năng liên quan để giảm tải thông tin hiển thị đồng thời.
*   **Ergonomic Colors (Hệ màu dịu mắt):**
    *   Tránh sử dụng màu tương phản quá gắt (ví dụ: không dùng đen/trắng tuyệt đối `#000000`/`#FFFFFF`).
    *   Sử dụng các gam màu Slate/Zinc kết hợp với sắc xanh công nghệ (Tech Blue) làm điểm nhấn.
*   **Trải nghiệm Tốc độ Cao (Speed UX):**
    *   Tối ưu luồng thao tác của nhân viên thu ngân: hạn chế tối đa việc sử dụng chuột.
    *   Hỗ trợ phím tắt (`F2` để tìm kiếm, `F8` chọn thẻ, `F9` chọn tiền mặt, `Enter` để hoàn tất thanh toán).
    *   Hỗ trợ quét mã vạch trực tiếp mà không cần mở modal tìm kiếm.

## 2. Cấu trúc Layout Đề xuất (Grid/Flexbox)

*   **POS Page:** 2 cột (hoặc 3 cột trên màn hình rộng). Cột trái (60% - Danh sách/Grid sản phẩm), Cột phải (40% - Giỏ hàng hiện tại & Thanh toán).
*   **Inventory/Order List:** Bố cục chia đôi màn hình (Split Pane). Danh sách bảng dữ liệu bên trái, khung xem chi tiết/tác vụ nhanh (Detail Sidebar) ở bên phải.

## 3. Định dạng Output Mong muốn

Khi chuyển đổi hoặc tối ưu hóa màn hình được yêu cầu:
1.  **Phân tích UX ngắn:** Chỉ ra các điểm rối rắm hiện tại và đề xuất gom nhóm/thay đổi input.
2.  **Code React + Tailwind CSS:** Viết code hoàn chỉnh, sạch sẽ, chia component rõ ràng và tích hợp các thuộc tính responsive (`sm:`, `md:`, `lg:`).
3.  **Bảng phím tắt:** Định nghĩa các sự kiện bàn phím cần lắng nghe.
