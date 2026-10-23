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
| Git | Publish `kimsakota/BTL.MMT` | Commit đầu `4c072d6` trên main, theo dõi origin/main; SHA local/remote trùng |

Evidence: [bootstrap-verification.json](../../results/setup/bootstrap-verification.json), [build Release log](../../results/setup/build-release.log), [project-validation.json](../../results/setup/project-validation.json). Thời gian trong JSON là UTC; ngày báo cáo theo Asia/Saigon là 23/10/2026.

## Cách đọc cảnh báo Vst

10 cảnh báo duy nhất CS8500 liên quan pointer/sizeof của generic có thể chứa tham chiếu managed và KeyEntryPath chứa field interface. Source giữ nguyên để truy nguyên; xem [audit](../thiet-ke/Tai-su-dung-VstHelper.md). Chưa chạy allocator, stress, leak test hay phép đo chứng minh hiệu năng.

Core chưa có implementation mạng. Chưa có tracker, peer, file transfer, codec test, demo hai máy, resume, adapter Vst hoặc benchmark. Các JSON/vector là thiết kế; CSV kết quả hiện chỉ có header. Bộ khởi tạo không ghi các công việc đó thành đã hoàn thành.

## Xuất bản GitHub

Theo yêu cầu Linh ngày 23/10/2026, bộ khởi tạo đã được đưa lên [kimsakota/BTL.MMT](https://github.com/kimsakota/BTL.MMT), commit đầu `4c072d6` (`first commit`, 108 file). [Workflow lần đầu](https://github.com/kimsakota/BTL.MMT/actions/runs/39954201976) completed/success: kiểm tra tài liệu/config/vector/hash snapshot, build Release, CLI help và version. [Evidence CI](../../results/setup/github-first-ci.json); log local trước push ở [validation](../../results/setup/github-publication-validation.json) và [build](../../results/setup/github-publication-build.log).

Các log khởi tạo trước đó giữ nguyên để truy lịch sử. Máy các thành viên khác chưa được kiểm tra; từng người làm P00 bằng evidence của máy mình. CI bootstrap chưa thay kiểm thử codec, mạng, lifetime hoặc thực nghiệm P2P.
