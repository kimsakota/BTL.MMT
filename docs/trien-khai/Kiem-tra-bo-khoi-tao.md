# Kết quả kiểm tra bộ khởi tạo ngày 23/10/2026

Đây là kiểm tra nền dự án, chưa phải kiểm thử ứng dụng P2P.

## Kết quả thực tế

| Phần | Thao tác | Kết quả |
|---|---|---|
| SDK | dotnet --version | 10.0.301 trên Windows x64 |
| Solution | Build Debug lần đầu và Release cuối | Thành công; wrapper Vst có 10 CS8500, không tắt cảnh báo |
| CLI | --help | Exit 0, chỉ liệt kê chức năng bootstrap |
| CLI | --version | Exit 0, 0.1.0-bootstrap |
| CLI | Command chưa hỗ trợ | Exit 2 đúng dự kiến |
| Project audit | Link/JSON/XML/config/vector | Lượt kiểm tra đầu không có lỗi; báo cáo mới nhất bên dưới |
| Provenance | 21 snapshot file và 14 PDF nguồn | SHA-256 khớp ở lượt kiểm tra nguồn |
| Git | Khởi tạo main local | Chưa có commit, stage hoặc remote; chưa publish GitHub |

Evidence: [bootstrap-verification.json](../../results/setup/bootstrap-verification.json), [build Release log](../../results/setup/build-release.log), [project-validation.json](../../results/setup/project-validation.json). Thời gian trong JSON là UTC; ngày báo cáo theo Asia/Saigon là 23/10/2026.

## Cách đọc cảnh báo Vst

10 cảnh báo duy nhất CS8500 liên quan pointer/sizeof của generic có thể chứa tham chiếu managed và KeyEntryPath chứa field interface. Source giữ nguyên để truy nguyên; xem [audit](../thiet-ke/Tai-su-dung-VstHelper.md). Chưa chạy allocator, stress, leak test hay phép đo chứng minh hiệu năng.

Core chưa có implementation mạng. Chưa có tracker, peer, file transfer, codec test, demo hai máy, resume, adapter Vst hoặc benchmark. Các JSON/vector là thiết kế; CSV kết quả hiện chỉ có header. Bộ khởi tạo không ghi các công việc đó thành đã hoàn thành.

Workflow GitHub đã chuẩn bị nhưng chưa chạy online. Máy các thành viên khác chưa được kiểm tra; từng người làm P00 bằng evidence của máy mình.
