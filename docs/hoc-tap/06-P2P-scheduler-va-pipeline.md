# Bài 06 · Chọn phần nào, lấy từ ai, giữ bao nhiêu việc đang chờ?

**Học trước:** Bài 03–05; array/Dictionary và state trong nền C#. **Mục tiêu:** tự ra quyết định từ một bảng availability nhỏ, giải thích pipeline bằng đơn vị, rồi giữ state đúng khi nhiều connection tiến triển khác nhau.

Slide: `01 Introduction.pdf` trang PDF 55, 72–74; `02 Link Layer.pdf` trang 29–48 cho ý tưởng pipeline; `04-2 Congestion control.pdf` trang 3–4 đọc cùng [ghi chú đơn vị](Ghi-chu-chuan-hoa-slide.md). Scheduler ứng dụng không thay thuật toán TCP congestion control của OS.

## 1. P2P không chỉ là mở nhiều connection

Mỗi peer có dữ liệu khác nhau ở từng thời điểm. Scheduler phải quyết định: piece/block nào còn thiếu, nguồn nào có, connection nào còn credit, có nên phát request mới hay chờ writer. Nếu chỉ mở nhiều socket rồi tải toàn bộ từ seed, chưa dùng được khả năng phân phối của receiver.

Tách ba quyết định để học:

1. **Piece selection:** chọn piece cần tải.
2. **Source selection:** chọn peer có piece và còn khả năng phục vụ.
3. **Dispatch:** dành request ID/state/credit, phát REQUEST_BLOCK qua transport.

Writer không tự chọn nguồn; tracker không quyết định từng block; transport không tự ghi piece Verified. [Kiến trúc](../thiet-ke/Kien-truc-du-an.md) là nơi chốt ranh giới.

## 2. Bitmap là cách biểu diễn một tập

Giả sử có 4 piece 0…3. Peer A có {0,1,2,3}, B có {0}, C có {1,2}. Theo bit order v1, các bit piece nằm từ bit cao xuống: A `11110000`, B `10000000`, C `01100000`; bit thừa phải bằng 0.

Bitmap chỉ là thông tin peer **công bố**. HAVE cập nhật một piece; BITFIELD trao trạng thái ban đầu. Receiver vẫn phải tự kiểm tra hash dữ liệu. Không lấy bitmap remote làm local Verified.

Đổi một bit sai có thể làm tải phần không có hoặc tưởng không còn nguồn. Test index/bit order bằng vài piece trước khi tối ưu bitset.

## 3. Sequential và rarest-first qua bảng tính tay

D chưa có piece nào; dữ liệu nguồn như mục 2:

| Piece | A có | B có | C có | Rarity nhìn từ D |
|---:|---|---|---|---:|
| 0 | Có | Có | Không | 2 |
| 1 | Có | Không | Có | 2 |
| 2 | Có | Không | Có | 2 |
| 3 | Có | Không | Không | 1 |

Sequential chọn piece thiếu có ID nhỏ nhất đủ điều kiện → 0. Rarest-first chọn piece có ít nguồn nhất **nhưng lớn hơn 0** → 3. Nguồn A đang online được tính như nguồn khác, không bỏ seed khỏi bảng rarity.

Nếu A rời, rarity piece 3 bằng 0. Không chọn nó như “hiếm nhất”; nó đang unavailable. Có thể tải piece khác nhưng không thể hứa hoàn tất tệp khi không còn nguồn chứa piece 3.

Rarest-first hướng tới tăng đa dạng bản sao và giảm phần chỉ có ở ít nguồn. Nó không bảo đảm luôn nhanh hơn: nguồn của piece hiếm có thể chậm, request overhead và tình trạng uplink khác nhau. Đó là câu hỏi cần thí nghiệm, không phải kết quả đã biết của ứng dụng.

Khi đồng hạng, dùng tie-break được mô tả rõ. Random seed giúp tái lập lựa chọn trong cùng chuỗi sự kiện, nhưng một mình seed không làm OS/network scheduling hoàn toàn xác định. Cần lưu config và log sự kiện nữa.

## 4. Pipeline: vì sao gửi một request rồi đứng đợi có thể chậm?

Giả sử mô hình lý tưởng một connection, RTT=50 ms, băng thông 100 Mbit/s. Muốn có đủ data đang bay để sử dụng link, lượng dữ liệu khoảng:

```text
BDP = 100,000,000 bit/s × 0.05 s / 8 = 625,000 byte
```

Nếu app chỉ cho 8 request, mỗi block 16 KiB: W=8×16384=131072 byte. Giới hạn kiểu W/RTT của mô hình đơn giản là khoảng 20.97 Mbit/s. Để W đạt 625000 byte cần ít nhất ceil(625000/16384)=39 request.

Đây là phân tích giới hạn ứng dụng, không dự đoán throughput thực tế hoặc giá trị cwnd. Mạng, flow control, CPU/hash/đĩa, nhiều connection và response processing còn ảnh hưởng. Đừng so trực tiếp “RTT 50 ms lớn hơn window 128 KiB” vì khác đơn vị.

Pipeline là cho vài request cùng chờ, không bỏ mọi giới hạn. Tăng số request làm tăng state, buffer và việc chờ writer; có thể tăng RAM mà không tăng tốc nếu bottleneck nằm ở đĩa.

## 5. State machine giúp tránh tính cùng block hai lần

Block đi qua các trạng thái:

```text
Missing → InFlight → Received → Written
```

Piece có state riêng: Missing/Partial → Verifying → Verified. Block Written không tự tạo một piece Verified nếu các block khác còn thiếu hoặc hash chưa đúng.

Một request cần biết connection, request ID, piece/offset/length kỳ vọng, deadline và generation của lần giao việc. Generation thay đổi khi tái giao, giúp phân biệt completion cũ với state mới. Nó có thể là bookkeeping local; v1 không tự có một field generation trên wire.

Ví dụ block X đang InFlight từ A: một vòng scheduler khác không cấp X lần nữa trong chính sách cơ sở. Nếu sau đó timeout và giao lại cho B, phản hồi A tới muộn không được tăng completion thêm một lần. Endgame tải trùng có kiểm soát là một thiết kế nâng cao riêng, không lẫn vào baseline.

## 6. Credit và ownership không phải một thứ

Credit là quyền cho thêm một lượng công việc đang chờ. Nếu connection cap=2, chỉ phát hai request; completion/failure cuối cần hoàn lại credit đúng một lần. Trước gửi phải dành state để phản hồi nhanh không tới trước lúc được ghi nhận.

Nhưng request timeout/credit rollback không có nghĩa buffer đang được socket/writer dùng đã được trả ngay. State logic và lifetime tài nguyên là hai sổ cần phối hợp. [Bài 07](07-ownership-buffer-va-Vst.md) và [Bài 08](08-peer-roi-va-resume.md) phân tích chi tiết.

Bản đầu nên có một owner xử lý state scheduler tuần tự qua event; async I/O báo completion về owner ấy. Đây là lựa chọn để dễ chứng minh invariant, không phải quy tắc mọi C# app phải dùng actor framework. Không cần thêm framework để có một vòng xử lý event.

Khi queue writer đầy hoặc budget hết, ngừng phát request mới. Tạo vô hạn Task đang đợi queue không làm toàn hệ thống bounded dù queue có capacity.

## 7. Chứng minh đã có P2P thật

Một lab 3 process A(seed), B, C phải thể hiện:

- B nhận piece, ghi/hash đúng và công bố HAVE.
- C lấy ít nhất một block/piece từ B khi B chưa nhất thiết có toàn tệp.
- Các receiver vẫn nhận đủ và hash đúng.

Log cần source peer và piece/block ID; capture/log dữ liệu B→C giúp kiểm chứng. Chỉ thấy B và C kết nối nhau hoặc cùng có tốc độ tải chưa đủ chứng minh đã trao đổi payload hữu ích.

Khi so CS/P2P, giữ cùng core codec/storage/hash và báo rõ P2P dùng thêm upload của receiver. Bài 10 giải thích cách đối chứng tài nguyên này.

## 8. Bài tập tăng dần

**A · Tập và bitmap.** Với bảng mục 3, D đã Verified piece 0 và piece 3 đang InFlight. Sequential còn chọn gì? Rarest-first chọn trong tập nào? Nếu A rời thì sao?

**B · Hàm thuần.** Viết chọn piece từ local state + remote bitmap + inflight, chưa có socket. Test không có nguồn, chỉ một nguồn, bit thừa và piece đã hoàn thành. Expected phải tính độc lập trước code.

**C · Trace credit.** Cap=2, gửi r1/r2; r1 thành công, r2 timeout, r2 cũ tới muộn. Điền outstanding/credit sau mỗi event; không trả credit hai lần.

**D · Khi được giao tích hợp.** Ghép sequential với transport/storage, chứng minh B→C, rồi thêm rarest-first như một policy thay được. Giữ cùng cap và block size khi so sánh.

<details>
<summary>Gợi ý A và C</summary>

A: bỏ piece 0 vì đã Verified, bỏ 3 vì đang chờ; còn 1/2 đồng rarity=2, sequential chọn 1. Nếu A rời thì 1/2 vẫn do C cung cấp, piece 3 không thể coi hoàn tất từ InFlight chưa xong. C: khởi đầu credit 2 → gửi r1 còn1 → gửi r2 còn0 → r1 cuối còn1 → r2 timeout còn2; phản hồi cũ không làm credit thành3. Tài nguyên của I/O cũ vẫn phải có owner cho tới kết thúc.

</details>

Bạn hiểu bài khi tự tính rarity, trace state/credit và giải thích giới hạn mô hình W/RTT. Tiếp [Bài 07](07-ownership-buffer-va-Vst.md), không cần viết cả scheduler ngay khi mới hiểu một bảng bitmap.
