# Kế hoạch triển khai và phân công

Linh xác nhận ngày 23/10/2026: dự án có **8 tuần làm việc, tuần hiện tại là tuần 1**. Chưa có ngày bắt đầu/kết thúc theo lịch lớp; dùng số tuần tương đối, không tự suy ra ngày nộp chính thức.

**Linh tự giao việc và bài đọc cho từng người mỗi tuần.** Assistant cung cấp học liệu, prerequisite, bài tập và backlog để Linh chọn; không gán sẵn ai làm mảng nào hoặc ép viết tính năng khi nền còn thiếu. Bộ khởi tạo đã có trên GitHub; chưa có implementation P2P. Lịch 8 tuần dưới đây là khung tham khảo chưa phải cam kết cá nhân. Gate G0–G7 là mốc kỹ thuật, không đồng nhất số tuần.

## Phần việc của nhóm

Nhóm đã xác nhận gồm **Linh, Minh, Cường, Quân, Tú**. Chưa có xác nhận phân công hàng tuần hoặc trình độ từng người. Dữ liệu nhóm ở [team.json](../configs/team.json); các proposedArea cũ chỉ là gợi ý ban đầu, không được dùng làm việc đã giao.

Năm mảng kiến thức để Linh lựa chọn là transport/giao thức, manifest/storage, tracker/discovery, scheduler/đồng thời và test/harness/thực nghiệm. [Lộ trình theo chuyên môn](hoc-tap/Lo-trinh-theo-chuyen-mon.md) nêu rõ nền cần bù, mục đọc, ví dụ và mức thực hành cho từng mảng. Mọi người học nền chung phù hợp trước; không lập hồ sơ học theo tên.

## Lịch 8 tuần

| Tuần | Trọng tâm và task | Đầu ra cuối tuần |
|---|---|---|
| **1 — hiện tại, còn T7–CN** | Nền chung + 5 bài nhỏ được đề xuất theo yêu cầu Linh; khoảng 150 phút/người tính cả trao đổi | Giải thích được mô hình và ví dụ; ghi điểm cần bù, không mặc định hoàn thành N01–N03 |
| **2** | G2: F01, F02, N04, H01 | Truyền tệp một nguồn qua TCP, đúng SHA-256 kể cả tệp rỗng/cuối lẻ; có CLI chạy lại được và baseline client–server |
| **3** | G3: D01, D02, N05; chuẩn bị ghép S01 | Tracker/discovery qua mạng, heartbeat/TTL; baseline chạy hai máy với endpoint/log/hash thật |
| **4** | G4: S01, S02; bắt đầu harness H02 | Ít nhất 3 peer tải nhiều nguồn, receiver chia sẻ lại piece Verified; log chứng minh receiver trao đổi trực tiếp với nhau |
| **5** | G5: S04, B01, H02; F03 nếu nằm trong phạm vi đăng ký | Peer rời được giao lại hữu hạn; queue/buffer có giới hạn, cleanup đúng; resume được test nếu đã cam kết |
| **6** | S03, V01 và E01; hoàn thiện F03/H02 | Rarest-first có đối chứng sequential; adapter Vst nhỏ có test trong phạm vi đã audit; harness/calibration sẵn sàng; chốt phiên bản dùng để đo |
| **7** | G6: E02, E03 | Chạy ma trận đã chốt: tối thiểu 3 kịch bản/2 tham số, lưu raw data, lặp theo kế hoạch; biểu đồ CS/P2P, scheduler và phần Vst đủ điều kiện |
| **8** | G7: R01–R03; sửa lỗi và chạy xác nhận | Hoàn thiện báo cáo/slide, demo 3–5 phút, diễn tập cả 5 người và kiểm tra gói nộp tái lập |

Phần cơ sở, thiết kế và kiểm thử của báo cáo được viết dần từ tuần 1, kết quả được bổ sung ở tuần 7; tuần 8 chủ yếu hoàn thiện và bảo vệ. Dừng thêm tính năng sau tuần 6 để ổn định thí nghiệm. Lỗi correctness/lifetime phải sửa trước khi tiếp tục đo.

Đề xuất giới hạn cho 8 tuần: làm tốt P2P và một phần tích hợp Vst có kiểm chứng; V02 xây pool block lớn chưa được xếp vào lịch. V01 phải chốt API/kích thước hợp lệ theo audit, không mặc định bin hiện có phục vụ block 16 KiB. Resume/Vst nếu đã đăng ký là bắt buộc thì giữ trong tiêu chí hoàn thành; không tự bỏ khi trễ. Nếu thiếu thời gian, thu hẹp mở rộng chưa cam kết và ma trận so sánh phụ, vẫn giữ yêu cầu môn học và điều kiện so sánh công bằng.

## Tuần 1 còn T7–CN: đề xuất để Linh giao cho 5 người

Linh cho biết chỉ còn hai ngày cuối tuần và sinh viên vẫn học các môn khác. Bảng sau là đề xuất theo yêu cầu lần này để Linh chốt giao; **chưa xác nhận ai đã nhận hoặc hoàn thành**, không cố định chuyên môn của cả 8 tuần.

**Ngân sách đề xuất: tối đa 150 phút/người cho cả T7–CN, đã tính buổi nhóm.** Không giả định có hai ngày rảnh toàn thời gian. Mục tiêu là hiểu nền chung, tự giải một ví dụ và chia sẻ cách hiểu. Implementation codec/tracker/P2P chưa là đầu ra bắt buộc của cuối tuần này.

### 1. Chia thời gian

| Lúc làm | Việc | Phút/người |
|---|---|---:|
| T7, tự chọn giờ | Mở repo/học liệu; thử build/help nếu SDK đã sẵn | 15 |
| T7, tự học | Bài 00 mục 1–3 và Bài 01 mục 1–4; ghi 2 ý đã hiểu, 1 câu còn vướng | 45 |
| CN, tự học | Đọc phần riêng và giải bài nhỏ trong bảng dưới | 45 |
| CN, nhóm tự chọn giờ | 5 người trình bày 3 phút/người, hỏi đáp 10 phút, chốt bước sau 5 phút | 30 |
| CN | Sửa chỗ hiểu sai sau trao đổi; nếu đã rõ thì kết thúc | 15 |
| **Tổng** | **60 phút T7 + 90 phút CN** | **150** |

Có thể đổi thứ tự theo lịch môn khác, giữ tổng khối lượng. Nếu cài/build vướng quá 15 phút thì ghi lỗi và chuyển sang học/bài giấy, không dành cả buổi cài công cụ. Chưa chạy được thì ghi chưa chạy, không dùng log máy khác. Linh chia sẻ đúng bản học liệu hiện tại cho nhóm; các bài mới đang ở bản local, chưa được push lên GitHub.

### 2. Phần nền chung cho cả 5

- [Bài 00](hoc-tap/00-mo-du-an-va-CSharp.md) **mục 1–3**: source/project/process, Cli → Core, hai process không tự dùng chung dữ liệu.
- [Bài 01](hoc-tap/01-mang-topology-va-endpoint.md) **mục 1–4**: IP/port, loopback/bind/advertise và vai trò tracker/seed/receiver trong P2P.

Không đọc hết thư mục hoặc toàn bộ slide cuối tuần này. Nền C# chỉ tra đúng từ đang vướng, ví dụ hàm/reference ở mục 1/3; chưa giao học cả async/unsafe/Vst. Đọc để tự trả lời: “Hệ thống này chia sẻ tệp ra sao?”, “Tracker làm gì?”, “IP và port dùng vào việc gì?”.

### 3. Đề xuất việc riêng, mỗi người 45 phút

Dành khoảng 15 phút đọc phần được chọn, 20 phút tự giải ví dụ, 10 phút tóm tắt/câu hỏi. Đầu ra là một bảng/sơ đồ/5–10 dòng hoặc ảnh bài viết tay trong cuộc trao đổi/task hiện dùng; không cần báo cáo A4, slide hoặc file hồ sơ cá nhân.

| Người | Phần đọc riêng | Việc nhỏ cụ thể | Đầu ra để chia sẻ CN |
|---|---|---|---|
| **Linh** | [Bài 02](hoc-tap/02-socket-TCP-va-async.md) mục 1–3 | Cần nhận 16 byte; lần lượt nhận 6,3,7 byte. Tính offset/còn thiếu. Nếu EOF sau 9 byte thì kết luận gì? | Bảng 3 lần nhận và giải thích vì sao một Send không bằng một Receive |
| **Minh** | [Bài 04](hoc-tap/04-truyen-tep-va-integrity.md) mục 1–2 và 5 | Tệp 21 byte, piece 8, block 4: chia piece/block, tính offset và length phần cuối. Phân biệt nhận đủ với Verified. | Một bảng chia tệp, 2–3 câu giải thích kiểm tra dữ liệu |
| **Cường** | [Bài 05](hoc-tap/05-tracker-va-discovery.md) mục 1–3 | Peer đăng ký t=0, heartbeat t=8, TTL=20; xét t=27/28 theo age>=TTL. Giải thích cùng peer chia sẻ hai file thì key thế nào. | Timeline nhỏ và key `(fileId,peerId)`; chưa viết server |
| **Quân** | [Bài 06](hoc-tap/06-P2P-scheduler-va-pipeline.md) mục 1–3 | A có {0,1,2,3,4}, B {0,2}, C {1,2,3}; D đã có {0}. Lập rarity, chọn sequential/rarest-first; xét A rời. | Bảng 5 piece và lý do chọn; chưa làm pipeline/concurrency |
| **Tú** | [Bài 00](hoc-tap/00-mo-du-an-va-CSharp.md) mục 4–5; [Bài 09](hoc-tap/09-kiem-thu-va-capture.md) mục 1 | Lập expected output/exit cho help, version, unknown-command của CLI hiện có. Chạy nếu môi trường sẵn, còn không trace Program.cs và ghi chưa chạy. | Bảng 3 ca, tách expected/actual; chưa tạo test framework hoặc harness |

Các số trên là input bài học, không đổi config v1 hoặc tạo số đo. Phần việc có thể đổi người do Linh chọn; không suy ra nền tảng mỗi người từ tên. Ai đã biết có thể viết vài dòng để kiểm bài trong cùng 45 phút, không tăng thành yêu cầu code cho những người khác.

<details>
<summary>Gợi ý để Linh review sau khi mọi người tự làm</summary>

- **Linh:** offset 0→6→9→16, số còn thiếu 16→10→7→0. EOF lúc 9/16 là thiếu7, không báo frame hoàn chỉnh.
- **Minh:** 3 piece dài 8,8,5; block file offsets 0,4,8,12,16,20 và lengths 4,4,4,4,4,1. Nhận đủ chưa Verified cho tới khi ghi/hash đúng theo contract.
- **Cường:** lastSeen8, t27 age 19 còn hiệu lực; t28 age 20 hết hạn theo quy ước bài tập. Hai file cần hai key dù cùng peerId.
- **Quân:** rarity theo piece 0…4 là2,2,3,2,1; bỏ0 đã có. Sequential chọn1; rarest chọn4. A rời thì piece 4 không còn nguồn, không lấy rarity0 làm “hiếm nhất có thể tải”.
- **Tú:** help/version dự kiến exit 0; unknown-command exit 2. Build/help không kiểm chứng tracker, peer hoặc truyền tệp. Actual chỉ được ghi khi đã chạy thật.

</details>

### 4. Chốt cuối CN bằng hiểu bài, không bằng số trang đã đọc

Trong 30 phút chung, mỗi người nói: vấn đề mình học là gì, giải ví dụ thế nào, còn vướng gì. Người nghe hỏi lại một câu và dùng phần chia sẻ để nối toàn hệ thống; không biến thành 5 mảng kiến thức tách rời.

Cuối buổi nhóm cần có: một sơ đồ lời nói/hình vẽ tracker–seed–receiver, năm bài nhỏ đã trao đổi và danh sách điểm cần bù. Chỉ ghi hoàn thành phần đã được xem thật. Chưa giải được thì giữ câu hỏi, bù đúng mục trong 15 phút còn lại; không phải làm thêm buổi tối để đạt checklist code.

Không ép phải xong N01–N03, registry socket, scheduler, capture, benchmark hoặc tích hợp Vst trong cuối tuần này. Các backlog đó vẫn để chọn sau khi đủ nền. Đầu tuần 2 Linh dựa trên kết quả và quỹ giờ thực tế để giao bước tiếp, không mặc định mọi người đều đã sẵn sàng viết module.

Nếu một người chỉ có khoảng **90 phút**, dùng bản rút gọn: 45 phút nền chung + 25 phút bài riêng + 20 phút trao đổi bằng nhóm nhỏ/tin nhắn/voice; bỏ phần setup nếu chưa sẵn. Khi cả nhóm dùng bản này, thay buổi 30 phút bằng trao đổi 20 phút, không cộng cả hai. Mức 90 cũng là đề xuất để Linh chọn, không phải lịch đã xác nhận.

## Các mốc và đầu việc

Mốc G0–G7 được giải thích ở [lộ trình học và thực hành](hoc-tap/README.md). Linh chọn người, hạn, bài đọc và mức thực hành khi giao; bảng dưới đây không có owner mặc định. Chỉ ghi nhận nhận việc/hoàn thành khi có xác nhận và evidence. Hướng dẫn nhánh/PR nằm ở [CONTRIBUTING](../CONTRIBUTING.md).

| ID | Gate | Công việc | Phụ thuộc | Tiêu chí hoàn tất |
| --- | --- | --- | --- | --- |
| P00 | G0 | Mở solution, build, đọc Lesson 00–01 | Bộ khởi tạo | Từng người có log máy mình và sơ đồ layering tự giải thích |
| P01 | G0 | Review scope, header, manifest, ownership | P00 | Quyết định nhóm ghi vào docs, phân công được nhận |
| P02 | G0 | Clone repo đã có và kiểm tra quyền truy cập | Repo BTL.MMT đã publish | Thành viên truy cập/clone được, có quyền cần thiết để cộng tác |
| N01 | G1 | Echo TCP nhỏ, vòng đời socket | P00 | Send/receive/EOF giải thích được, hai process và log |
| N02 | G1 | Header/codec big-endian | P01 | T01–T03, mọi byte split/ghép/EOF và vector đúng |
| N03 | G1 | Test project và runner entry | P01 | Chạy test thật của N02, fail trả nonzero |
| F01 | G2 | Manifest và piece/block arithmetic | P01 | Empty/tail/hash, int64/overflow cases đúng |
| F02 | G2 | Writer và integrity | F01, N02 | T04/T08/T09, Verified chỉ sau write+hash |
| N04 | G2 | Transfer TCP một nguồn | N01, N02, F02 | Tệp nhiều cỡ đúng hash, partial/EOF/error đúng |
| H01 | G2 | CLI nhận config và mode CS | N04 | Command và exit code thật; baseline chạy lại được |
| D01 | G3 | Tracker registry/TTL | P01, N02 | Register lại, heartbeat, hết hạn, bounded registry |
| D02 | G3 | Discovery client và endpoint | D01 | Lấy danh sách, loại chính mình, stale peer không treo vô hạn |
| N05 | G3 | Lab hai máy và capture | N04, D02 | IP/port/SDK/log/pcap/hash có bằng chứng |
| S01 | G4 | State machine/pipeline sequential | N04, D02 | Request/credit/generation rõ, bounded inflight |
| S02 | G4 | Upload lại Verified piece | S01, F02 | 3+ peer trao đổi trực tiếp và nhận đủ tệp |
| S03 | G4 | Rarest-first có tie-break seed | S02 | Test rarity/tie/no-source, cùng limit với sequential |
| S04 | G5 | Timeout, peer rời, kết quả muộn | S02 | T07/T10, không double completion/ghi trùng |
| B01 | G5 | Lease và budget managed | N04 | Cancel/queue đầy/return đúng một lần, T12 |
| F03 | G5 | Resume có rehash | F02, S04 | T11, metadata/tệp lệch không tạo HAVE giả |
| V01 | G5 | Review tập API Vst và adapter nhỏ | B01, audit | CS8500 được khoanh phạm vi, T13, Windows guard |
| V02 | G6 | Pool block lớn nếu chọn mở rộng; chưa xếp lịch 8 tuần | V01 | Bound/OOM/lifetime/biên size có test, baseline hợp lý |
| H02 | G5 | Harness nhiều process + fault schedule | H01, S04 | READY/GO, timeout, log từng process, cleanup có evidence |
| E01 | G6 | Calibration, chốt ma trận | H02, correctness đạt | Config/version/máy/units cố định, không có số giả |
| E02 | G6 | Đo CS/P2P và scheduler | E01, S03 | S1–S3, raw data và số lần lặp theo kế hoạch |
| E03 | G6 | So buffer strategies | E01, V01 | Cùng policy/I/O/limits, RAM/CPU/GC/throughput |
| R01 | G7 | Report/slide/phần cá nhân | E02 | R01–R09 có evidence và nguồn |
| R02 | G7 | Demo 3–5 phút và diễn tập | R01 | Hai máy hoặc bằng chứng được duyệt, giải thích được lỗi/giới hạn |
| R03 | G7 | Kiểm tra gói nộp | R01, R02 | Clone/chạy lại, đúng tên kỳ/nhóm, không build/cache lớn |

## Các quyết định cần chốt ở P01

Phạm vi resume/Vst có bắt buộc trong đăng ký hay là mở rộng; thông số mặc định; API event/lease; rule timeout và retry; policy lưu trạng thái; bộ framework test; họ tên/MSSV và số nhóm thực tế. Đã biết quỹ 8 tuần, tuần này là tuần 1; gắn hạn tương đối theo lịch trên và chốt lịch lớp/quỹ giờ trong buổi đầu, không chờ ngày nộp chính thức mới bắt đầu.

Các vấn đề audit Vst nằm ở [tài liệu tái sử dụng](thiet-ke/Tai-su-dung-VstHelper.md); ID V01 trong bảng là task tích hợp, các nhãn V01–V07 trong audit là mục phát hiện và phải ghi rõ tiền tố khi tạo issue để tránh nhầm.

## Cách phối hợp

Trước khi tách nhánh, thống nhất header, manifest và rule Verified. Transport chuyển lease theo hợp đồng; scheduler sở hữu request/state; writer báo completion và không tự chọn nguồn. Thay đổi API hoặc wire format cần cập nhật tài liệu/test và trao đổi với người dùng API.

Một task hoàn tất khi đạt tiêu chí trong bảng và có evidence tương ứng: thiết kế, diff, test, log hoặc dữ liệu đo. Chưa kiểm tra thì ghi phần còn thiếu; khi chuyển việc giữ cùng ID để truy nguyên. Nhóm cập nhật task đang làm thay vì duy trì thêm một mẫu báo cáo tiến độ trùng nội dung.
