# Bài 09 Chứng minh hoạt động bằng test và gói tin

Đọc Intro PDF 124, Socket 38–42, TCP 19–27. Dùng [ma trận test](../kiem-chung/Ke-hoach-kiem-thu.md) làm checklist; không coi mọi mục là đã pass vì file tồn tại.

## Các lớp bằng chứng

Unit test codec/manifest/state có input nhỏ xác định. Integration test kiểm tra các module kết nối thật. System test hai máy thêm IP/firewall/NIC và tài nguyên OS. Benchmark chỉ chạy sau correctness và không thay test sai dữ liệu/lifetime.

Mock stream cố ý trả ít byte kiểm tra vòng đọc đủ; không gọi nó là giả lập loss mạng. TCP có thể truyền lại khiến ứng dụng chỉ thấy chậm; lỗi hash cần dữ liệu ứng dụng/tệp được cố ý làm sai trong một ca được gắn nhãn.

## Capture có mục tiêu

Chọn loopback khi process cùng máy, NIC LAN khi hai máy. Capture filter lab có thể dùng `tcp portrange 42300-42308`. Display filter tập trung `tcp.stream` hoặc port cụ thể; lưu stream ID cùng connection ID của log. Đọc SYN/SYN-ACK/ACK, payload, ACK và FIN/RST; số gói không bằng số frame ứng dụng.

Checksum cảnh báo/offload hoặc packet lớn ở capture host chưa đủ để kết luận app sai. Một cặp packet ở hai điểm capture không được cộng đôi như hai lần truyền khác nhau.

## Bài nhỏ

1. Capture một lượt echo và một lượt tệp nhỏ, ghi hai endpoint và stream tương ứng.
2. Từ log chọn một REQUEST_BLOCK rồi tìm byte header E423 ở stream reassembled; giải thích vì sao một packet riêng có thể không chứa đủ header/payload.
3. Ngắt một peer, đối chiếu log request timeout/reassign và TCP đóng/lỗi quan sát được.
4. Chọn 3 ca sai để giữ input nhỏ và regression: truncated header, last piece 3 byte, response cũ tới muộn.

Hoàn thành khi có command/revision/SDK/config/log/hash/pcap có thể kiểm tra lại; không lấy ảnh demo online làm evidence của nhóm. Tiếp [Bài 10](10-thuc-nghiem-va-phan-tich.md).
