# Trạng thái hiện tại ET4230

Cập nhật 23/10/2026. Đây là bộ khởi tạo dự án Đề 14; chưa có application P2P.

## Đã xác nhận

- Linh yêu cầu đọc slide và dựng dự án cặn kẽ A–Z. Điều chỉnh mới nhất: tổ chức docs riêng cho P2P; không sao chép mọi quy trình môn Hệ điều hành.
- Nhóm gồm Linh, Minh, Cường, Quân, Tú. Chưa có đầy đủ họ tên/MSSV/username hoặc việc nhận nhiệm vụ.
- Linh xác nhận C# được phép dùng. Chọn C#/.NET 10; không hỏi lại do danh sách ngôn ngữ ban đầu của DOCX.
- Đã lập chỉ mục 14 PDF, 697 trang; có hash, bản đồ kiến thức và ghi chú chuẩn hóa; đã mở 6 trang render trọng yếu.
- Đã soạn yêu cầu, kiến trúc/giao thức v1, audit Vst, kế hoạch test/thí nghiệm, 12 bài học, kế hoạch/phân công, hướng dẫn báo cáo/demo, workspace và bộ nhớ.
- Docs đã gộp từ 42 xuống 30 file, tra qua [mục lục](../docs/README.md): học tập, thiết kế, triển khai, kiểm chứng, tham khảo. Đã bỏ onboarding theo người và mẫu tiến độ trùng task; lộ trình/bài học, phân công/backlog, báo cáo/demo và hai bản khảo sát cũ đã gộp theo trách nhiệm nội dung.
- Snapshot gồm 20 file C# + csproj gốc, HEAD 7236e72..., working tree dirty; giữ byte có SHA-256. Chưa sửa source gốc.
- Máy Linh có SDK 10.0.301, runtime 10.0.9 Windows x64. Build wrapper đầu thành công, 10 cảnh báo CS8500; không phải kiểm chứng runtime Vst.

## Giới hạn hiện hành

Core chưa có implementation; CLI chỉ help/version/command lỗi. Chưa có tracker, peer, file transfer, application unit tests, test hai máy, resume, adapter Vst hoặc benchmark. Phân công/backlog là đề xuất, chưa gán đóng góp.

Kiểm tra bootstrap trước đợt sắp xếp docs: Release build thành công với 10 cảnh báo CS8500; help/version exit 0, command không hỗ trợ exit 2. Báo cáo ở `results/setup/project-validation.json`; evidence xem [Kiểm tra bộ khởi tạo](../docs/trien-khai/Kiem-tra-bo-khoi-tao.md).

Sau sắp xếp docs ngày 23/10/2026: checker với `--verify-sources` đạt, không có link hỏng; 21 snapshot file và 14 PDF đúng hash. Báo cáo [docs-reorganization.json](../results/setup/docs-reorganization.json). Đã cập nhật đường dẫn trong checker, công cụ trích slide và dòng hướng dẫn CLI. Build riêng CLI Debug exit 0, không warning/error; help exit 0 và trỏ về README. Log [build](../results/setup/docs-reorganization-build.log), [help](../results/setup/docs-reorganization-help.log). Lần build này không bao gồm wrapper Vst; không phải test P2P hoặc benchmark.

Linh đã yêu cầu commit/push bộ khởi tạo ngày 23/10/2026. Đã xuất bản 108 file bằng commit đầu `4c072d6` (`first commit`) lên [kimsakota/BTL.MMT](https://github.com/kimsakota/BTL.MMT), repository public, default branch main. Origin là `https://github.com/kimsakota/BTL.MMT.git`, local main theo dõi origin/main; đã đối chiếu SHA local và remote sau push.

[GitHub Actions lần đầu](https://github.com/kimsakota/BTL.MMT/actions/runs/39954201976) đã completed/success trên `4c072d6`: checker, build Release toàn solution, CLI help/version đều đạt. [Evidence CI](../results/setup/github-first-ci.json); kiểm tra local trước xuất bản ở `results/setup/github-publication-validation.json` và `github-publication-build.log`. Đây vẫn là kiểm tra bootstrap, chưa phải test mạng/P2P. Chưa có xác nhận invitation/board hoặc bài nộp. Không nhập kết quả xv6, thông tin nhóm hoặc quy tắc báo cáo tuần của môn khác.

Rà cấu trúc ngày 23/10/2026: khung Core/Cli/VstReference phù hợp để bắt đầu; Core vẫn chưa có module implementation, tests chưa có test project. Đã sửa `.gitignore` từ `VstHelper/` thành `/VstHelper/` vì quy tắc cũ vô tình bỏ qua cả snapshot tham chiếu. Đã kiểm tra 23 file snapshot/metadata hiện được Git nhìn thấy và 5 đường dẫn build/cache/repo gốc giả định vẫn bị ignore; [evidence](../results/setup/git-ignore-verification.json). Source snapshot không đổi.
