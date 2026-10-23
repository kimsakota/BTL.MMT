# Học và thực hành Đề 14

Học cơ chế cần dùng, tự mô tả input/output và ownership, viết một bước nhỏ rồi kiểm tra trước khi ghép hệ thống. Các bài đã được soạn; chưa có xác nhận thành viên đã học hoặc làm xong. Chọn bài theo nền tảng và task đang làm, không cần lập hồ sơ học theo từng người.

## Đọc theo chặng

| Mốc | Bài liên quan | Đầu ra |
|---|---|---|
| G0 Khởi động | 00–01 | Mở/build được, hiểu endpoint và đường đi dữ liệu |
| G1 Dòng byte | 02–03 | Echo nhỏ, codec và test split/ghép/EOF |
| G2 Tệp một nguồn | 04 | Manifest, truyền tệp và hash đúng; baseline client–server |
| G3 Discovery | 05 | Tracker TTL/heartbeat, peer tìm được nhau, lab hai máy |
| G4 P2P | 06 | Tải nhiều nguồn, upload Verified piece, so chính sách chọn |
| G5 Lỗi và tài nguyên | 07–08 | Queue/buffer có giới hạn, xử lý peer rời; resume theo phạm vi |
| G6 Thực nghiệm | 09–10 | Test đúng trước, đo có kiểm soát, dữ liệu/biểu đồ tái lập |
| G7 Bàn giao | 11 | Báo cáo, demo, README chạy lại và bảo vệ cá nhân |

[Kiến thức từ slide](Kien-thuc-tu-slide.md) giúp tra phần môn học; [ghi chú chuẩn hóa](Ghi-chu-chuan-hoa-slide.md) giải thích các chỗ cần đọc kỹ. Task cụ thể nằm trong [Kế hoạch](../Ke-hoach.md). Vst adapter được tích hợp sau khi lõi truyền tệp và lifetime đã được kiểm chứng.

## Các bài học

| Bài | Nội dung | Đầu ra tự làm |
|---|---|---|
| [00](00-mo-du-an-va-CSharp.md) | Solution, project, build/run, C# managed/unmanaged | Build/help và giải thích reference |
| [01](01-mang-topology-va-endpoint.md) | Phân lớp, IP/port, topology, mô hình P2P | Vẽ một request qua các lớp và bảng hai máy |
| [02](02-socket-TCP-va-async.md) | Socket, stream, async, EOF/cancel | Echo nhỏ và bảng vòng đời |
| [03](03-framing-va-giao-thuc.md) | Header, endian, đọc/ghi đủ | Codec và test split/coalescing |
| [04](04-truyen-tep-va-integrity.md) | Piece/block, offset, SHA-256, writer | Tệp một nguồn với hash đúng |
| [05](05-tracker-va-discovery.md) | Registry, TTL, heartbeat, endpoint | Tracker/discovery có test |
| [06](06-P2P-scheduler-va-pipeline.md) | Availability, sequential/rarest-first, pipeline | Nhiều nguồn và upload Verified piece |
| [07](07-ownership-buffer-va-Vst.md) | Lease, queue có giới hạn, Vst audit | Budget/ownership test và thiết kế adapter |
| [08](08-peer-roi-va-resume.md) | Timeout, kết quả muộn, resume | Failover đúng và recovery có rehash |
| [09](09-kiem-thu-va-capture.md) | Unit/integration/fault/capture | Test matrix và bằng chứng hai máy |
| [10](10-thuc-nghiem-va-phan-tich.md) | Biến, baseline, metrics, thống kê | Raw data, plot tái lập và giới hạn |
| [11](11-bao-cao-demo-va-bao-ve.md) | Báo cáo, demo và bảo vệ cá nhân | Gói nộp có evidence đủ 5 người |

## Mẫu gửi bài để review

Ghi tên bài/bước, điều mình hiểu, input/output và invariant; gửi diff hoặc đoạn nhỏ tự viết, lệnh/kết quả thật và chỗ còn vướng. Nếu chỉ đọc lý thuyết, ghi “đã đọc” thay vì “đã test”. Assistant review/gợi ý tăng dần theo yêu cầu học; người dùng có thể yêu cầu hỗ trợ implementation rõ hơn khi muốn.

Sau khi một bước đạt: đọc diff, giữ kết quả kiểm tra, cập nhật task và tự commit/push khi dùng Git. Chú thích chỉ thêm khi giúp giải thích cơ chế. Bắt đầu hiện tại là Bài 00–01; nếu đã nắm phần đó, chuyển tới bài/task phù hợp thay vì lặp lại thủ tục.
