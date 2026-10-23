# Bài 07 Ownership và buffer có giới hạn

Đọc [audit VstHelper](../thiet-ke/Tai-su-dung-VstHelper.md) trước khi dùng unsafe. Mục tiêu là vẽ được owner qua từng bước và chứng minh budget/cleanup, rồi mới thay allocator.

## Owner, consumer và lease

Pool sở hữu tài nguyên nền, còn một lease xác định ai đang chịu trách nhiệm cho lần thuê. Khi chuyển lease từ receive sang WriteJob, owner cũ không được dùng/Return. Buffer view chỉ gồm pointer/length không tự giữ vùng nhớ sống. Task đang dùng Memory phải kết thúc trước khi lease bị dispose, kể cả cancel/error.

C# GC không thu hộ bộ nhớ unmanaged theo hợp đồng Rent/Return, và một managed reference tồn tại không chứng minh pointer tự lấy bằng cast hợp lệ. Snapshot có CS8500 liên quan generic/managed type; chỉ dùng tập API đã được review, không dùng `GetHandle(object,offset)` trong transport.

## Budget và điều tiết

Tính payload đang ở receive/send, request đang chờ, writer queue, upload queue và scratch hash. Config giới hạn cả connection, inflight và global buffer count. Channel bounded tạo áp lực ngược; scheduler ngừng phát request khi không có credit. Đừng tạo Task vô hạn trước một queue bounded rồi coi đó là giới hạn toàn hệ thống.

Một actor sở hữu scheduler hoặc single writer có thể giảm nhu cầu custom lock. Lock native/Monitor không được giữ qua await; dữ liệu đã publish cho worker cần lifetime và địa chỉ ổn định. `RawList.GrowUp` có thể đổi địa chỉ.

## Bài nhỏ

1. Vẽ owner của buffer tại Rent → Receive pending → Write queued → Write done → Return.
2. Chèn lỗi/cancel tại mỗi mũi tên: ai dọn socket, request và lease? Gọi cancel rồi Return ngay sai ở đâu?
3. Tính budget từ peer.default.json; chỉ ra working set process còn có runtime/GC/metadata ngoài payload budget.
4. Viết baseline lease/bounded queue có test trước. Sau đó thiết kế adapter MemoryManager/IMemoryOwner cho một vùng unmanaged với lifetime rõ, chưa ghép cả pool Vst vào runtime.
5. Kiểm tra size 4096/4097/8192 và pattern nhiều allocation; code hiện tại đổi nhánh từ 8 KiB đã làm tròn. Block lớn cần baseline hoặc pool riêng.

Hoàn thành bài khi T12 và tập T13 được chọn có kết quả thật, live lease được giải thích ở mọi failure path. Chưa gọi adapter là nhanh hơn cho tới [thí nghiệm](10-thuc-nghiem-va-phan-tich.md). Tiếp [Bài 08](08-peer-roi-va-resume.md).
