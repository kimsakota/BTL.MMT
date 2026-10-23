# Bài 05 Tracker và thông tin có thể hết hạn

Mục tiêu: peer tìm nhau qua một tracker tối giản. Tracker quản lý endpoint, không chuyển dữ liệu tệp. Áp dụng client–server, timeout, registry và phân biệt IP bind/advertise của Bài 01.

## Hợp đồng

Registry key `(fileId,peerId)` chứa endpoint và last-seen monotonic. REGISTER tạo/cập nhật; HEARTBEAT gia hạn; LIST_PEERS trả các mục chưa hết hạn. Các type dùng header chung và JSON theo [giao thức](../thiet-ke/Giao-thuc-v1.md). TTL mặc định đề xuất 20 giây, heartbeat 5 giây; đây là tham số lab, không phải TCP timer.

Thông tin trả về có thể đã cũ ngay sau khi đọc. Peer phải connect với timeout và loại nguồn lỗi ở local; không cho rằng tracker kiểm chứng một peer đang giữ mọi piece. Sau kết nối, availability lấy từ bitmap/HAVE.

## Bài nhỏ

1. Dùng đồng hồ giả để mô tả registry tại t=0, 5, 19, 21. Peer heartbeat ở t=5 có còn hiệu lực ở t=21 không? Tự ghi điều kiện `now-lastSeen >= TTL` hay `>` theo hợp đồng nhóm chọn.
2. REGISTER cùng key hai lần với endpoint mới: phải có một mục mới nhất, không tăng số lượng thành hai.
3. Thiết kế test nhiều đăng ký/tra cứu/expiry đồng thời; xác định owner và khóa/actor quản lý map.
4. Viết discovery client, lọc chính mình và xử lý danh sách rỗng. Chỉ công bố IP truy cập được, không dùng wildcard như địa chỉ đích.
5. Chạy hai máy, ghi endpoint thật và connect/log/capture.

Registry phải có giới hạn số mục/độ dài dữ liệu trong lab; không để input quyết định một mảng vô hạn. Hoàn thành khi T06 và phần discovery của T14 đúng, có thể giải thích sự khác nhau giữa cache stale, TCP connection lỗi và piece unavailable. Tiếp [Bài 06](06-P2P-scheduler-va-pipeline.md).
