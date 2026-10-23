# Kiểm thử và vector thiết kế

Hiện có [vector giao thức](protocol-vectors.json) để kiểm tra byte layout của thiết kế. Chưa có test project hoặc test ứng dụng được chạy. [Ma trận](../docs/kiem-chung/Ke-hoach-kiem-thu.md) định nghĩa T01–T15; tác giả viết test cùng codec/manifest/state khi triển khai.

Không gọi `tools/check_project.py` là bộ test P2P: công cụ đó chỉ kiểm tra cấu trúc, link, JSON, hash và tính nhất quán vector. Build wrapper Vst cũng không chứng minh an toàn runtime.
