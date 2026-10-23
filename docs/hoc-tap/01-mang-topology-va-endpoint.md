# Bài 01 · Byte đến đúng ứng dụng bằng cách nào?

**Học trước:** Bài 00, ý nghĩa process và byte[]. **Sau bài này:** tự vẽ tracker/seed/peer, phân biệt IP/port và giải thích vì sao ba receiver cùng tải server chưa chứng minh P2P.

Slide: `01 Introduction.pdf` trang PDF 52–55, 63–74, 101–106; `03-1 IP address (updated).pdf` trang 21–43; `03-2 Internet Protocol (updated).pdf` trang 9–11, 44–50; `04-3 Socket.pdf` trang 2–9. Đọc [ghi chú chuẩn hóa](Ghi-chu-chuan-hoa-slide.md) khi footer khác số trang PDF.

## 1. Bắt đầu từ một nhu cầu rất cụ thể

Máy A có tệp, máy B cần tệp. Chương trình B phải biết: liên hệ máy nào, ứng dụng nào trên máy đó, gửi thông điệp gì, đọc phản hồi ra sao và làm sao biết tệp đúng. Đây là nhiều câu hỏi khác nhau; một địa chỉ IP chưa trả lời hết.

Mô hình phân lớp chia trách nhiệm:

| Lớp | Câu hỏi | Trong đề tài |
|---|---|---|
| Application | Byte có nghĩa gì? | HELLO, REQUEST_BLOCK, BLOCK, manifest |
| Transport | Trao đổi giữa các endpoint ứng dụng ra sao? | TCP cung cấp dòng byte có thứ tự |
| Network | Datagram đi tới IP đích theo đường nào? | IP và định tuyến của mạng/OS |
| Link | Truyền qua link kế tiếp như thế nào? | Ethernet/Wi-Fi, frame lớp 2 |
| Physical | Biểu diễn tín hiệu/bits ra sao? | Cáp/sóng và phần cứng |

Scheduler chọn **nguồn cung cấp piece**, không chọn đường OSPF. Codec thêm **header ứng dụng**, không tự tạo TCP header. Trách nhiệm đúng giúp biết lỗi phải tìm ở đâu.

## 2. IP, port và một kết nối

IP là địa chỉ logic của interface trong mô hình này; một máy có thể có nhiều interface/IP. Port giúp phân biệt endpoint transport. MAC có phạm vi chuyển frame trên link, không thay IP/port để chọn peer ở tầng ứng dụng.

Ví dụ lab giả định, không phải thông tin máy nhóm:

```text
A = 192.168.10.21, peer listener TCP 42301
B = 192.168.10.22, peer listener TCP 42301
Kết nối B → A: B:52010 → A:42301
```

Hai máy có thể cùng listener port 42301 vì IP khác nhau. 52010 minh họa port phía client được chọn khi mở kết nối; không phải port người dùng bắt buộc cấu hình. Một kết nối TCP được phân biệt bởi hai đầu IP/port, trong ngữ cảnh giao thức TCP.

A có thể có nhiều connected socket cùng local port 42301, được phân biệt bằng remote endpoint. Listener không biến mất sau một accept; nó tiếp tục nhận kết nối mới. Nếu nhiều process trên cùng máy muốn bind cùng IP/protocol/port, cấu hình lab thông thường phải chọn port listener khác nhau; các cơ chế reuse chuyên biệt không phải thiết kế cơ sở.

## 3. Loopback, bind và advertise khác nhau

| Giá trị/thao tác | Ý nghĩa | Lỗi hay gặp |
|---|---|---|
| 127.0.0.1 | Quay về chính máy đang dùng địa chỉ | Máy B dùng 127.0.0.1 để tìm dịch vụ trên A |
| Bind 0.0.0.0 | Lắng nghe trên các interface IPv4 phù hợp | Gửi 0.0.0.0 cho tracker như địa chỉ đích |
| Bind IP cụ thể | Chỉ lắng nghe trên interface được chọn | Chọn IP không thuộc máy hoặc sai interface |
| Advertise IP | Công bố endpoint peer khác có thể truy cập | Công bố loopback cho peer trên máy khác |

Nếu A bind wildcard, B vẫn cần IP thực của A. Nếu hai process cùng một máy, loopback có thể đúng. Vì vậy “địa chỉ hợp lệ” còn phụ thuộc **ai đang dùng nó và từ đâu**.

Subnet cho biết cách chia địa chỉ mạng. Ví dụ hai IP 192.168.10.21/24 và 192.168.10.22/24 thuộc cùng prefix 192.168.10.0/24; nếu thiết bị và cấu hình cho phép, chúng có thể trao đổi trong LAN. IP 192.168.11.22/24 khác prefix; đường đi có thể cần gateway. Đừng kết luận chỉ từ ba octet giống nhau nếu prefix không phải /24.

NAT chuyển đổi địa chỉ/port ở biên mạng. Có địa chỉ riêng và kết nối ra ngoài được không tự có nghĩa peer ngoài Internet kết nối ngược vào listener được. Bản cơ sở dùng LAN; NAT traversal chưa thuộc phạm vi.

## 4. Client–server và P2P là vai trò, không chỉ là số máy

Client–server baseline: nguồn A gửi dữ liệu cho B và C; B/C không upload cho nhau. P2P: B nhận một piece, kiểm tra đúng, rồi có thể gửi piece ấy cho C khi chưa có toàn bộ tệp.

```text
Baseline:       B ← A → C
P2P dữ liệu:    B ← A → C, đồng thời B ↔ C khi có piece phù hợp
Discovery:     A/B/C hỏi tracker T; T trả địa chỉ, không chuyển tệp
```

Tracker chỉ giúp tìm endpoint. Nó không phải router, không nằm trên mọi đường data. Tracker lỗi có thể làm discovery mới thất bại trong khi các kết nối peer đã có vẫn chuyển dữ liệu được; kết quả cụ thể phụ thuộc runtime sẽ thiết kế.

Một process P2P vừa đóng vai client khi connect ra, vừa server khi accept/upload. Điều đó không đòi hỏi mọi cặp peer đều kết nối nhau. “Có 3 process tải cùng lúc” chưa đủ: phải thấy receiver chia sẻ dữ liệu đã Verified cho receiver khác.

## 5. Theo dấu một yêu cầu block

1. Scheduler của B thấy thiếu piece 2, block đầu tiên; chọn A đang công bố piece ấy.
2. Codec B chuyển request thành header + payload byte theo v1.
3. Socket B giao byte cho TCP; OS xử lý vận chuyển qua IP/link.
4. TCP bên A cung cấp dòng byte; codec A lắp lại frame và kiểm tra input.
5. Upload logic A chỉ phục vụ piece đã Verified, trả BLOCK.
6. B nhận, ghi đúng offset, hash rồi mới công bố piece hoàn tất.

**Piece** là đơn vị kiểm tra hash/công bố availability; **block** là đơn vị yêu cầu ứng dụng; **TCP segment** là đơn vị TCP trên đường truyền; **IP fragment** là khái niệm khác ở lớp mạng. Một block 16 KiB có thể đi qua nhiều segment. Một receive trả 1000 byte không nói 15384 byte còn lại đã mất.

## 6. Trễ, throughput và goodput qua số cụ thể

Ví dụ lý tưởng: đưa 1 MiB lên link 100 Mbit/s, bỏ qua header và mọi chi phí khác:

```text
L = 1,048,576 byte = 8,388,608 bit
Thời gian serialization = L(bit) / R(bit/s)
                         = 0.08388608 s ≈ 83.89 ms
```

Đây là thời gian đưa lượng bit đó lên link, không phải toàn bộ completion time. Còn propagation, queueing, chờ protocol, CPU/hash và đĩa. Với các link 100/20/50 Mbit/s theo một đường, bottleneck lý tưởng là 20 Mbit/s, không phải tổng 170.

Throughput phải nêu đang đếm byte ở đâu. Goodput của đề tài đếm dữ liệu tệp hữu ích theo quy ước hoàn tất đã xác minh; không cộng header hoặc block lặp như byte tệp mới. Mbit/s dùng 10^6 bit/s; MiB dùng 2^20 byte. Việc nhân 8 là đổi byte thành bit.

## 7. Thực hành từ giấy tới lab

**A · Vẽ topology.** Đặt T, A, B cùng máy, rồi chuyển B sang máy thứ hai. Ghi listener và advertised endpoint; chỉ ra giá trị nào phải đổi. Chưa cần code.

**B · Phân loại lỗi.** B báo connection refused; B kết nối nhưng treo chờ thông điệp; tệp đủ byte nhưng hash sai. Với từng ca, nêu ít nhất một lớp/cơ chế phải kiểm tra. Đừng mặc định tất cả là “mất mạng”.

**C · Tính toán.** Tính thời gian serialization của 2 MiB ở 20 Mbit/s. Nếu chương trình báo 20 MB/s trên link ấy, hãy kiểm tra đơn vị và cửa sổ đo trước khi tuyên bố rất nhanh.

**D · Khi Linh giao lab.** Điền IP/prefix/port bằng dữ liệu thật, chạy echo sau Bài 02. Chỉ chẩn đoán/cấu hình theo lab được chọn; bài học không yêu cầu tự đổi firewall hoặc cấu hình router của máy chính.

<details>
<summary>Gợi ý cho A–C</summary>

A: cùng máy cần phân biệt các listener; B sang máy khác phải dùng IP A/T có thể truy cập từ B và công bố IP B thay loopback. B: refused có thể không có listener đúng endpoint; treo có thể hai đầu cùng đợi hoặc framing sai; hash sai có thể offset/buffer/input thay đổi dù TCP chạy bình thường. C: 2×1048576×8/20000000 = 0.8388608 s. 20 MB/s là 160 Mbit/s, cần xem lại điều đang đo.

</details>

Bạn hiểu bài khi kể được đường đi một request, giải thích bốn loại đơn vị dữ liệu và tự chọn endpoint đúng cho lab. Tiếp [Bài 02](02-socket-TCP-va-async.md); người muốn đào sâu IP học thêm theo [lộ trình chuyên môn](Lo-trinh-theo-chuyen-mon.md).
