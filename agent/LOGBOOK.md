# Nhật ký dự án

## L001 Ngày 23/10/2026 Khởi tạo theo slide và mô hình Hệ điều hành

Nguồn trực tiếp: Linh yêu cầu nghiên cứu thư mục Slide, tạo dự án A–Z theo cách tổ chức môn Hệ điều hành, nhóm 5 người Linh/Minh/Cường/Quân/Tú. Linh xác nhận dùng C# được phép. Các nhận định trước về C++ hoặc phải xác nhận lại C# đã được cập nhật.

Đã lập chỉ mục 14 PDF/697 trang bằng pypdf + pdftotext, giữ SHA-256 và trang PDF 1-based; đọc cấu trúc học phần và đào sâu phần Intro/IP/TCP/flow/socket. Đã render/xem 6 trang sơ đồ/công thức; ghi chú window/RTT, slow start, EWMA và đóng TCP có đối chiếu RFC.

Đã tham khảo cấu trúc docs/lessons, hợp đồng module, GitHub Desktop, evidence và memory Light ở repo Hệ điều hành. Không sao chép implementation xv6, danh tính nhóm hoặc deadline/quy định báo cáo sang ET4230.

Đã tạo solution .NET 10 (Core, Cli bootstrap, VstReference), snapshot 21 source/project files giữ bytes và provenance. Build đầu thành công trên SDK 10.0.301; 10 CS8500 được giữ và audit, chưa thử allocator runtime. Chưa có chức năng P2P/test mạng/benchmark. Phân công là đề xuất, chưa phải đóng góp hoặc nhận task của sinh viên.

Đã soạn 12 bài học, thiết kế wire protocol v1, invariant/ownership, backlog, test matrix, 3 scenario/2 tham số, khung report/demo và các công cụ kiểm tra project.

## L002 Ngày 23/10/2026 Kiểm tra bootstrap

Release build exit 0, 10 warning duy nhất CS8500 từ snapshot; CLI help/version exit 0, command chưa hỗ trợ exit 2. Evidence được ghi vào results/setup/ và docs/Kiem-tra-bo-khoi-tao.md. Audit đầu có 59 Markdown/130 link local, JSON/config/vector, 21 snapshot hash và 14 PDF hash đúng; audit cuối được chạy sau các cập nhật tài liệu. Đây không là test codec/mạng hoặc benchmark.

Đã khởi tạo Git local main, chưa stage/commit/push hoặc tạo remote. Đã chuẩn bị workflow bootstrap Windows, chưa chạy GitHub CI. Bước tiếp theo là nhóm tự mở/build/học Lesson 00–01, review contract và nhận task; không ghi ai đã đọc hoặc nhận việc.

## L003 Ngày 23/10/2026 Tổ chức docs theo nhu cầu P2P

Linh phản hồi thư mục onboarding theo người không cần thiết và yêu cầu nghiên cứu, làm lại docs hợp lý, không sao chép mọi thứ từ dự án Hệ điều hành. Đã đổi quy tắc làm việc và bộ nhớ theo điều chỉnh này; vẫn giữ cách giải thích cơ chế, ownership và review khi hướng dẫn học.

Đã rà 42 file, gộp còn 30: thêm mục lục theo việc cần làm; gom vào học tập, thiết kế, triển khai, kiểm chứng và tham khảo. Lộ trình gộp với mục lục 12 bài học; phân công/quy trình gộp vào kế hoạch/backlog; dàn ý báo cáo và kịch bản demo gộp thành một tài liệu; hai bản khảo sát ban đầu được rút gọn vào cơ sở chọn đề và thiết kế hiện hành. Đã bỏ 6 file onboarding và mẫu tiến độ chưa có dữ liệu thành viên. Hướng dẫn Git gộp về CONTRIBUTING. Các hợp đồng giao thức, kiểm thử, thực nghiệm và nội dung bài học kỹ thuật được giữ.

Trước khi chuyển, đã lưu bản sao và hash ở `tmp/docs-reorganization/20261023T121530Z`; MIGRATION.json ghi đường dẫn cũ/mới. Cập nhật liên kết của docs, README phụ thuộc, AGENTS và bộ nhớ; cập nhật checker, nơi ghi chỉ mục slide và dòng hướng dẫn help. Tài liệu/log lịch sử giữ dấu vết lần chạy cũ.

Kiểm tra `tools/check_project.py --verify-sources --report results/setup/docs-reorganization.json` đạt: link local, JSON/config/vector, 21 snapshot hash và 14 PDF hash đúng. Build riêng CLI Debug và chạy help với `--no-build` đều exit 0; build có 0 warning/error. Log ở `results/setup/docs-reorganization-build.log` và `docs-reorganization-help.log`. Không chạy wrapper Vst hoặc thử chức năng mạng trong đợt này. Không stage/commit/push.

## L004 Ngày 23/10/2026 Rà cấu trúc dự án

Linh hỏi cấu trúc đã hợp lý chưa. Đã đọc project references, solution, cây source, kiến trúc, config, tests, workflow và ignore. Khung 3 project giữ hướng Cli → Core và VstReference độc lập, phù hợp giai đoạn hiện tại. Core chưa có implementation; test project cần đi cùng codec đầu tiên theo N03. Adapter Vst là phần sau, chưa được tích hợp hoặc kiểm chứng runtime. Không tạo thêm project/class rỗng trong đợt review.

Phát hiện bằng `git check-ignore -v` rằng dòng `VstHelper/` trong `.gitignore` cũng bỏ qua `references/VstHelper/`. Đã giới hạn thành `/VstHelper/`. Kiểm tra lại bằng Git với danh sách đường dẫn phân tách NUL: 21 file source/project cùng README và SNAPSHOT.json không bị ignore; 5 đường dẫn mẫu cho root checkout, bin/obj/tmp/raw vẫn bị ignore. Evidence ở `results/setup/git-ignore-verification.json`. Không sửa snapshot, stage, commit hoặc push.

## L005 Ngày 23/10/2026 Xuất bản GitHub theo yêu cầu Linh

Linh giao tạo/đưa dự án lên `https://github.com/kimsakota/BTL.MMT.git` và cung cấp các lệnh commit/push. Khi kiểm tra, origin đã đúng URL, remote tồn tại nhưng chưa có ref, local main chưa có commit. Không khởi tạo lại repo, không nối thêm dòng echo vào README; cập nhật tên BTL.MMT và hướng dẫn clone, giữ nội dung dự án.

Đã kiểm tra local: checker có đối chiếu nguồn đạt, build Release toàn solution exit 0 (incremental, 0 warning/error ở lần này; 10 CS8500 ở build sạch trước vẫn thuộc audit Vst). Kiểm tra tag của ba action trong workflow có tồn tại. Loại dòng trống thừa cuối 52 file văn bản của dự án, giữ snapshot nguyên byte; kiểm tra staged blobs của đủ 21 file theo SHA-256 và không có bin/obj/tmp. Source/tài liệu/cấu hình/evidence được stage theo yêu cầu, không chứa repository VstHelper gốc của môn khác.

Đã tạo root commit `4c072d6b557e5e6844a7bdd0be86b8c80ae328d7`, message `first commit`, 108 file; push main thành công và đặt upstream origin/main. SHA local/remote trùng. GitHub API xác nhận repository public, default branch main. Không force push, không mời thành viên hoặc tạo board.

Workflow `Project bootstrap validation`, run `39954201976`, completed/success: checker/config/vector/snapshot hash, build, help và version trên Windows đều đạt. Evidence ở `results/setup/github-first-ci.json`. Đã cập nhật bộ nhớ và hướng dẫn kiểm tra để phản ánh việc publish; kết quả này không chứng minh application P2P đã được viết hoặc benchmark đã chạy.
