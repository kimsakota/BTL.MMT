# Bài 11 · Giải thích được điều mình hiểu và điều mình đã làm

**Học trước:** module liên quan và cách đọc evidence ở Bài 09–10. **Mục tiêu:** chuyển kiến thức, thiết kế, code và số đo thành một lập luận mà người khác kiểm tra được.

Yêu cầu/rubric ở [Tổng quan](../Tong-quan.md); dàn ý và checklist bàn giao ở [Báo cáo và demo](../Bao-cao-va-demo.md). Bài này dạy cách giải thích, không tạo một bản báo cáo kết quả khi ứng dụng chưa có.

## 1. Một phát biểu kỹ thuật cần ba phần

| Phần | Câu hỏi | Ví dụ cách viết |
|---|---|---|
| Claim | Đang khẳng định điều gì? | Codec không phụ thuộc cách chia Receive |
| Evidence | Người khác xem đâu để kiểm tra? | Vector cố định, test split tại mọi byte, integration socket |
| Scope/limit | Phát biểu đúng trong phạm vi nào? | Version1, type đã hỗ trợ, payload cap đã chốt |

“Chương trình chạy tốt” thiếu cả điều kiện và tiêu chí. “Đã test hết” không thể kiểm tra nếu không có danh sách input, expected, revision và lệnh.

Một test pass không chứng minh mọi case. Một sơ đồ thiết kế cũng không chứng minh source đã có. Khi viết báo cáo, đặt đúng nhãn: yêu cầu, đề xuất, implementation hiện có, kết quả thực nghiệm và giới hạn.

## 2. Kể đường đi một block để nối các môn kiến thức

Tự kể thành một chuỗi, không chỉ đọc tên class:

1. Local state cho biết block nào thiếu; remote bitmap cho biết nguồn ứng viên.
2. Scheduler dành request ID/credit và expected piece/offset/length.
3. Codec biểu diễn thông điệp theo big-endian; socket vận chuyển byte qua TCP.
4. Peer nguồn chỉ upload piece đã Verified.
5. Receiver parser nhận đủ/validate, chuyển buffer theo hợp đồng tới writer.
6. Writer ghi đúng offset; piece đủ thì hash; đúng mới Verified/HAVE.
7. Lỗi/timeout tới bước nào thì state và resource ở bước ấy có failure path.

Nếu không biết giải thích tại một mũi tên, quay về bài tương ứng. Một người không cần thuộc toàn bộ implementation của người khác, nhưng phải hiểu contract mình gọi và dữ liệu mình trao.

## 3. Ví dụ một câu hỏi bảo vệ và cách xây câu trả lời

**Câu hỏi:** “TCP tin cậy rồi, tại sao nhóm vẫn dùng SHA-256?”

Cách trả lời có lập luận:

- TCP cung cấp dòng byte trong kết nối, không chứng minh receiver ghi đúng offset hoặc nguồn đang phục vụ đúng phiên bản tệp.
- Manifest cung cấp digest kỳ vọng; hash piece giúp phát hiện phần sai trước khi chia sẻ lại, hash toàn tệp kiểm đầu ra.
- Test có thể cố ý sửa byte ứng dụng hoặc ghi sai offset để kiểm receiver không công bố Verified.
- Hash theo manifest tin cậy là kiểm tra integrity của bài lab, chưa tự xác thực danh tính peer hoặc tạo TLS.

Đây là hướng lập luận, không phải đoạn bắt học thuộc. Người làm module cần chỉ được code/test thật hỗ trợ từng phát biểu khi implementation đã có.

## 4. Từ module riêng tới câu hỏi cá nhân

| Nếu được Linh giao mảng | Phải tự giải thích được | Evidence phù hợp khi đã làm |
|---|---|---|
| Transport/codec | Partial I/O, header/endian, EOF, lifetime | Vector/test malformed/split, trace connection |
| Manifest/storage | Offset cuối, hash, Written/Verified | Test biên/ghi đảo thứ tự/corrupt data |
| Tracker/discovery | Key, clock, TTL biên, endpoint stale | Fake-clock test, registry snapshot và lab |
| Scheduler | Rarity, credit, generation, source rời | Bảng trace, unit state và trao đổi B→C |
| Test/harness/thực nghiệm | Oracle, start/end, denominator, failed run | Lệnh runner, metadata/raw/plot tái lập |

Bảng là yêu cầu hiểu theo chuyên môn, không gán tên hoặc đóng góp. Tất cả vẫn cần nền chung về process, IP/port, byte stream, piece và Verified. Người làm test/harness có phần kỹ thuật thực sự; không biến vai trò ấy thành chỉ chụp ảnh và viết bìa.

## 5. Viết báo cáo từ công việc thật, không chép nhật ký thao tác

Bố cục chính theo tài liệu bàn giao: vấn đề/mục tiêu → cơ sở môn → thiết kế → hiện thực/kiểm thử → thực nghiệm/kết quả → giới hạn. Mỗi đoạn trả lời một câu hỏi của người đọc chưa biết cuộc trao đổi nhóm.

Ví dụ chuyển nhật ký thành giải thích:

```text
Nhật ký: “Sửa writer rồi chạy lại ba lần.”
Nội dung kỹ thuật: “Hai worker dùng chung Seek/Write có thể làm sai offset.
Bản sửa serialize cặp thao tác trong một writer. Regression gửi block
đảo thứ tự và đối chiếu nội dung/hash để kiểm lại lỗi này.”
```

Chỉ viết câu bản sửa đạt khi có source và test thật. Nếu mới phân tích nguy cơ, ghi đó là thiết kế/ca cần kiểm tra. Không nhập số ví dụ học tập ở Bài 10 thành kết quả hệ thống.

Trích slide bằng tên file và trang PDF 1-based; nêu source/phiên bản Vst và biến thể/adapter tự viết. Khai báo hỗ trợ AI theo yêu cầu môn, nhưng không lấy việc có assistant chuẩn bị khung thành đóng góp code của một sinh viên.

## 6. Demo là một chuỗi bằng chứng ngắn

Với giới hạn 3–5 phút, cần ưu tiên một luồng chứng minh được cơ chế:

| Khoảng gợi ý | Nội dung | Vì sao có mặt? |
|---|---|---|
| 30 s | Topology/role và môi trường | Người xem biết ai nối ai |
| 45 s | Tracker/seed/receiver ready | Chứng minh điều kiện chạy |
| 60 s | Tải nhiều nguồn và receiver upload | Chứng minh P2P thật |
| 45 s | Một peer rời và công việc được giao lại | Chứng minh failure path đã kiểm tra |
| 45 s | Hash/output và một kết quả thật | Chứng minh đúng dữ liệu, giải thích giới hạn |

Tổng khoảng 3 phút 45 giây, là kịch bản đề xuất. Chỉ giữ phần đã có; không dựng cảnh báo thành công giả để đủ kịch bản. Command/config phải đúng revision. Một hình Wireshark nhiều packet không thay việc chỉ ra stream/frame có ý nghĩa.

## 7. Diễn tập câu hỏi theo mức khó

**Nền chung:** IP khác port thế nào? Vì sao một send không bằng một receive? Piece khác TCP segment ra sao?

**Cơ chế:** ai giữ buffer khi WriteAsync đang pending? Vì sao metadata Verified cần rehash sau crash? Vì sao timeout chưa nói peer chết?

**Phản biện:** nếu P2P nhanh vì receiver upload thêm thì phép so nói được gì? Nếu Vst ít GC hơn nhưng throughput không đổi thì có sai không? Nếu nguồn duy nhất rời thì kết quả đúng là gì?

**Bằng chứng:** mở một test và giải thích oracle; từ một plot truy tới CSV/config/revision; chỉ ra phần mình làm và phần mới là đề xuất. Không tự xác nhận nhận việc/hoàn thành của người khác.

## 8. Bài luyện và gợi ý

**A.** Sửa ba phát biểu: “build pass nên P2P chạy”, “GC nên không có lỗi buffer”, “v1 là BitTorrent chuẩn”.

**B.** Chọn một quyết định thiết kế, viết 5 câu: vấn đề, cách chọn, phương án khác, evidence cần có, giới hạn. Chưa có evidence thì nói rõ chưa kiểm chứng.

**C.** Kể một failure path của phần được giao trong 90 giây, rồi trả lời “ai dọn tài nguyên?”. Người nghe đặt thêm một tình huống xen giữa hai bước.

<details>
<summary>Gợi ý A</summary>

Build chỉ kiểm biên dịch mã hiện có. GC không tự bảo đảm lease/alias và unmanaged lifetime. V1 là giao thức nội bộ lấy cảm hứng phân phối P2P, chưa tương thích BitTorrent. Cả ba cần sửa phạm vi claim, không chỉ thay một từ cho mềm hơn.

</details>

Bạn hiểu bài khi giải thích được cơ chế bằng lời của mình và chỉ được evidence thật, không cần học thuộc một bài nói. Quay lại [lộ trình theo chuyên môn](Lo-trinh-theo-chuyen-mon.md) để chọn chỗ cần bù kiến thức.
