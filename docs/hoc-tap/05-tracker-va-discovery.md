# Bài 05 · Tracker, registry và thông tin có thể hết hạn

**Học trước:** endpoint ở Bài 01; header/control payload ở Bài 03; Dictionary và interface trong nền C#. **Câu hỏi:** tracker biết gì, biết từ khi nào, và một danh sách peer được trả về có bảo đảm kết nối được không?

Nguồn: `01 Introduction.pdf` trang PDF 52–55; `04-3 Socket.pdf` trang 32–42. Giao thức REGISTER/LIST_PEERS/HEARTBEAT ở [v1](../thiet-ke/Giao-thuc-v1.md). Không đồng nhất TTL ứng dụng ở bài này với TTL/hop-limit của IP.

## 1. Tách discovery khỏi truyền dữ liệu

Receiver biết fileId nhưng chưa biết nguồn. Nó hỏi tracker để lấy các endpoint ứng viên. Sau đó tự connect peer và trao đổi availability. Tracker bản cơ sở không chuyển BLOCK và không chứng thực mọi piece mà peer nói đang có.

```text
Peer → REGISTER(fileId, peerId, endpoint) → Tracker
Peer → HEARTBEAT(fileId, peerId)         → Tracker
Peer → LIST_PEERS(fileId)               → Danh sách ứng viên
Receiver → connect ứng viên             → hỏi bitmap/HAVE → tải dữ liệu
```

Một endpoint chỉ là địa chỉ/port tại thời điểm được biết. Peer có thể rời ngay sau khi tracker trả danh sách; client vẫn cần timeout và xử lý lỗi. Discovery thành công khác transfer thành công.

## 2. Registry là bảng gì?

Một entry tối thiểu có key `(fileId, peerId)` và value chứa host, port, lastSeen. Dùng peerId đơn lẻ làm key sẽ ghi đè đăng ký khi cùng peer chia sẻ hai tệp. Dùng host/port làm danh tính duy nhất cũng không đáp ứng nhu cầu cập nhật endpoint cho một peer.

| Thao tác | Input | Thay đổi/đầu ra cần định nghĩa |
|---|---|---|
| Register | Key, endpoint, now | Tạo mới hoặc cập nhật cùng key, cập nhật lastSeen |
| Heartbeat | Key, now | Gia hạn entry có tồn tại; key lạ xử lý theo contract |
| ListPeers | fileId, now | Chụp các entry chưa hết hạn cho tệp |
| Expire | now | Loại entry không còn hiệu lực |

REGISTER lặp không nên làm số entry tăng mãi. Đăng ký lại một key với port mới phải có chính sách cập nhật rõ, không trả hai bản cũ/mới như hai nguồn độc lập.

V1 chưa thay một cơ chế xác thực danh tính đầy đủ. Trong lab, validate định dạng/giới hạn vẫn cần; không gọi UUID hoặc hash fileId là bằng chứng peer đáng tin.

## 3. TTL và heartbeat bằng một timeline

Heartbeat là thông báo “tôi còn tham gia” theo chu kỳ ứng dụng. TTL là thời gian tracker chấp nhận entry kể từ lần cập nhật cuối. Ví dụ học tập dùng TTL=20 s, heartbeat dự kiến mỗi 5 s và **quy ước expiry age >= TTL**; khi viết API phải chốt cùng quy ước trong contract.

```text
Peer A: register t=0 → heartbeat t=5 → im lặng
```

| Now | LastSeen | Age | Kết luận theo ví dụ |
|---:|---:|---:|---|
| 0 | 0 | 0 | Có hiệu lực |
| 5 | 5 | 0 | Heartbeat gia hạn |
| 19 | 5 | 14 | Có hiệu lực |
| 21 | 5 | 16 | Vẫn có hiệu lực |
| 25 | 5 | 20 | Hết hạn đúng biên |

Tại t=21 không tính từ register t=0; phải tính từ lastSeen t=5. Đây là lỗi logic mà một câu “TTL 20 giây” chưa đủ tránh.

Chọn TTL quá gần heartbeat có thể loại peer khi có trễ xử lý. Chọn TTL rất lớn giữ endpoint chết lâu hơn. Không có một con số tốt nhất cho mọi mạng; các giá trị 5/20 là config lab được đề xuất, không phải TCP timer.

## 4. Vì sao dùng thời gian monotonic?

Để đo “đã trôi bao lâu”, đồng hồ cần không nhảy lùi do chỉnh giờ. Wall-clock UTC hữu ích ghi log/liên hệ thời điểm, nhưng máy có thể được đồng bộ hoặc người dùng chỉnh giờ.

Một implementation có thể dùng Stopwatch timestamp và chuyển qua elapsed time đúng đơn vị. Không coi raw tick của mọi API là cùng frequency. Test tốt dùng clock giả do test điều khiển, không chờ thật 20 giây.

Ví dụ kiểm thử: clock=0 register; set clock=5 heartbeat; set=24.999 còn sống; set=25 hết hạn theo quy ước đã chọn. Test chạy nhanh và xác định, không phụ thuộc scheduler của OS.

Clock được truyền như dependency nhỏ, không phải một framework DI tổng quát. Nó có ích vì logic thực sự phụ thuộc thời gian và cần hai nguồn thời gian khác nhau khi chạy/test.

## 5. Registry thuần logic trước, mạng sau

Có thể học và kiểm thử Register/List/Expire trong bộ nhớ trước khi có server socket. Sau đó adapter message chuyển payload đã validate thành lời gọi registry và encode kết quả.

Tách như vậy giúp tìm lỗi rõ: TTL tính sai ở lớp thuần logic khác header JSON bị cắt ở transport. Bài tập registry chưa chứng minh tracker qua mạng đã hoàn thành.

Khi trả kết quả, cần quyết định snapshot dữ liệu: caller giữ danh sách có được sửa registry không? Đừng trả collection nội bộ đang bị worker khác sửa rồi giả định enumeration luôn ổn. Bản cơ sở có thể dùng một owner tuần tự hoặc khóa ngắn cho mutation/snapshot; không giữ lock trong lúc gửi mạng.

Giới hạn số entry, số peer trong phản hồi và độ dài input. Một Dictionary có thể tăng không giới hạn nếu chỉ xử lý ca bình thường. Payload cap cũng có nghĩa danh sách trả về phải nằm trong giới hạn đã chốt.

## 6. Từ danh sách tới connection thật

Ví dụ tracker trả A lúc t=10, A rời lúc t=10.1, receiver connect lúc t=11. Kết nối thất bại không chứng minh ListPeers tính sai; thông tin có thể cũ ngay sau snapshot.

Client cần lọc chính mình, loại endpoint không dùng được, giới hạn số connect/inflight, dùng deadline và tránh retry liên tục cùng nguồn lỗi. Availability của piece được hỏi sau kết nối; registry fileId chưa mô tả bitmap chi tiết ở bản cơ sở.

Không advertise wildcard hoặc loopback cho máy khác. Một server listening trên 0.0.0.0 vẫn cần công bố địa chỉ được peer truy cập. Hai tracker khác nhau hoặc registry bị restart có thể mất discovery state; đó là giới hạn phải giải thích, không tự thêm DHT để làm bài lớn hơn.

## 7. Bài tập theo cấp

**A · Timeline.** TTL=20, register A t=0, heartbeat A t=5, register B t=8. Ai còn hiệu lực tại t=24,25,28? Chốt quy ước biên trước khi tính.

**B · Cấu trúc dữ liệu.** Mô tả hai lần Register cùng `(fileX,peerA)` với port 42301 rồi 42309; thêm `(fileY,peerA)`. Có bao nhiêu entry và endpoint nào cho fileX?

**C · Hàm nhỏ.** Tự viết registry với clock giả. Test cập nhật, expiry đúng biên, key lạ, lọc fileId, snapshot và giới hạn số entry. Chưa dùng sleep/socket.

**D · Khi được giao tích hợp.** Nối control frames v1, chạy nhiều peer và hai máy; đối chiếu log registry với connect timeout. Không nhầm ACK ứng dụng với TCP ACK.

<details>
<summary>Lời giải A–B</summary>

A: tại 24, A age19/B age16 đều sống; tại 25 A age20 hết hạn, B age17 còn; tại 28 B age20 hết hạn. B: hai entry vì hai fileId; entry fileX có endpoint mới 42309. Nếu cho heartbeat tự tạo entry lạ thì đó là quyết định khác cần chốt, không âm thầm suy ra từ tên thao tác.

</details>

Hiểu bài khi giải thích được key, snapshot, age, clock và endpoint stale. Đào sâu concurrency/actor khi Linh giao, sau khi logic thuần đã rõ. Tiếp [Bài 06](06-P2P-scheduler-va-pipeline.md) hoặc [hướng tracker](Lo-trinh-theo-chuyen-mon.md).
