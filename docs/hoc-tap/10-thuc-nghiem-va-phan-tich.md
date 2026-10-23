# Bài 10 Từ kết quả chạy đến kết luận

Đọc Intro PDF 63–74, Congestion 3–13, 22–37 và phần metrics MANET 23–25 như ví dụ cách chọn chỉ tiêu. Áp dụng [thiết kế thực nghiệm](../kiem-chung/Thiet-ke-thuc-nghiem.md), không nhập giả định MANET vào LAN.

## Một so sánh có thể giải thích

Giả thuyết là câu có thể sai, ví dụ “rarest-first giảm thời gian hoàn tất khi peer có upload không đều”. Biến độc lập là policy; metrics là completion time; các yếu tố giữ cố định gồm tệp, máy, limit, số peer, block/piece, lịch rời và buffer. Chỉ so hai cấu hình thay nhiều thứ không xác định riêng tác động policy.

P2P dùng thêm upload của receiver so với CS. Báo rõ tài nguyên này; có thể đo thêm nhóm cùng tổng upload budget nếu muốn tách hiệu quả tổ chức. App payload rate limit không phải link bandwidth. Buffer giảm allocation có thể không tăng goodput khi bottleneck là link/đĩa.

## Metric và mẫu

Mbit/s=8×byte/(giây×10^6). Time window và denominator phải thống nhất. Không dùng thời gian trên hai đồng hồ wall-clock chưa đồng bộ để trừ trực tiếp. Rời theo lịch, timeout và fail là các trạng thái phải giữ trong dữ liệu; không chỉ báo trung bình các run đẹp.

Lặp 5 lần/cấu hình theo kế hoạch, lưu seed ngẫu nhiên và đảo thứ tự cặp thử. Trung bình/độ lệch chuẩn/min–max có ý nghĩa khi cùng định nghĩa metric; ít mẫu không đủ để tuyên bố khác biệt rất nhỏ chắc chắn.

## Bài nhỏ

1. Viết một bảng biến cho Q1, Q2, Q3 của kế hoạch, chưa chạy gì.
2. Làm calibration để chọn tệp và deadline; ghi đây là pilot, tách khỏi bộ đo cuối nếu điều kiện khác.
3. Viết runner/plot đọc raw data thật; kiểm tra schema và trạng thái thiếu/fail thay vì tự điền số 0.
4. Vẽ một biểu đồ với đơn vị/chú giải/error bar, rồi viết một nhận xét quan sát, một suy luận và một giới hạn riêng.

Hoàn thành khi một người khác dựng lại plot từ raw data và giải thích được cấu hình. Không tạo bộ CSV hiệu năng giả để làm report có vẻ đầy đủ. Tiếp [Bài 11](11-bao-cao-demo-va-bao-ve.md).
