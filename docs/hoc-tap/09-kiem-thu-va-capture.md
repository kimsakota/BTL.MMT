# Bài 09 · Biết điều mình viết đúng bằng cách nào?

**Học trước:** cơ chế của phần đang test; không cần học hết P2P mới đọc bài này. **Mục tiêu:** tự viết expected result trước implementation, phân biệt loại bằng chứng và chọn cách tái hiện lỗi không phụ thuộc may rủi.

Slide: `01 Introduction.pdf` trang PDF 124; `04-1 UDP, TCP.pdf` trang 19–27; `04-3 Socket.pdf` trang 38–42. Danh mục ca dự án là [T01–T15](../kiem-chung/Ke-hoach-kiem-thu.md), chưa phải bảng kết quả đã pass.

## 1. Một test cần oracle, không chỉ cần chạy không crash

Oracle là cách xác định kết quả kỳ vọng. Ví dụ CountPieces(19,8) phải là 3 do đã tính bằng giấy. Nếu hàm cho 2 mà test chỉ in kết quả rồi kết thúc 0, test không phát hiện lỗi.

Một test rõ thường gồm Arrange → Act → Assert:

```text
Arrange: tệp 19 byte, piece size 8
Act: gọi hàm tính số piece
Assert: kết quả phải bằng 3, sai thì test thất bại
```

Tên test nên chỉ điều kiện và hành vi, ví dụ “tệp có phần dư tạo thêm một piece”. Không lấy chính công thức trong implementation làm expected ở mọi test; có thể sao chép cùng lỗi.

Encode rồi decode được object cũ chưa đủ chứng minh đúng wire format. Cả hai có thể cùng sai endian. Cần byte kỳ vọng độc lập từ [vector](../../tests/protocol-vectors.json).

## 2. Các lớp kiểm tra trả lời các câu khác nhau

| Lớp | Input/môi trường | Chứng minh được gì? | Chưa chứng minh gì? |
|---|---|---|---|
| Build | Source/reference/compiler | Biên dịch được mã hiện có | Logic/runtime đúng |
| Unit | Hàm/module nhỏ, input xác định | Rule được chọn đúng trong ca đó | Hai máy kết nối được |
| Integration | Nhiều module/connection thật | Hợp đồng ghép nhau hoạt động | Mọi môi trường mạng đều ổn |
| System/lab | Process/máy thật | Luồng end-to-end trong môi trường ghi lại | Hiệu năng ở môi trường khác |
| Benchmark | Workload/đo được kiểm soát | Metric theo điều kiện đã công bố | Không còn lỗi correctness |

Checker repo hiện kiểm link/config/vector/hash; nó không chạy codec ứng dụng vì codec chưa có. Một bảng test hoặc một file vector tồn tại cũng chưa là evidence pass.

## 3. Biến dữ liệu biên thành ca kiểm tra

Cho số byte/piece/block, thử quanh biên: 0,1,B−1,B,B+1,P−1,P,P+1. Chọn biên vì nhánh logic đổi ở đó, không chỉ để tăng số lượng test.

Với parser: header đủ/thiếu; cắt tại mọi vị trí; hai frame ghép; cap−1/cap/cap+1 theo loại hợp lệ; uint32 cực đại; sai magic/version/type/flags. Ca input lớn phải kiểm tra từ chối trước allocation không hợp lệ, không thực sự tạo một mảng vài GiB để thử may mắn.

Với registry: thời điểm trước/đúng/sau TTL, cùng key cập nhật, khác fileId, snapshot. Với scheduler: thiếu nguồn, đồng hạng, block đang chờ, completion lặp, r1 muộn sau r2.

## 4. Test stream và clock giả vì điều gì?

Fake stream có thể được điều khiển trả 1,3,12 byte để buộc parser đi qua các trạng thái. Dữ liệu vẫn giữ đúng thứ tự/nội dung; đây là **phân mảnh cách đọc**, không giả lập packet loss mạng.

Fake clock cho test nhảy thẳng từ t=5 tới t=25. Không cần sleep20s để kiểm expiry. Test có thể dùng barrier/event điều khiển thứ tự enqueue/write/cancel thay vì hy vọng một race xuất hiện đúng lúc.

Vẫn cần socket integration sau fake stream: abstraction test không kiểm mọi lỗi API/lifecycle. Stress hữu ích để tìm vấn đề, nhưng chạy 10000 lần không crash chưa là chứng minh ownership đúng.

## 5. Một bảng test hoàn chỉnh nhỏ

Đây là **thiết kế test**, chưa được chạy:

| Input/chuỗi sự kiện | Expected | Vì sao quan trọng |
|---|---|---|
| HAVE piece 2 từ vector | Frame type4, ID0, payload4 đúng byte | Oracle độc lập |
| Hai frame, feed từng byte | Hai message đúng thứ tự | Parser không phụ thuộc receive boundary |
| EOF sau 5 byte header | Truncated frame, không success | Failure path |
| Length=0xffffffff | Reject trước allocation lớn | Giới hạn input |
| Timeout r1 rồi response r1 muộn | Không double completion/credit | State terminal một lần |

Mỗi test phải có assert quyết định đạt/trượt. Runner/CI phải trả nonzero khi test fail. Framework cụ thể được nhóm chọn; học cấu trúc test trước, không tạo project test rỗng rồi coi đã kiểm codec.

## 6. Log và capture bổ sung nhau

Log ứng dụng biết peerId, fileId, requestId, piece, generation, state và lý do reject. Capture biết endpoint, thời điểm gói, TCP sequence/ACK và bytes quan sát được. TCP sequence number không phải requestId ứng dụng.

Log tốt cho phép nối hai phía bằng connection/request thông tin phù hợp; khi ID chỉ có nghĩa trên một connection, log thiếu connection ID dễ ghép nhầm.

[Wireshark hướng dẫn xem và lọc packet](https://www.wireshark.org/docs/wsug_html_chunked/ChapterWork.html) phân biệt công cụ xem dữ liệu; mục tiêu của lab là tìm được luồng tương ứng với sự kiện, không chỉ chụp một ảnh nhiều dòng xanh.

## 7. Một quy trình capture có mục tiêu

1. Ghi topology, IP/port và lệnh đúng revision; chọn loopback nếu cùng máy, NIC phù hợp nếu qua LAN.
2. Capture filter theo port lab, ví dụ `tcp portrange 42300-42308` nếu đó là các port thực đã chọn. Filter này loại dữ liệu trước khi lưu.
3. Chạy một ca nhỏ đã biết input; lưu log từng process và pcap theo run ID.
4. Dùng display filter như `tcp.port == 42301` hoặc `tcp.stream eq 0` để xem một phần capture; stream 0 chỉ là ví dụ, phải chọn stream thật.
5. Follow TCP Stream/reassembly để tìm header E423 và payload; một packet đơn lẻ không nhất thiết chứa đủ frame.
6. Đối chiếu thời điểm connect/request/completion/error; ghi cả trường hợp capture không nhìn đủ sự kiện.

Capture filter và display filter có cú pháp khác nhau. SYN/SYN-ACK/ACK cho thấy mở TCP, không có nghĩa HELLO ứng dụng hoặc manifest đã hợp lệ. FIN/RST không tự chứng minh tệp đúng.

Checksum cảnh báo trên host có thể liên quan offload; không kết luận nội dung ứng dụng lỗi chỉ từ màu cảnh báo. Capture ở hai nơi có thể thấy cùng một dữ liệu, không cộng đôi thành hai lần upload. Thiếu packet trong capture cũng có thể do vị trí/filter/buffer capture, cần điều tra.

## 8. Fault injection có nhãn rõ

Muốn test hash sai thì cố ý sửa một byte dữ liệu ứng dụng/tệp trong ca có kiểm soát, rồi mong receiver từ chối. Đọc ít byte từ TCP không phải tạo “packet corruption”. Muốn test nguồn rời thì dừng đúng peer theo lịch và ghi việc đó là dự kiến.

Muốn test writer chậm thì làm consumer giả bị chặn có kiểm soát; kiểm rằng backlog/budget giữ giới hạn. Không cần tự đổi mạng host chính hoặc cài luật firewall để kiểm logic queue.

## 9. Bài tập và tự chấm

**A.** Một test chỉ chạy Encode rồi Decode và so object cũ. Tạo ví dụ cả hai cùng sai mà test vẫn pass.

**B.** Viết bảng input/expected cho 5 biên của module mình, trước khi viết implementation. Reviewer kiểm expected bằng tính tay hoặc nguồn độc lập.

**C.** Chọn một lỗi từng gặp, thu input tối thiểu, viết regression làm bản lỗi thất bại và bản sửa đạt. Không chỉ thêm assert phản ánh cách cài hiện tại.

**D.** Khi được giao lab, nối một REQUEST_BLOCK trong log với stream capture; giải thích bytes nào thuộc framing và bytes nào thuộc TCP.

<details>
<summary>Gợi ý A và tiêu chí evidence</summary>

A: Encode uint theo little-endian dù contract big-endian, Decode cũng little-endian; round-trip vẫn bằng object cũ. Vector byte kỳ vọng sẽ phát hiện. Evidence cho C/D gồm lệnh, revision/dirty state, môi trường, input/config, expected, actual và exit; ảnh màn hình không thay dữ liệu nguồn nếu cần tái lập.

</details>

Bạn hiểu bài khi nói rõ mỗi bằng chứng trả lời câu hỏi nào và tự đặt được oracle. Tiếp [Bài 10](10-thuc-nghiem-va-phan-tich.md); kết quả test đúng không tự là kết quả nhanh.
