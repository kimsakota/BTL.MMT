# Chọn kiến thức riêng theo phần được giao

Tài liệu để **Linh chọn bài đọc và mức thực hành cho từng người mỗi tuần**. Các hướng A–E không gắn sẵn tên thành viên. Một người có thể học hai hướng hoặc đổi hướng; chưa có đánh giá nền tảng/việc nhận nhiệm vụ của ai.

Đây không phải năm hồ sơ cá nhân cần điền. Mỗi hướng trả lời: thiếu nền nào thì bù ở đâu, đọc mục nào để hiểu điều gì, tự giải ví dụ nào, làm bài nhỏ nào trước khi ghép hệ thống.

## 1. Chọn mức bắt đầu, không nhảy từ tên task sang code

Thử trả lời bằng lời của mình:

| Câu tự kiểm tra | Nếu chưa rõ, đọc |
|---|---|
| Gán byte[] sang biến khác có copy byte không? | Nền tảng C# mục 3/6 |
| Hai process có tự cùng nhìn một Dictionary không? | [Bài 00](00-mo-du-an-va-CSharp.md) mục 3 |
| IP/port và bind/advertise khác gì? | [Bài 01](01-mang-topology-va-endpoint.md) mục 2/3 |
| Receive trả ít byte là lỗi hay có thể bình thường? | [Bài 02](02-socket-TCP-va-async.md) mục 1/3 |
| Piece đã nhận đủ nhưng hash sai có upload không? | [Bài 04](04-truyen-tep-va-integrity.md) mục 4/5 |

Các câu này không phải kết quả kiểm tra đã được ghi cho thành viên nào. Mở [Nền tảng C#](CSharp-nen-tang.md) và [mục lục](README.md) theo ô cần bù.

Một phần học có ba mức: **giải thích** cơ chế → **tự viết/test một phần nhỏ** → **ghép và debug failure path**. Linh chọn mức tương ứng quỹ giờ/nền tảng. Chưa cần yêu cầu người mới vừa học Socket vừa làm pool unsafe.

## 2. Hướng A · Transport và giao thức

**Cần biết trước:** hàm/exception, byte/uint/int, array/slice, Task/await và Dispose. Biết listener khác connected socket, không coi Send=message.

| Đọc theo thứ tự | Đọc để hiểu gì? | Câu phải tự giải |
|---|---|---|
| [Bài 01](01-mang-topology-va-endpoint.md) mục 2–5 | Endpoint và đường đi byte | Hai máy cùng port 42301 có xung đột không? |
| [Bài 02](02-socket-TCP-va-async.md) mục 1–4 | Stream, partial I/O, EOF | Header 16 nhận 5+7+4 thì offset đổi ra sao? |
| [Bài 03](03-framing-va-giao-thuc.md) mục 2–6 | Endian, parser state, validation | Request length 12 vì sao yêu cầu data 16384? |
| [Bài 07](07-ownership-buffer-va-Vst.md) mục 2–4; [Bài 08](08-peer-roi-va-resume.md) mục 3–4 | Buffer lifetime và late response | Timeout request có cho Return ngay không? |
| [Bài 09](09-kiem-thu-va-capture.md) mục 1–5 | Oracle và fake stream | Vì sao round-trip encode/decode chưa đủ? |

**Nguồn đọc sâu:** `04-3 Socket.pdf` trang PDF 8–9, 11–21, 27, 32–42; `04-1 UDP, TCP.pdf` trang 18–27. [ReceiveAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync?view=net-10.0) đọc phần giá trị trả về/ngoại lệ của overload Memory; [SendAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendasync?view=net-10.0) đọc ý nghĩa count. Không cần học mọi overload trước.

**Bài riêng trước khi ghép:** tự trace hai frame 48 byte theo chunk 1/3/12/2/30, viết header codec/test vector, rồi parser qua fake stream. Sau đó mới nối echo socket. Khi được giao integration, một reader và một send owner trên mỗi connection, có cancel/cleanup rõ.

**Đào sâu thêm khi có nền:** backpressure giữa network/writer, request ID lifetime, framing malformed trước allocation, tác động buffer/inflight lên throughput. TLS, QUIC hoặc tự viết TCP không phải phần cơ sở của task này.

**Phải học chéo:** từ storage cần biết khi nào writer đã dùng xong buffer; từ scheduler cần biết ID/expected length và completion được chấp nhận. Đầu ra review gồm byte trace, API input/output, test và một timeline lỗi, không chỉ ảnh echo thành công.

## 3. Hướng B · Manifest, integrity và storage

**Cần biết trước:** chia nguyên/dư, long/checked, range/index, array/reference, exception, stream/file lifetime. Hash là kiểm tra dữ liệu theo kỳ vọng, không phải mã hóa.

| Đọc theo thứ tự | Đọc để hiểu gì? | Câu phải tự giải |
|---|---|---|
| Nền C# mục 1–3/6/7 | Hàm, overflow, alias, owner | Nhân int rồi gán long có luôn an toàn không? |
| [Bài 04](04-truyen-tep-va-integrity.md) mục 1–4 | Piece/block và manifest | F=19/P=8/B=4 có những range nào? |
| [Bài 04](04-truyen-tep-va-integrity.md) mục 5–7 | Ghi, hash và Verified | Receive xong khác writer xong thế nào? |
| [Bài 07](07-ownership-buffer-va-Vst.md) mục 2–5 | Queue/lease và lỗi ghi | Writer lỗi còn job trong queue thì ai dọn? |
| [Bài 08](08-peer-roi-va-resume.md) mục 6–7; [Bài 09](09-kiem-thu-va-capture.md) mục 3 | Resume và test biên | Metadata nói đủ nhưng byte 17 bị sửa thì sao? |

**Nguồn sâu:** [RandomAccess.WriteAsync](https://learn.microsoft.com/en-us/dotnet/api/system.io.randomaccess.writeasync?view=net-10.0) để hiểu ghi theo offset, completion và exception; đọc phần manifest của [v1](../thiet-ke/Giao-thuc-v1.md). API ghi tệp không nhất thiết trả count như Socket.SendAsync; phải đọc đúng hợp đồng API thực chọn.

**Bài riêng trước khi ghép:** hàm tính pieceCount/range với expected tính tay; manifest validation; tệp nhỏ ghi block đảo thứ tự rồi so byte/hash. Sau đó viết writer có queue giới hạn và contract rõ. Chưa cần peer discovery để kiểm arithmetic.

**Đào sâu:** ảnh hưởng Seek+Write chung handle, read-back hash, flush/cache policy, nguồn thay đổi sau tạo manifest, metadata atomic replacement và giới hạn durability. Chỉ tích hợp Win32File khi đã hiểu baseline managed và audit WRITE_THROUGH/offset/size.

**Phải học chéo:** scheduler không được công bố piece trước Verified; transport chuyển buffer thế nào; harness tính completion có gồm hash hay không. Đầu ra review là bảng range, test tệp rỗng/cuối lẻ/corrupt và trace ownership qua writer.

## 4. Hướng C · Tracker, registry và thời gian

**Cần biết trước:** Dictionary/key/equality, record/DTO, hàm validation, endpoint, elapsed time và snapshot. Chưa cần tự viết cả scheduler.

| Đọc theo thứ tự | Đọc để hiểu gì? | Câu phải tự giải |
|---|---|---|
| Nền C# mục 4/5; [Bài 01](01-mang-topology-va-endpoint.md) mục 2–4 | Mô hình entry và địa chỉ | Vì sao không key chỉ peerId? |
| [Bài 05](05-tracker-va-discovery.md) mục 2–4 | Register/update, TTL và clock | LastSeen=5, TTL=20, now=21/25 thì sao? |
| [Bài 05](05-tracker-va-discovery.md) mục 5–6 | Snapshot, giới hạn và stale data | ListPeers đúng mà connect fail có thể bình thường không? |
| [Bài 03](03-framing-va-giao-thuc.md) mục 5; [Bài 09](09-kiem-thu-va-capture.md) mục 4 | Control payload và fake clock | JSON hợp lệ đã bảo đảm endpoint hợp lệ chưa? |
| [Bài 08](08-peer-roi-va-resume.md) mục 1/5 | Registry khác connection state | Có cần chờ TTL khi socket đã lỗi không? |

**Nguồn sâu:** `01 Introduction.pdf` trang PDF 52–55; `03-1 IP address (updated).pdf` trang 21–43; `04-3 Socket.pdf` trang 32–42. [Stopwatch](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-10.0) đọc timestamp/elapsed và frequency, không coi mọi tick giống DateTime tick.

**Bài riêng trước khi ghép:** registry thuần bộ nhớ với clock giả, test key/update/expiry đúng biên/snapshot/bound. Tiếp theo mapping REGISTER/HEARTBEAT/LIST_PEERS theo header chung. Logic registry chạy được chưa có nghĩa server tracker đã chạy qua mạng.

**Đào sâu:** một owner tuần tự so với khóa ngắn, cleanup entry lớn, snapshot nhất quán, giới hạn phản hồi, retry khi tracker restart. Không cần database/ORM/DHT chỉ để có registry lab.

**Phải học chéo:** transport cung cấp message hợp lệ và request ID; peer nhận danh sách rồi tự connect/tra availability; scheduler không tin entry sống nghĩa có mọi piece. Đầu ra review là key/value contract, timeline TTL, test không sleep và một ca endpoint stale.

## 5. Hướng D · Scheduler, state machine và đồng thời

**Cần biết trước:** tập/bitmap, array/index, enum/state, collection ownership, Task khác thread. Nắm piece/block/Verified, biết request/completion không luôn đến cùng thứ tự giữa các connection.

| Đọc theo thứ tự | Đọc để hiểu gì? | Câu phải tự giải |
|---|---|---|
| [Bài 04](04-truyen-tep-va-integrity.md) mục 1/5; [Bài 05](05-tracker-va-discovery.md) mục 1/6 | Dữ liệu nào có thể chia sẻ, discovery biết gì | Tracker có quyết định nguồn từng block không? |
| [Bài 06](06-P2P-scheduler-va-pipeline.md) mục 2–3 | Bitmap, eligible set, rarity | Bảng A/B/C khiến sequential/rarest chọn gì? |
| [Bài 06](06-P2P-scheduler-va-pipeline.md) mục 4–6 | Pipeline, credit, state owner | Vì sao cap 8×16 KiB ở RTT 50 ms có thể giới hạn app? |
| [Bài 08](08-peer-roi-va-resume.md) mục 3–5 | Late response và tái giao | R1 timeout rồi response muộn có hoàn credit nữa không? |
| [Bài 07](07-ownership-buffer-va-Vst.md) mục 3–6; [Bài 09](09-kiem-thu-va-capture.md) mục 4 | Backpressure và kiểm thử event order | Queue bounded có đủ chặn mọi allocation không? |

**Nguồn sâu:** `02 Link Layer.pdf` trang PDF 29–48 cho ý tưởng sliding window; `04-2 Congestion control.pdf` trang 3–4 và ghi chú chuẩn hóa cho BDP/đơn vị; [Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels) để hiểu producer/consumer, capacity và full mode. Window request của app không phải cwnd TCP.

**Bài riêng trước khi ghép:** hàm chọn piece từ state/bitmap/inflight; bảng sự kiện request/timeout/late-result với expected cuối; dispatcher giả có credit. Chỉ sau khi giải được trace mới ghép nhiều connection và writer.

**Đào sâu:** chi phí quét availability so với duy trì rarity cache; event order và determinism; actor/state owner so với nhiều khóa; fairness nguồn chậm; request ID lifetime; lý do endgame tải trùng cần contract riêng. Không thêm thuật toán chỉ vì tên nghe mạnh; so policy trong cùng limits.

**Phải học chéo:** writer xác nhận gì trước Verified; buffer đang dùng dù request đã terminal; transport phân biệt response trên connection nào. Đầu ra review là quyết định từ bảng nhỏ, state/credit trace và test không double completion; sau đó mới có evidence B→C qua mạng.

## 6. Hướng E · Kiểm thử, CLI, harness và thực nghiệm

**Cần biết trước:** command/exit code, file/JSON/CSV, exception/async, process khác Task, đơn vị byte/bit/thời gian, trung bình và biến thiên. Đây là mảng kỹ thuật, không phải chỉ viết báo cáo hộ nhóm.

| Đọc theo thứ tự | Đọc để hiểu gì? | Câu phải tự giải |
|---|---|---|
| [Bài 00](00-mo-du-an-va-CSharp.md) mục 3–5; nền C# mục 7–9 | Vòng đời process, lỗi và cancel | In failed nhưng exit 0 ảnh hưởng runner thế nào? |
| [Bài 09](09-kiem-thu-va-capture.md) mục 1–5 | Oracle, lớp test và fake dependency | Encode/decode cùng sai vẫn pass ra sao? |
| [Bài 09](09-kiem-thu-va-capture.md) mục 6–8; [Bài 01](01-mang-topology-va-endpoint.md) mục 5 | Nối log và capture | Packet count khác app frame count thế nào? |
| [Bài 10](10-thuc-nghiem-va-phan-tich.md) mục 1–6 | Đối chứng, metric, status và denominator | Completion 2/4 khác 2/3 trong departure thế nào? |
| [Bài 10](10-thuc-nghiem-va-phan-tich.md) mục 7–9; [Bài 11](11-bao-cao-demo-va-bao-ve.md) mục 1/5/6 | Runner/metadata/plot và lập luận | Một hình có thể truy tới raw/config/revision không? |

**Nguồn sâu:** [Process](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-10.0) để hiểu start, output/error và exit; [Wireshark user guide](https://www.wireshark.org/docs/wsug_html_chunked/ChapterWork.html) để đọc display filter/Follow Stream. Dùng kiến thức TCP của [Bài 02](02-socket-TCP-va-async.md) và [Bài 03](03-framing-va-giao-thuc.md) để biết mình đang đo gì.

**Bài riêng trước khi ghép:** một bảng test input/expected độc lập; runner chạy một process nhỏ, lấy exit code và đọc stdout/stderr; schema một run có completed/failed/timeout/departed-as-planned. Với child process redirect output, cần xử lý pipe/log trong lúc chạy, tránh chỉ WaitForExit rồi mới đọc khiến child kẹt khi pipe đầy.

Tiếp theo harness nhiều process có READY/GO, deadline, log riêng, lỗi khởi động và cleanup đúng các process của run. Không kill theo tên chung làm ảnh hưởng process ngoài run. Giới hạn dữ liệu log giữ trong RAM; không giải quyết deadlock pipe bằng cách tích toàn bộ output vô hạn.

**Đào sâu:** xác định start/end qua nhiều máy, overhead đo, cache/warm-up, đảo thứ tự cặp, run lỗi/censored data, standard deviation và ý nghĩa error bar. Phần thống kê không thay correctness; phải phát hiện output hash sai trước khi cộng vào successful goodput.

**Phải học chéo:** hiểu một request/block end-to-end để chọn oracle và metric; biết writer completion có gồm hash hay chưa; biết scheduler dùng random seed vào đâu. Đầu ra review là test có thể fail, lệnh tái lập, schema/log thật và phép tính metric độc lập; không phải CSV số minh họa đóng vai kết quả.

## 7. Nếu muốn học riêng VstHelper thì đi đường nào?

Vst không là bài nhập môn C#. Trình tự kiến thức: nền C# mục 3/6/7 → [Bài 02](02-socket-TCP-va-async.md) mục 7 → [Bài 07](07-ownership-buffer-va-Vst.md) toàn bộ → audit snapshot → một adapter nhỏ → kiểm thử lifetime → thí nghiệm có đối chứng.

Cần trả lời trước: view khác owner ở đâu; copy struct pointer có tạo owner mới không; byte[] được GC quản lý vì sao vẫn sai khi Return pool sớm; native allocation nào cần free; API nào dùng Windows; allocation 4097 rơi vào nhánh nào; block 16 KiB có thực sự dùng bin nhỏ không?

Ví dụ bài riêng vừa sức: vẽ owner timeline và làm managed lease trước, sau đó kiểm một tập API nhỏ đã audit với pattern byte và size biên. Không yêu cầu sửa mọi collection/allocator hay port toàn thư viện. Nếu chỉ học I/O native, vẫn cần hiểu offset/lifetime/flush policy và giữ baseline managed để so.

## 8. Linh chọn bài đọc mỗi tuần bằng cách nào?

Không bắt đầu bằng “đọc hết [Bài 00](00-mo-du-an-va-CSharp.md)–11”. Chọn một **mục tiêu hiểu** vừa đủ cho công việc, xem prerequisite rồi chọn bài tập tương ứng. Có thể giao lý thuyết và ví dụ trước, đến khi giải thích được mới nâng lên implementation.

Ví dụ cho một tuần học, chưa phân tên:

| Mục tiêu Linh chọn | Phần đọc | Đầu ra học tập trước code |
|---|---|---|
| Phân biệt endpoint trên hai máy | [Bài 00](00-mo-du-an-va-CSharp.md) mục 3, [Bài 01](01-mang-topology-va-endpoint.md) mục 2–4 | Vẽ topology và giải thích IP/port/loopback |
| Hiểu vì sao phải có framing | [Bài 02](02-socket-TCP-va-async.md) mục 1/3, [Bài 03](03-framing-va-giao-thuc.md) mục 1–4 | Trace một stream qua hai cách chia khác nhau |
| Hiểu registry hết hạn | Nền C# mục 4/5, [Bài 05](05-tracker-va-discovery.md) mục 2–4 | Giải timeline TTL đúng biên và định nghĩa key |
| Hiểu offset tệp | Nền C# mục 1/2, [Bài 04](04-truyen-tep-va-integrity.md) mục 1–3 | Bảng F=19/P=8/B=4 không vượt cuối |
| Hiểu sở hữu buffer qua async | Nền C# mục 3/6/8/9, [Bài 07](07-ownership-buffer-va-Vst.md) mục 2–4 | Chỉ ra owner và thời điểm Return hợp lệ |

Định dạng giao việc gọn trong cuộc trao đổi/task đang dùng: **người do Linh chọn — mục tiêu hiểu — mục đọc — bài nhỏ — cách review**. Không cần thêm file theo tuần hoặc theo tên. Các mốc 8 tuần ở [Kế hoạch](../Ke-hoach.md) là tham khảo để cân khối lượng, không tự thay quyết định phân công của Linh.

Khi nhờ assistant dạy, có thể nói: “Dạy từng bước mục X; kiểm tra cách tôi hiểu trước, cho ví dụ khác rồi mới giao bài.” Assistant giải thích và review theo yêu cầu; không mặc định viết trọn tính năng hoặc ép nhảy sang code khi nền còn thiếu.
