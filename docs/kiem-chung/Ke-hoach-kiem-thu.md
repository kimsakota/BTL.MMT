# Kế hoạch kiểm thử theo invariant

Hiện chưa có kiểm thử ứng dụng hay truyền mạng. Test dưới đây là tiêu chí để nhóm tự viết cùng tính năng; vector trong tests là input kỳ vọng, không phải bằng chứng test pass.

| ID | Phần | Ca kiểm tra | Điều phải chứng minh |
|---|---|---|---|
| T01 | Codec | Header chia tại mọi byte, payload phân mảnh, hai frame ghép | Đọc đúng thông điệp bất kể receive boundary |
| T02 | Codec | Sai magic/version/flag/type, length quá lớn/overflow, EOF | Từ chối trước cấp phát lớn, kết nối và lease được dọn |
| T03 | Endian | Vector HAVE/REQUEST_BLOCK | Byte đúng theo hợp đồng, không phụ thuộc endian host |
| T04 | Tệp | 0 byte, 1 byte, nhỏ hơn block, không chia hết block/piece | Dữ liệu và SHA-256 khớp, không đọc/ghi vượt cuối |
| T05 | Transport | Gửi thiếu, receive=0, timeout, cancel lúc gửi/nhận | Không báo hoàn tất giả, không trả buffer còn đang dùng |
| T06 | Discovery | Register lại, heartbeat, TTL hết, lọc chính mình | Không trùng peer theo key; cache stale được xử lý |
| T07 | Scheduler | Nguồn chậm, nguồn rời, block lặp, kết quả đến muộn | Mỗi completion tính một lần, giao lại có generation/retry budget |
| T08 | Piece | Dữ liệu ứng dụng cố ý sai hash | Không HAVE/upload piece sai; tải lại đúng vùng/state |
| T09 | Storage | Nhiều block hoàn tất đảo thứ tự, writer chậm | Đúng offset, queue có giới hạn, Verified sau ghi/hash |
| T10 | Peer | Cùng nhận và upload từ 3+ peer | Chỉ chia sẻ piece Verified, không deadlock, đủ tệp |
| T11 | Resume | Metadata/tệp lệch, bị dừng trước flush | Rehash và tải lại phần thiếu/sai, không tin bitmap mù |
| T12 | Budget | Pool hết, queue đầy, lặp connect/disconnect | Hạn chế tài nguyên có hiệu lực; live lease trở về mức giải thích được |
| T13 | Vst | Biên 4096/4097/8192, pattern, concurrent rent/return | Đúng nhánh và dữ liệu, chỉ dùng API đã review |
| T14 | Hệ thống | Hai máy, IP/port thật, một peer bị ngắt | Log + pcap + hash và trạng thái thống nhất |
| T15 | CLI | Config lỗi, command chưa triển khai, đường dẫn có dấu cách | Exit code rõ, không ghi đè tệp ngoài vùng output đã chọn |

## Thứ tự kiểm tra

Codec trong memory stream với các cách phân mảnh cố định → loopback hai process → nhiều peer local → hai máy → fault injection → thực nghiệm hiệu năng. Không cần mô phỏng packet loss bằng cách cố tình cắt/ghép dữ liệu sai trong codec; TCP segmentation không làm thay đổi nội dung stream.

Loss của mạng thường được TCP xử lý và làm chậm; test hash sai cần sửa dữ liệu ở mức ứng dụng/tệp có chủ đích, gắn nhãn fault injection. Không dựng số liệu “corrupted packet” từ việc ứng dụng nhận ít byte hơn yêu cầu.

## Bộ bằng chứng mỗi lần chạy

Lưu task/test IDs, revision + dirty diff nếu có, OS/SDK, config, lệnh chính xác, exit code, thời gian, log thô, input/output hash. Ca nhiều process phải thu log từng process và tình trạng kill/cleanup. Khi test phát hiện lỗi, giữ input tối thiểu và thêm regression cùng bản sửa.

Unit/integration test project sẽ được thêm khi bắt đầu codec. Không điền test trống luôn pass hoặc ghi dotnet build thành dotnet test. Bootstrap hiện được kiểm tra bằng build, CLI help/version/exit code và công cụ audit repo; test mạng còn Planned.
