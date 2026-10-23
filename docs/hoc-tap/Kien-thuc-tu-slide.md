# Bản đồ kiến thức từ slide ET4230

Đã lập chỉ mục toàn bộ **14 PDF, 697 trang**, trích nội dung để rà chủ đề và đọc sâu các phần liên quan Đề 14. Sơ đồ/công thức trọng yếu được mở bản render đại diện; không tuyên bố đã kiểm tra hình ảnh của mọi trang. Hash và page count nằm trong [slides-index.json](../tham-khao/slides-index.json).

Mọi trang bên dưới là **trang PDF 1-based**. Ví dụ `01 Introduction` trang PDF 106 có footer 115; khi tìm phải dùng 106. Thư mục nguồn là `../Slide` tính từ root BTL, các bản trích nằm ở `tmp/slides/` và không commit.

## Rà soát cả học phần

| File | Trang | Nội dung và cách dùng cho Đề 14 |
|---|---:|---|
| `01 Introduction.pdf` | 126 | Mô hình mạng, client–server 52–54/P2P 55; delay/loss 63–71; throughput 72–74; phân lớp/đóng gói 76–106. Nền để giải thích kiến trúc và nút thắt. |
| `02 Link Layer.pdf` | 50 | Framing 9–14, lỗi/ACK/timer 15–28, flow control và pipelining 29–48, Ethernet 49. Học ý tưởng ranh giới/thứ tự, phân biệt frame lớp 2 với frame ứng dụng. |
| `02-3 LAN Polling, Aloha.pdf` | 50 | MAC, chia sẻ kênh, polling/token; ALOHA 34–47. Là kiến thức môn học và giả định tải, không phải thuật toán ứng dụng phải cài vào TCP. |
| `02-4 LAN CSMA.pdf` | 54 | CSMA 3–16, CSMA/CD 17–20, Ethernet 22–31, Wi-Fi/CSMA/CA 32–51. Giải thích môi trường Ethernet/Wi-Fi khi đo; không coi Ethernet switch full-duplex là môi trường CSMA/CD chia sẻ. |
| `02-5 Hub-Bridges.pdf` | 35 | Collision/broadcast domain 3, hub/switch/router 6–17, learning bridge 18–28, STP 29–33. Dùng khi vẽ topology lab và giải thích gói đi qua thiết bị nào. |
| `03-1 IP address (updated).pdf` | 43 | Forwarding/control plane 3–20; IP/interface 21–27; DHCP 28–33; CIDR/subnet 34–43. Dùng khi bind/listen, chọn IP thật, chẩn đoán kết nối giữa máy. |
| `03-2 Internet Protocol (updated).pdf` | 50 | IPv4/fragment 3–8, NAT 9–11, IPv6/tunnel 12–17, IDS 18–37, câu hỏi 38–43, ARP/ICMP/ping/traceroute 44–50. Không đồng nhất IP fragment với piece ứng dụng. |
| `03-3 Routing Algorithm.pdf` | 29 | Longest prefix/default 2–6, static/dynamic 7–10, DV/Bellman-Ford/count-to-infinity 11–20, LS/Dijkstra 21–24. Dùng để phân biệt chọn nguồn ứng dụng với chọn đường IP. |
| `03-4 Routing Protocol (update).pdf` | 34 | AS/IGP/EGP 3–8, RIP 9–15, IGRP 16–17, OSPF 18–23, BGP 24–30, bài tập 31–33. Nền học phần; chương trình không tự thay bảng định tuyến router. |
| `03-5 Routing in WSNs and IoT applications.pdf` | 86 | WSN/node/topology 3–18, ứng dụng 19–27; ARPEES 29–37; nghiên cứu mở rộng/đánh giá 38–67; triển khai IoT 68–85. Tham khảo cách xác định baseline, metrics và giới hạn mô hình; không nhập mô hình pin vào P2P LAN. |
| `03-6 Routing Protocol for MANET.pdf` | 26 | DSDV 9–11, DSR 12–14, AODV 15–20, DYMO/geographic 21–22, biến và metrics 23–25. Liên hệ sự thay đổi kết nối, nhưng peer rời ứng dụng không có nghĩa triển khai AODV. |
| `04-1 UDP, TCP.pdf` | 28 | Transport/port/socket 3–11, reliability 12–14, UDP 15–17, TCP 18–27. Nền bắt buộc của peer transport. |
| `04-2 Congestion control.pdf` | 38 | rwnd/cwnd 3–4, lỗi/RTO 5–13, tắc nghẽn 14–25, Tahoe/Reno 28, MSS/Nagle/delayed ACK 29–37. Dùng để giải thích thực nghiệm, đọc cùng ghi chú chuẩn hóa. |
| `04-3 Socket.pdf` | 48 | Endpoint/socket 2–7, byte order 8–9, API 11–31, client/server 32–37, concurrent server 38–42. Chuyển nguyên lý C/POSIX sang API C#, không chép `fork()` vào .NET. |

## Những mối nối phải hiểu trước khi code

**Một yêu cầu block đi qua các lớp.** Scheduler tạo thông điệp yêu cầu; codec thêm header ứng dụng; socket đưa byte cho TCP; OS đóng gói IP rồi Ethernet/Wi-Fi. Router xử lý đường IP, switch xử lý frame link. Peer bên kia nhận dòng byte TCP, đọc đủ header/payload rồi giao request cho uploader. Chương trình kiểm soát giao thức ứng dụng, không tự sinh TCP ACK hay cửa sổ TCP.

**Ba loại phân chia khác nhau.** Piece là đơn vị kiểm tra hash và công bố availability; block là đơn vị yêu cầu/pipeline ứng dụng; TCP segment do stack tạo; IP fragment nếu có lại thuộc lớp mạng. Một block có thể đi qua nhiều segment, một lần receive có thể chứa thiếu hoặc ghép nhiều thông điệp. Đối chiếu Intro 101–106, IP phần fragmentation và Socket 20–21.

**Ba loại điều tiết khác nhau.** rwnd bảo vệ phía nhận TCP, cwnd liên quan nghẽn đường truyền, còn số request/pool capacity của ứng dụng bảo vệ RAM và đĩa. Khi hàng đợi ghi đầy, scheduler ngừng phát thêm request; đây không phải nhóm đã hiện thực TCP congestion control.

**Nút thắt và đơn vị.** Truyền L bit trên link R bit/s cần khoảng L/R giây; lan truyền khoảng d/v; hàng đợi thêm trễ phụ thuộc tải. Goodput đếm payload hữu ích đã xác minh, không đồng nhất với line rate. Dùng Mbit/s hệ thập phân, KiB/MiB là bội nhị phân. Trong mô hình đơn giản có window W byte và RTT giây, giới hạn do window xấp xỉ 8W/RTT bit/s; hiệu năng còn bị giới hạn bởi link, CPU, hash và đĩa.

**Trình tự trong TCP không tạo thứ tự toàn hệ thống.** Mỗi kết nối là một dòng có thứ tự; các kết nối từ các nguồn khác nhau vẫn hoàn tất block không theo thứ tự. Vì vậy storage cần offset rõ, scheduler cần state machine và ownership.

## Cách dùng slide để học, không chỉ tìm số trang

Trước một mục được giao, đọc bài giảng tương ứng để có thuật ngữ và một ví dụ nhỏ. Sau đó mở đúng trang PDF, chỉ ra mỗi khối/mũi tên/trường đang nói về lớp nào; tự kể một đường đi dữ liệu và viết một câu hỏi chưa hiểu. Quay về bài tập để tính/vẽ bằng input khác, rồi đối chiếu lời giải. Chỉ nhìn hình hoặc thuộc tên API chưa đủ để viết code đúng.

Không cần đọc liền 697 trang trước khi bắt đầu. Nền chung nằm ở Bài 00–03 và [nền C#](CSharp-nen-tang.md); phần sâu được chọn trong [lộ trình theo chuyên môn](Lo-trinh-theo-chuyen-mon.md). Linh tự giao ai đọc mục nào hàng tuần. Danh sách bên dưới là thứ tự nền mạng để tra cứu, không phải phân công tuần 1 bắt buộc cho mọi người.

## Thứ tự kiến thức mạng để tham khảo

1. Intro 4–13, 52–55, 63–74, 79–106; tự vẽ đường đi một tệp.
2. IP address 21–43; Internet Protocol 9–11 và 44–50; tự phân biệt IP lắng nghe với IP công bố.
3. TCP 3–11, 18–27 và Socket 2–21, 27, 32–42; viết bài echo nhỏ sau khi hiểu byte stream.
4. Link Layer 29–48 và Congestion 3–13, 22–37; tính BDP và pipeline ở mức ứng dụng.
5. Học C# async/Memory/ownership cùng [các bài giảng và ví dụ](README.md); thực hành ở mức phù hợp sau khi giải thích được cơ chế.

Các phần ALOHA, STP, routing, WSN/MANET vẫn cần học để nắm toàn môn, nhưng không làm thêm các đề mô phỏng đó vào phạm vi Đề 14. Đọc [ghi chú chuẩn hóa](Ghi-chu-chuan-hoa-slide.md) trước khi dùng công thức để giải thích dữ liệu.
