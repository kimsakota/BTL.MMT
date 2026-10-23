# Công cụ hỗ trợ dự án

- `check_project.py`: Python standard library, kiểm tra link/JSON/XML, team, limits, scenario, vector và snapshot hash. `--verify-sources` thêm đối chiếu các nguồn local; bỏ option này ở CI/máy không có thư mục slide gốc. Có thể ghi JSON bằng `--report <path>`.
- `extract_slides.py`: pypdf và pdftotext, trích PDF/lập chỉ mục hash+trang; có option `--slide-dir`, `--output-dir`, `--index`, `--pdftotext`. Không phải runtime dependency.
- `snapshot_vst.py`: chép nguồn chọn lọc, không sửa repo gốc; từ chối ghi đè snapshot đã có. Update source là một thay đổi được review riêng.

Các công cụ này không chạy tracker/peer, không tạo dữ liệu hiệu năng và không thay test ứng dụng. Runner/plot sẽ được Tú cùng nhóm tự viết ở H02/E01 khi có chương trình thật.
