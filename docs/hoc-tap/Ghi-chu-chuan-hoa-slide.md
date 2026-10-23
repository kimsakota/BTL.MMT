# Ghi chú khi chuyển slide thành thiết kế và thí nghiệm

Mục đích là giữ đúng khái niệm, đơn vị và phạm vi mô hình khi áp dụng vào C#. Các slide vẫn là tài liệu học chính. Trang bên dưới đã đối chiếu với bản trích; các trang 04-2/4, 04-2/23, 04-1/22, Intro/106, Socket/40 và IP-address/37 đã mở ảnh render.

| Nguồn | Điều cần hiểu đúng | Áp dụng |
|---|---|---|
| Intro PDF 106, footer 115 | Số footer và số trang file khác nhau | Link tài liệu dùng trang PDF, ghi rõ cách đếm |
| TCP PDF 8 | Bullet về routing/unreliable đang mô tả nhu cầu bổ sung trên Internetwork; bảng đặt TCP/UDP ở Host-to-Host | Không kết luận TCP làm IP routing |
| TCP PDF 22 | Tiêu đề “2-way handshake” nhưng hình vẽ có FIN, ACK, FIN, ACK | Giải thích đóng độc lập hai chiều; ACK có thể gộp với FIN, số segment quan sát phụ thuộc tình huống; không khẳng định luôn hai gói |
| Congestion PDF 4 | RTT có đơn vị thời gian, window thường là byte/segment | Quy đổi qua BDP: W byte cần khoảng >= B(bit/s) × RTT(s) / 8 để không bị giới hạn bởi window trong mô hình đơn giản; không so sánh trực tiếp ms và byte |
| Congestion PDF 23 | Hình 1→2→4→8 theo các vòng RTT; dòng chữ nhân đôi trên mỗi ACK là giản lược dễ hiểu nhầm | Slow start tăng cwnd theo ACK mới, xấp xỉ nhân đôi mỗi RTT khi đủ điều kiện; không nhân đôi toàn cwnd trên từng ACK |
| Congestion PDF 8 và 13 | Hai cách ký hiệu trọng số cũ/mới dễ bị trộn; trang 13 cần đối chiếu convention | Nếu chọn alpha=1/8 cho mẫu mới thì SRTT_new=(1-alpha)×SRTT_old+alpha×sample; nêu rõ convention |
| Congestion PDF 23–28 | Ví dụ lịch sử và biến thể TCP | Không gán mọi quy tắc Reno/Tahoe cho stack Windows hiện tại; ghi môi trường/biến thể thực đo |
| Congestion PDF 31 | MSS 1460 giả định MTU 1500, IPv4 header 20 và TCP header 20, không có option | Không cố định MSS hoặc đồng nhất block size với MSS |
| Socket PDF 27 | `shutdown` và đóng handle là các thao tác khác nhau | Trong C# dùng Shutdown/Dispose theo lifetime; không cho rằng đóng handle local tự đóng ngay handle ở peer |
| Socket PDF 38–40 | Mô hình concurrent server minh họa process con | C# có thể dùng async task và channel có giới hạn; concurrency không bắt buộc một OS thread/process cho mỗi client |

Đối chiếu ngắn với nguồn chuẩn: [RFC 9293](https://www.rfc-editor.org/rfc/rfc9293.html) về TCP và đóng kết nối; [RFC 5681 mục 3.1](https://www.rfc-editor.org/rfc/rfc5681.html#section-3.1) về slow start; [RFC 6298 mục 2](https://www.rfc-editor.org/rfc/rfc6298.html#section-2) về SRTT/RTO. Ứng dụng P2P không tự thay các thuật toán TCP của OS.

Trong pipeline của chương trình, timeout một request là chính sách ứng dụng để phát hiện công việc không tiến triển; TCP RTO do stack điều khiển. Buffer pool của ứng dụng cũng không phải rwnd/cwnd. Khi bảo vệ phải nói rõ tầng nào đang được quan sát hoặc điều chỉnh.
