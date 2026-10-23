# Kế hoạch triển khai và phân công

Toàn bộ owner là **đề xuất** cho đến khi nhận việc. Hiện chỉ bộ khởi tạo được assistant chuẩn bị theo yêu cầu Linh; mọi task implementation dưới đây ở trạng thái Planned. Gate là mốc đầu ra, không phải tuần lịch.

## Phần việc của nhóm

Nhóm đã xác nhận gồm **Linh, Minh, Cường, Quân, Tú**. Họ tên đầy đủ, MSSV, tài khoản và quỹ giờ chưa được cung cấp; dữ liệu nhóm nằm ở [team.json](../configs/team.json).

| Thành viên | Phần kỹ thuật đề xuất | Review chéo đề xuất | Đầu ra chính |
|---|---|---|---|
| Linh | Codec/transport, hợp đồng API và tích hợp buffer | Minh | Partial send/receive, connection lifecycle, ownership và test |
| Minh | Manifest, storage, integrity, resume | Cường | Tệp đầu/cuối/rỗng đúng, writer và hash có test |
| Cường | Tracker, TTL, heartbeat, discovery và metadata | Quân | Registry đúng, endpoint truy cập được, test stale peers |
| Quân | Scheduler, pipeline, chọn piece, peer rời mạng | Linh | State machine, kết quả đến muộn và so sánh policy |
| Tú | CLI/config, harness, logging và thực nghiệm | Cường/Minh | Chạy thật, thu dữ liệu thô, exit code và biểu đồ tái lập |

Mỗi người có phần kỹ thuật và tự viết phần báo cáo tương ứng; reviewer có thể thay theo việc thực tế. Chỉ ghi nhận công việc sau khi có kết quả và minh chứng. Không cần hồ sơ học hoặc checklist riêng theo từng tên.

## Các mốc và đầu việc

Mốc G0–G7 được giải thích ở [lộ trình học và thực hành](hoc-tap/README.md). Khi giao việc, điền owner đã nhận, hạn và evidence vào issue hoặc dòng task hiện dùng. Hướng dẫn nhánh/PR nằm ở [CONTRIBUTING](../CONTRIBUTING.md).

| ID | Gate | Owner đề xuất | Công việc | Phụ thuộc | Tiêu chí hoàn tất |
|---|---|---|---|---|---|
| P00 | G0 | Cả 5 | Mở solution, build, đọc Lesson 00–01 | Bộ khởi tạo | Từng người có log máy mình và sơ đồ layering tự giải thích |
| P01 | G0 | Linh + cả nhóm | Review scope, header, manifest, ownership | P00 | Quyết định nhóm ghi vào docs, phân công được nhận |
| P02 | G0 | Tú + cả nhóm | Thiết lập repo và quyền truy cập | Repo do Linh tạo/chia sẻ | Thành viên truy cập/clone được, có quyền cần thiết để cộng tác |
| N01 | G1 | Linh | Echo TCP nhỏ, vòng đời socket | P00 | Send/receive/EOF giải thích được, hai process và log |
| N02 | G1 | Linh | Header/codec big-endian | P01 | T01–T03, mọi byte split/ghép/EOF và vector đúng |
| N03 | G1 | Tú | Test project và runner entry | P01 | Chạy test thật của N02, fail trả nonzero |
| F01 | G2 | Minh | Manifest và piece/block arithmetic | P01 | Empty/tail/hash, int64/overflow cases đúng |
| F02 | G2 | Minh | Writer và integrity | F01, N02 | T04/T08/T09, Verified chỉ sau write+hash |
| N04 | G2 | Linh | Transfer TCP một nguồn | N01, N02, F02 | Tệp nhiều cỡ đúng hash, partial/EOF/error đúng |
| H01 | G2 | Tú | CLI nhận config và mode CS | N04 | Command và exit code thật; baseline chạy lại được |
| D01 | G3 | Cường | Tracker registry/TTL | P01, N02 | Register lại, heartbeat, hết hạn, bounded registry |
| D02 | G3 | Cường | Discovery client và endpoint | D01 | Lấy danh sách, loại chính mình, stale peer không treo vô hạn |
| N05 | G3 | Linh + Tú | Lab hai máy và capture | N04, D02 | IP/port/SDK/log/pcap/hash có bằng chứng |
| S01 | G4 | Quân | State machine/pipeline sequential | N04, D02 | Request/credit/generation rõ, bounded inflight |
| S02 | G4 | Quân + Minh | Upload lại Verified piece | S01, F02 | 3+ peer trao đổi trực tiếp và nhận đủ tệp |
| S03 | G4 | Quân | Rarest-first có tie-break seed | S02 | Test rarity/tie/no-source, cùng limit với sequential |
| S04 | G5 | Quân + Linh | Timeout, peer rời, kết quả muộn | S02 | T07/T10, không double completion/ghi trùng |
| B01 | G5 | Linh | Lease và budget managed | N04 | Cancel/queue đầy/return đúng một lần, T12 |
| F03 | G5 | Minh | Resume có rehash | F02, S04 | T11, metadata/tệp lệch không tạo HAVE giả |
| V01 | G5 | Linh + Minh | Review tập API Vst và adapter nhỏ | B01, audit | CS8500 được khoanh phạm vi, T13, Windows guard |
| V02 | G6 | Linh | Pool block lớn nếu chọn mở rộng | V01 | Bound/OOM/lifetime/biên size có test, baseline hợp lý |
| H02 | G5 | Tú | Harness nhiều process + fault schedule | H01, S04 | READY/GO, timeout, log từng process, cleanup có evidence |
| E01 | G6 | Tú + cả nhóm | Calibration, chốt ma trận | H02, correctness đạt | Config/version/máy/units cố định, không có số giả |
| E02 | G6 | Tú + Quân | Đo CS/P2P và scheduler | E01, S03 | S1–S3, raw data và số lần lặp theo kế hoạch |
| E03 | G6 | Tú + Linh | So buffer strategies | E01, V01 | Cùng policy/I/O/limits, RAM/CPU/GC/throughput |
| R01 | G7 | Cả nhóm | Report/slide/phần cá nhân | E02 | R01–R09 có evidence và nguồn |
| R02 | G7 | Cả nhóm | Demo 3–5 phút và diễn tập | R01 | Hai máy hoặc bằng chứng được duyệt, giải thích được lỗi/giới hạn |
| R03 | G7 | Tú + Linh | Kiểm tra gói nộp | R01, R02 | Clone/chạy lại, đúng tên kỳ/nhóm, không build/cache lớn |

## Các quyết định cần chốt ở P01

Phạm vi resume/Vst có bắt buộc trong đăng ký hay là mở rộng; thông số mặc định; API event/lease; rule timeout và retry; policy lưu trạng thái; bộ framework test; họ tên/MSSV và số nhóm thực tế. Chưa có deadline nên nhóm cần gắn hạn cho các task đầu sau khi biết lịch và quỹ giờ.

## Task đầu mỗi người

Mỗi người làm P00, đọc hợp đồng P01. Sau đó: Linh N01/N02; Minh F01; Cường thiết kế D01 với input giả; Quân vẽ state S01 với chuỗi sự kiện; Tú chuẩn bị N03 và thiết kế H01. Thiết kế/test data có thể làm trước implementation phụ thuộc; chưa ghép code bằng API phỏng đoán riêng.

Các vấn đề audit Vst nằm ở [tài liệu tái sử dụng](thiet-ke/Tai-su-dung-VstHelper.md); ID V01 trong bảng là task tích hợp, các nhãn V01–V07 trong audit là mục phát hiện và phải ghi rõ tiền tố khi tạo issue để tránh nhầm.

## Cách phối hợp

Trước khi tách nhánh, thống nhất header, manifest và rule Verified. Transport chuyển lease theo hợp đồng; scheduler sở hữu request/state; writer báo completion và không tự chọn nguồn. Thay đổi API hoặc wire format cần cập nhật tài liệu/test và trao đổi với người dùng API.

Một task hoàn tất khi đạt tiêu chí trong bảng và có evidence tương ứng: thiết kế, diff, test, log hoặc dữ liệu đo. Chưa kiểm tra thì ghi phần còn thiếu; khi chuyển việc giữ cùng ID để truy nguyên. Nhóm cập nhật task đang làm thay vì duy trì thêm một mẫu báo cáo tiến độ trùng nội dung.
