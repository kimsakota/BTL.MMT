# Cách đóng góp cho ET4230 Đề 14

Nhóm: Linh, Minh, Cường, Quân, Tú. Vai trò trong [Kế hoạch](docs/Ke-hoach.md) cần từng người nhận. Repo dùng chung là [kimsakota/BTL.MMT](https://github.com/kimsakota/BTL.MMT), nhánh chính `main`; tài liệu này không chứng minh ai đã được mời hoặc đã làm PR.

## Một thay đổi nhỏ có thể review

Chọn task ID từ [backlog](docs/Ke-hoach.md), ghi mục tiêu và tiêu chí. Tạo nhánh theo tên không dấu, ví dụ `linh/n02-frame-codec`, `minh/f01-manifest`, `cuong/d01-tracker`, `quan/s01-scheduler`, `tu/n03-test-runner`.

Trước code, nêu API, caller, dữ liệu vào/ra, ownership/lifetime, giới hạn và lỗi. Tác giả tự viết theo bài học; reviewer/assistant giải thích và đọc diff. Nếu muốn assistant làm trọn một tính năng, yêu cầu rõ phạm vi để giữ mục tiêu học.

Chạy build/test liên quan và ghi phần chưa chạy. Tài liệu kiểm tra link/nội dung; network cần integration; allocator cần kiểm tra runtime/lifetime. Không ghi build thành test hoặc test thành benchmark.

Tác giả tự commit/push, mở PR bằng template, gắn task. Một người khác tác giả review; mọi thay đổi wire protocol hoặc contract cần người dùng API xác nhận. Sau merge cập nhật task bằng cùng ID. Assistant không tự stage/commit/push khi chỉ đang hướng dẫn.

## Dùng GitHub Desktop

Repo trên máy Linh đã có Git local và origin: chọn **File → Add local repository** và trỏ tới thư mục BTL. Thành viên khác dùng **File → Clone repository → URL** với `https://github.com/kimsakota/BTL.MMT.git`. Chủ repo cấp quyền khi nhóm thống nhất; không dùng thư mục Git của môn khác.

Trước khi sửa, Fetch/Pull nhánh gốc rồi chọn **Current branch → New branch** cho task. Trong **Changes**, đọc diff, chọn đúng file, ghi commit theo thay đổi thực tế; sau đó **Push origin → Create Pull Request**. PR nêu mục đích, cách kiểm tra, kết quả và phần còn thiếu để người review chạy lại được. Tránh đưa bin/obj/tmp, log cá nhân hoặc dữ liệu lớn vào commit. Không cần tạo PR chỉ để điền hồ sơ thành viên.

## Source tham chiếu

Không sửa `references/VstHelper` để làm warning biến mất. Đề xuất sửa upstream/biến thể vào adapter/source riêng và giải thích provenance. Không copy `.git`, bin/obj, `.vs` hoặc file cá nhân của repo môn khác.

## Tiêu chí Done

Đúng đầu ra của task, có test/evidence thật tương ứng, reviewer đọc được failure path, tài liệu/contract cập nhật, người viết giải thích được dữ liệu và cách chạy. Chú thích hữu ích được khuyến khích nhưng không là thủ tục để trì hoãn một mốc đã đạt.
