# Bài 08 Lỗi giữa chừng và trạng thái phục hồi

Mục tiêu: giải thích và kiểm tra failure path thay vì chỉ truyền thành công. TCP RTO thuộc OS; request timeout/peer TTL thuộc ứng dụng và có thể phải chọn lớn hơn thời gian xử lý bình thường của workload.

## Khi peer rời

Connection lỗi/EOF hoặc request hết hạn → đánh dấu request cuối một lần → thu hồi credit → phần chưa Written có thể được giao lại với generation mới. Kết quả từ generation cũ tới muộn phải được xử lý mà không tăng completion hai lần hoặc ghi cạnh tranh với generation mới. Dữ liệu đã nhận và đang được writer dùng vẫn có owner tới khi writer kết thúc.

Timeout không chứng minh peer chết vĩnh viễn. Có retry budget/backoff và log nguyên nhân. Khi không còn nguồn giữ một piece, báo unavailable/không hoàn tất; không dựng dữ liệu hoặc lặp vô hạn. Có thể giữ seed online trong lab để khảo sát failover có nguồn dự phòng.

## Resume

Tệp `.part` và metadata có thể không khớp sau crash. Metadata ghi trước khi dữ liệu bền có thể nói sai; bản cơ sở khởi động lại kiểm tra manifest, kích thước và rehash piece dự kiến đã có trước khi phát HAVE. Chỉ đọc bitmap rồi tiếp tục ngay chưa đủ chứng minh dữ liệu đúng.

## Bài nhỏ

1. Viết bảng sự kiện: request r1 → timeout → r2 thay thế → response r1 muộn → r2 xong. Với mỗi event ghi state, generation, credit và owner.
2. Cố ý làm peer ngắt khi đang gửi block; receiver không được đánh dấu piece hoàn tất từ dữ liệu thiếu.
3. Cố ý sửa byte trong `.part` sau một lần dừng, rồi resume; piece bị sửa phải tải lại.
4. Thử chỉ còn một nguồn giữ piece cuối và nguồn đó rời: expected kết quả là không hoàn tất có lý do. Đây là test đúng, không phải bắt buộc mọi lỗi đều được “phục hồi thành công”.

Hoàn thành khi T07/T11/T12 có bằng chứng, không double-free/lease leak và report phân biệt lỗi dự kiến với lỗi chương trình. Tiếp [Bài 09](09-kiem-thu-va-capture.md).
