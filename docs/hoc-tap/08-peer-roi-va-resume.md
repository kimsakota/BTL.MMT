# Bài 08 · Lỗi giữa chừng, kết quả tới muộn và resume

**Học trước:** state/credit ở Bài 06, ownership ở Bài 07, hash/writer ở Bài 04. **Câu hỏi:** một request hết hạn có phải byte cũ biến mất không? Sau crash, bitmap lưu trên đĩa đáng tin đến đâu?

Các thuật ngữ timeout/retry ở đây thuộc ứng dụng. TCP retransmission timer do OS xử lý, TTL do tracker xử lý; cùng đơn vị giây không làm chúng thành một cơ chế.

## 1. Phân loại lỗi trước khi sửa

| Tình huống | Quan sát có thể có | Việc không được suy ra vội |
|---|---|---|
| Peer đóng chiều gửi | EOF sau dữ liệu đã đệm | Toàn tệp đã nhận xong |
| Kết nối bị reset/lỗi | Socket operation thất bại | Mọi consumer khác đã dừng |
| Không có phản hồi trước deadline | Request timeout | Peer đã chết vĩnh viễn |
| Payload đủ nhưng hash sai | Integrity failure | Chắc chắn TCP đã giao sai byte |
| Writer thất bại | I/O error/completion lỗi | Receive xong nên phiên vẫn thành công |
| Không nguồn nào có piece cuối | Unavailable | Retry vô hạn sẽ tự tạo được dữ liệu |

Mỗi lỗi có owner xử lý: transport báo connection event; scheduler xử lý request/state; writer báo ghi/hash; runtime quyết định dừng phiên hoặc tiếp tục theo policy.

## 2. Timeout là quyết định của mình

Deadline phải đủ cho workload bình thường, không chỉ RTT mạng. Một request có thể chờ sender queue, rate limiter, đọc đĩa, network và writer. Đặt quá ngắn tạo retry/lưu lượng lặp; quá dài làm phát hiện nguồn không tiến triển chậm.

Thời gian dùng elapsed/monotonic, không lấy wall-clock đang có thể chỉnh để đo duration. Chọn deadline là giả thuyết cần calibration, không lấy TCP RTO làm số bất biến cho app.

Retry cần giới hạn số lần, điều kiện chọn nguồn khác và cách báo exhausted/unavailable. Backoff giúp tránh vòng lặp dồn dập khi lỗi kéo dài; chưa cần thuật toán thích nghi phức tạp ở bản cơ sở.

## 3. Một timeline request cũ tới muộn

Ví dụ học tập, block X, cap=1 cho công việc logic đang chờ:

| Sự kiện | State logic | Completion được cộng? |
|---|---|---|
| Giao r1 cho A, generation 0 | X InFlight r1 | Chưa |
| r1 timeout trước khi nhận/ghi đủ | r1 Expired; X có thể tái giao theo điều kiện | Không |
| Giao r2 cho B, generation 1 | X InFlight r2 | Chưa |
| Response r1 tới muộn | Nhận diện r1 cũ, xử lý/drop theo framing/contract | Không |
| r2 nhận và writer hoàn tất | X Written cho lần hợp lệ | Đúng một lần |
| Callback r1 lỗi/trễ lặp lại | r1 đã là terminal | Không cộng/hoàn credit lần hai |

Generation là số hiệu lần giao việc trong state local. Nó không tự tồn tại trong header v1 và không thần kỳ ngăn mọi race. Phải map response về đúng request/connection rồi so với state hiện tại.

Nếu tái dùng cùng request ID quá sớm trên cùng connection, response rất trễ có thể bị nhầm thành request mới. Cần chính sách không reuse khi còn nguy cơ alias, giữ tombstone phù hợp, hoặc đóng connection/tạo phiên mới theo contract. “Không trùng ID đang pending” chưa tự giải quyết mọi late response.

## 4. Request terminal khác tài nguyên đã thu hồi

Timeout có thể kết thúc **quyền chấp nhận kết quả** của r1. Nhưng socket receive loop, buffer hoặc write job cũ có thể còn hoạt động. Không Return chỉ vì r1 đã Expired.

Nếu block cũ đã vào writer, không tùy tiện cho hai job generation khác ghi chồng cùng offset. Bản thiết kế cần một owner serialize việc ghi và kiểm tra state; job đang I/O phải settle hoặc có cơ chế commit/cô lập được chứng minh trước khi tái sử dụng cùng vùng. Bản cơ sở nên chọn cách đơn giản có thể giải thích.

Một response không dùng vẫn cần được đọc/drain hoặc kết nối phải đóng đúng quy tắc framing; không bỏ payload giữa dòng rồi đọc các byte còn lại như header mới.

CANCEL v1 là best effort: peer có thể đã phát BLOCK trước khi thấy CANCEL. Vì vậy nhận response sau CANCEL là tình huống cần xử lý, không phải bằng chứng giao thức TCP sai.

## 5. Disconnect không chỉ là xóa peer khỏi danh sách

Khi một connection lỗi, liệt kê các request nó sở hữu, kết thúc đúng một lần, quyết định phần nào đủ điều kiện tái giao, và xử lý các resource đang chờ. Availability nguồn ấy không được dùng tiếp như nguồn còn hoạt động.

Tracker có thể vẫn giữ endpoint đến TTL. Client không phải đợi TTL mới biết connected socket đã lỗi. Ngược lại, một entry chưa hết TTL không chứng minh connection đang sống.

Nếu còn nguồn giữ mọi phần thiếu, hệ thống có thể tiếp tục. Nếu piece cuối chỉ ở peer đã rời, báo chưa hoàn tất có lý do là hành vi đúng. Kịch bản lab giữ seed online để khảo sát failover có nguồn thay thế; không biến kết quả ấy thành chứng minh chịu mọi dạng mất dữ liệu.

## 6. Resume là khôi phục sự thật từ dữ liệu còn lại

Sau crash, `.part` và metadata có thể thuộc các thời điểm khác nhau. File có đúng size không chứng minh từng byte đã ghi; bitmap nói Verified không bảo đảm dữ liệu đã bền trước sự cố.

Trình tự cơ sở đề xuất:

1. Đọc manifest/metadata có giới hạn, kiểm tra version, fileId, size, piece/block config.
2. Kiểm tra tệp `.part` và các range hợp lệ; không đọc vượt file.
3. Rehash các piece được cho là có; chỉ những piece khớp manifest mới trở lại Verified.
4. Phần thiếu/sai trở thành cần tải lại; chưa phát HAVE cho chúng.
5. Tiếp tục truyền theo state mới; sau đủ dữ liệu kiểm tra toàn tệp.

Metadata có thể được ghi qua file tạm rồi thay thế để tránh một số trạng thái ghi dở, nhưng atomic replace và chính sách flush không đồng nghĩa sống sót qua mọi sự cố điện/thiết bị. Phải nói đang kiểm tra process kill, OS crash hay power loss; bản lab chưa mặc định chứng minh tất cả.

## 7. Ví dụ crash dễ suy luận sai

Piece 0 có 8 byte, writer đã nhận đủ input. Chương trình ghi metadata “piece 0 Verified” trước khi việc ghi/hash thực sự đạt contract, rồi dừng. Khi khởi động lại, metadata có thể nói hoàn tất trong khi tệp chưa đúng.

Rehash sẽ phát hiện piece ấy không khớp. Chỉ đọc bitmap rồi upload sẽ lan dữ liệu sai sang peer khác. Vì vậy thứ tự “ghi/hash → cập nhật state” và bước kiểm tra lại khi resume đều cần, không chọn một trong hai rồi bỏ phần còn lại.

Một test process kill có thể cho dữ liệu vẫn tồn tại vì OS cache; không dùng một lần như vậy để tuyên bố WriteAsync đã durable qua mất điện.

## 8. Bài tập tăng dần

**A · Vẽ state.** Thêm generation, credit và owner vào timeline r1 timeout/r2 thay thế/r1 muộn. Chỉ rõ thời điểm request kết thúc và thời điểm buffer được phép Return.

**B · Tái dùng ID.** Hai request cùng connection, cùng ID và cùng piece/offset nhưng khác lần giao. Nếu wire không có generation, chỉ kiểm tra piece/offset có phân biệt được response cũ không?

**C · Resume nhỏ.** Với tệp 19 byte của Bài 04, metadata nói cả ba piece hoàn tất. Sửa một byte ở offset 17 rồi rehash. Piece nào được giữ, piece nào phải tải lại?

**D · Khi được giao tích hợp.** Lặp ngắt ở các mốc receive/enqueue/write/hash, không chỉ đúng một thời điểm thuận lợi. Giữ log/error input để regression; thử tình huống thiếu nguồn phải thất bại có giải thích.

<details>
<summary>Gợi ý B–C</summary>

B: không, nếu mọi trường so khớp đều trùng thì local generation mới chưa đủ để nhận diện byte cũ; phải giải quyết mapping/ID lifetime. C: offset17 thuộc piece 2 (16–18), piece 0/1 có thể giữ nếu rehash đúng, piece 2 phải trở về thiếu/sai. Không reset toàn tệp nếu thiết kế có thể xác minh riêng, nhưng cũng không giữ bitmap giả.

</details>

Hiểu bài khi mô tả được outcome logic, lifetime và dữ liệu bền như ba vấn đề liên quan nhưng khác nhau. Tiếp [Bài 09](09-kiem-thu-va-capture.md), nơi chuyển các timeline thành ca kiểm tra có oracle.
