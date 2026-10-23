# Hồ sơ nguồn nghiên cứu

Ngày đối chiếu 23/10/2026. Tài liệu nhóm tự soạn nằm ở docs; file slide gốc giữ ở thư mục môn học, không chép 697 trang vào repo.

| Nguồn | Nơi lưu | Cách dùng |
|---|---|---|
| Slide ET4230 | `D:/2. FullStack/2026.1/Mạng máy tính/Slide` | [Chỉ mục hash/page count](slides-index.json), [bản đồ kiến thức](../hoc-tap/Kien-thuc-tu-slide.md) |
| Yêu cầu BTL 10/2026 | `C:/Users/KimSakota/Downloads/ET4230_Bai_tap_lon_Mo_phong_CAP_NHAT_2026.docx` | Yêu cầu môn học và 15 đề, đã đọc nội dung; xem [phạm vi](../Tong-quan.md) |
| VstHelper working tree | Thư mục VstHelper trong dự án Hệ điều hành | [Snapshot](../../references/VstHelper/SNAPSHOT.json), [audit](../thiet-ke/Tai-su-dung-VstHelper.md) |

[reference-files.json](reference-files.json) giữ SHA-256 của file yêu cầu và các tài liệu dự án Hệ điều hành đã tham khảo lúc khởi tạo. Đây là lịch sử nguồn; cấu trúc và quy trình hiện hành được chọn riêng cho ET4230. Các đường dẫn nguồn máy Linh không phải dependency build của thành viên/CI.

[Cơ sở chọn Đề 14](Lua-chon-de-tai.md) ghi kết luận khảo sát ban đầu và so sánh với Đề 13. Phạm vi được dùng để triển khai xem [Tổng quan](../Tong-quan.md).

## Nguồn chuẩn bổ trợ

- [RFC 9293](https://www.rfc-editor.org/rfc/rfc9293.html): cơ chế TCP/byte stream và đóng kết nối.
- [RFC 5681](https://www.rfc-editor.org/rfc/rfc5681.html): slow start và congestion control truyền thống.
- [RFC 6298](https://www.rfc-editor.org/rfc/rfc6298.html): RTT/RTO và convention hệ số.
- [BEP 3](https://www.bittorrent.org/beps/bep_0003.html): ý tưởng phân phối P2P, tracker và piece; giao thức nhóm là thiết kế riêng.
- [Microsoft Memory/Span guidelines](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines): ownership/consumer/lease.
- [Socket.SendAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendasync), [ReceiveAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync): API .NET.
- [Wireshark TCP graphs](https://www.wireshark.org/docs/wsug_html_chunked/ChStatTCPStreamGraphs.html): đọc throughput/RTT/window trong capture.

## Tái tạo chỉ mục slide

`python tools/extract_slides.py` từ root BTL nếu đã có pypdf/Poppler. Script đọc bytes PDF qua stdin cho pdftotext để tránh đường dẫn Unicode trên một số bản Poppler Windows, kiểm tra số trang bằng pypdf, ghi text/catalog vào `tmp/slides/` và chỉ mục hash vào `docs/tham-khao/slides-index.json`. Có thể truyền `--pdftotext` bằng đường dẫn binary thực tế. Không cần chạy lại nếu slide/hash chưa thay đổi.
