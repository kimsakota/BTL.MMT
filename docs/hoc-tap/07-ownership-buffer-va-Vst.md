# Bài 07 · Ai sở hữu buffer và ai được quyền trả nó?

**Học trước:** reference/alias/Memory/Dispose/await trong nền C#, vòng đời I/O ở Bài 02, writer ở Bài 04. **Mục tiêu:** trace được một buffer qua cả thành công và lỗi; tính budget có đơn vị; biết phần nào của Vst được dùng có điều kiện.

Nguồn bổ trợ: [Memory owner/consumer guidelines](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines), [Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels). Phát hiện riêng của snapshot nằm ở [Audit Vst](../thiet-ke/Tai-su-dung-VstHelper.md); bài này không thay thế audit.

## 1. Vì sao GC chưa giải quyết hết?

Một byte[] managed được GC quản lý object, nhưng pool cho phép tái sử dụng cùng mảng trước khi object bị thu hồi. Nếu Return sớm, code khác thuê lại và ghi đè trong khi socket vẫn đọc/ghi vùng ấy. Không cần free pointer mới có lỗi lifetime.

Vùng unmanaged còn cần cơ chế giải phóng đúng theo API cấp phát. Memory/Span view không tự chứng minh vùng nền còn hợp lệ. Copy một struct chứa pointer không tạo một allocation mới; hai bản sao Dispose cùng allocation có thể sai.

Tối ưu cấp phát chỉ có giá trị sau khi dữ liệu và lifetime đúng. Một benchmark nhanh nhưng dùng buffer đã trả là kết quả không hợp lệ.

## 2. Owner, consumer, lease và view

| Khái niệm | Trách nhiệm | Ví dụ |
|---|---|---|
| Owner | Quyết định lifetime/thu hồi theo hợp đồng | Receive logic đang giữ lần thuê |
| Consumer | Được dùng dữ liệu trong thời hạn cho phép | Socket operation, writer |
| View | Mô tả vùng dữ liệu, không nhất thiết sở hữu | Memory slice, pointer+length |
| Lease | Quyền/thời gian sử dụng được quy định | Khoảng consumer được truy cập buffer |

Tên BufferLease trong thiết kế dự án có thể là object đại diện một lần thuê/ownership. Không mặc định mọi kiểu có chữ Buffer đều là owner hoặc mọi consumer đều được Dispose. Phải đọc hợp đồng cụ thể.

Pool quản lý kho vùng nhớ; owner của một lần thuê chịu trách nhiệm trả đúng một lần. Khi **transfer ownership**, owner cũ không tiếp tục đọc/ghi/Return lần thuê ấy. Nếu chỉ **mượn**, caller vẫn phải giữ vùng sống tới khi consumer hoàn tất.

## 3. Theo dấu một buffer từ receive đến đĩa

```text
Pool → Receive owner → WriteJob/queue owner → Writer owner → Pool
```

| Mốc | Ai chịu trách nhiệm? | Được Return chưa? |
|---|---|---|
| Rent thành công | Receive owner | Chưa nếu còn cần nhận |
| ReceiveAsync pending | Receive owner giữ sống; socket là consumer | Chưa |
| Đã nhận đủ, chuẩn bị enqueue | Receive owner | Chỉ nếu hủy job và không consumer nào còn dùng |
| Enqueue đã được chấp nhận theo contract | Ownership chuyển sang queue/writer | Receive owner không được trả nữa |
| WriteAsync pending | Writer giữ sống, I/O là consumer | Chưa |
| Write kết thúc hoặc lỗi đã settle | Writer thực hiện cleanup theo contract | Có, nếu không còn consumer khác |

Nếu enqueue thất bại trước transfer, caller vẫn phải dọn lần thuê. Nếu queue đã nhận nhưng consumer lỗi, cần xử lý cả các job còn nằm trong queue. Không chỉ dọn biến local của writer rồi quên các item chưa đọc.

Một state machine ownership có thể là `OwnedByReceive → Queued → OwnedByWriter → Returned`. Invariant: đúng một bên chịu trách nhiệm Return; dữ liệu không bị truy cập sau Returned.

## 4. Vì sao Cancel rồi Return ngay là sai?

Ví dụ race học tập:

```text
t0: buffer X được thuê
t1: socket bắt đầu gửi X
t2: token bị cancel
t3: X bị Return và một uploader khác thuê lại, ghi byte mới
t4: operation gửi cũ thực sự kết thúc
```

Tại t3–t4, hai việc còn có thể dùng cùng vùng dữ liệu. Sửa bằng cách quan sát/đợi operation kết thúc theo API rồi cleanup. Không dùng việc token.IsCancellationRequested=true hoặc Task không được await nữa làm bằng chứng consumer đã dừng.

Timeout request và cleanup I/O phải được phối hợp. Credit được trả cho scheduler không tự giải phóng vùng nhớ mà một write job cũ còn giữ.

## 5. Bounded queue và backpressure

Producer tạo job nhanh hơn consumer xử lý thì backlog tăng. Với tốc độ giả định nhận 20 block/s, writer ghi 10 block/s, sau 5 s chênh lệch đã là khoảng 50 block nếu bỏ qua giới hạn và thời gian khởi động. Queue vô hạn chỉ trì hoãn lúc hết RAM.

Channel bounded với policy chờ có thể làm producer chậm lại khi đầy. Nhưng nếu tạo 10000 Task, mỗi Task đã thuê buffer rồi đứng chờ ghi queue, capacity queue=64 không còn giới hạn tất cả buffer đang sống.

Dự án cần giới hạn trước khi phát thêm request/thuê bộ nhớ: connection cap, request credit, pool cap và queue capacity phối hợp. Các chế độ drop cần hợp đồng dispose item bị bỏ; không bật drop tùy tiện với item sở hữu tài nguyên. Bản cơ sở ưu tiên policy Wait và shutdown/drain được thiết kế rõ.

## 6. Tính budget từ config thật, chưa gọi đó là đo RAM

[peer.default.json](../../configs/peer.default.json) hiện đề xuất B=16384 byte, tối đa 256 block buffer và scratch hash 1048576 byte:

```text
256 × 16384 = 4,194,304 byte = 4 MiB
scratch hash = 1 MiB
tổng hai phần = 5 MiB, dưới payload budget cấu hình 32 MiB
```

Checker có phép dự trữ bảo thủ: `4 connections × (8 inflight + 2) + 64 writer queue + 64 upload queue = 168`, không vượt 256. Đây là kiểm tra nhất quán tham số, **chưa chứng minh implementation bounded** vì implementation chưa có.

Còn phải xét các buffer header/control, consumer đang xử lý ngoài queue, copy tạm, metadata, managed heap và native/runtime/kernel socket memory. Payload budget không bằng toàn bộ working set. Các state có thể transfer cùng một lease thay vì tạo thêm; khi lập accounting phải nói rõ đang đếm object, allocation hay reservation để tránh đếm thiếu hoặc nhầm số thực.

## 7. Áp dụng VstHelper có chọn lọc

| Kiến thức | Dùng để hiểu điều gì? | Điều kiện trước khi tích hợp |
|---|---|---|
| Slot/page/bin | Cấp phát nhỏ, làm tròn size, tái sử dụng | Size/alignment và lifetime được kiểm chứng |
| Buffer view | Pointer+length, slice | Owner nền còn sống, range hợp lệ |
| RawList | Grow/capacity/locality | Không giữ pointer qua thao tác đổi allocation |
| Win32 I/O | Handle, offset, flush policy | Windows guard, bytes thực tế và I/O policy tương đương |
| Lock/atomic | Quyền cập nhật, contention | Không giữ custom lock qua await hoặc blocking I/O |

Audit ghi rõ request 4097 byte được làm tròn tới 8192 và đi nhánh AllocateHuge của snapshot. Không trình bày block 16 KiB như đang dùng bin nhỏ hiện có. Cần thử biên 4096/4097/8192 và đọc implementation, không chỉ gọi Rent thấy chưa crash.

Không dùng generic có managed reference vào tập API pointer chưa có ràng buộc unmanaged phù hợp. Không dùng GetHandle(object,offset) trong transport khi chưa có hợp đồng pin/lifetime. Giữ nguyên snapshot; adapter/biến thể nằm ở source riêng và mô tả nguồn.

Adapter MemoryManager/IMemoryOwner là ranh giới ownership phải học, không phải vài dòng cast byte* sang Memory. Core vẫn cần đường managed độc lập. Đo managed/Vst phải giữ cùng block size, limits, scheduler, I/O và môi trường Windows; kết quả có thể không nhanh hơn.

## 8. Bài luyện có failure path

**A · Vẽ owner.** Điền owner tại Rent, receive pending, enqueue chờ, enqueue thành công, write pending, write xong. Chèn lỗi ở từng mũi tên: ai trả?

**B · Tính budget.** Với 64 buffer, block 16 KiB và scratch 1 MiB, hai phần chiếm bao nhiêu? Nếu tăng block gấp 4 mà không đổi cap, số bộ nhớ nào đổi?

**C · Managed trước.** Khi được giao, viết một lần thuê có tracker test cho outstanding count, transfer và Return đúng một lần. Thử queue đầy, cancel, writer lỗi và shutdown còn job. Không viết test chỉ đếm gọi Dispose khi consumer chưa settle.

**D · Native sau.** Chỉ trên tập API đã review: kiểm tra size, pattern byte, reuse và failure/cleanup. Test an toàn cần hợp đồng và instrumentation, không chỉ stress không crash.

<details>
<summary>Gợi ý B và tiêu chí A/C</summary>

64×16 KiB=1 MiB; cộng scratch=2 MiB. Block gấp 4 làm pool phần đó thành 4 MiB, tổng với scratch=5 MiB. A/C: mọi nhánh phải kết thúc với một owner được xác định; tài nguyên chỉ Returned sau consumer cuối. Kết quả “outstanding=0” vẫn chưa đủ nếu đã Return quá sớm, nên cần trace thứ tự.

</details>

Hiểu bài khi tự chứng minh lifetime và accounting bằng một timeline cụ thể. Sau đó mới chọn bài thực hành Vst theo Linh; tiếp [Bài 08](08-peer-roi-va-resume.md).
