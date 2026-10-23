# Tái sử dụng VstHelper có kiểm chứng

Ngày khảo sát: 23/10/2026. Linh đã xác nhận chọn C# và được phép dùng. Mục tiêu là học/tận dụng ownership, pool, con trỏ, I/O và đồng bộ; mức nhanh hơn hoặc ít RAM hơn phải được đo.

## Nguồn và bản build thực tế

Snapshot lấy từ working tree `Hệ điều hành/BTL/VstHelper`, origin `kimsakota/ShiftSort`, HEAD `7236e72f83c2b780bf37fa42c9cf601d3d6db35c`. Working tree có sửa đổi; snapshot không bằng nội dung commit sạch. [SNAPSHOT.json](../../references/VstHelper/SNAPSHOT.json) ghi 21 file source/project và SHA-256. Không có `.git`, binary hoặc tài liệu cá nhân đi kèm.

Wrapper [ET4230.VstReference](../../src/ET4230.VstReference/README.md) dùng .NET 10 và unsafe, giữ nguyên 20 file C# gốc. Lượt build đầu 23/10 thành công với **10 cảnh báo CS8500**. Đây là kiểm tra biên dịch wrapper, không phải build target netstandard2.0 gốc hoặc kiểm thử allocator/runtime.

## Chọn phần tái sử dụng

| Phần | Giá trị cho P2P | Điều kiện tích hợp |
|---|---|---|
| `Memory/Memory.cs` | Slot/page/bins, Rent/Return | Kiểm tra size/alignment/lifetime, không dùng generic có managed reference; pool block lớn cần cơ chế mới |
| `Memory/Buffer.cs` | View con trỏ + length, thao tác byte | View không tự sở hữu vùng nhớ; giới hạn và owner phải rõ |
| `Lists/RawList.cs` | Mảng unmanaged, locality và grow | Không giữ pointer/ref qua GrowUp; không copy struct như một owner độc lập |
| `IO/File.cs` | Đọc/ghi bằng pointer và map | Windows, writer điều phối Seek+Write, kiểm tra bytes thực tế và flush policy |
| `Thread/LockManager.cs` | Atomic và reader/writer lock native | Windows; test tranh chấp; không giữ lock này qua await/I/O |
| `Keys/*` | Thiết kế metadata và index | Optional; piece ID liên tục dùng array/bitmap thường đủ |
| `StringReader/Writer` | Học thao tác biểu diễn byte | Không dùng raw native layout làm network encoding; dùng BinaryPrimitives với endian đã định nghĩa |

## Các vấn đề có bằng chứng cần giữ trong backlog

- **V01 Pool size:** `Memory.cs:436–438` làm tròn lũy thừa hai; required >=8 KiB gọi AllocateHuge. Request 4097 byte đã ra nhánh lớn. Không trình bày block 16/64 KiB như đang tái sử dụng bin của code hiện tại.
- **V02 Generic constraint:** `Rent<T>()` và `StringWriter.Endian<T>` dùng pointer/sizeof T nhưng không ràng buộc unmanaged. Cảnh báo compiler là tín hiệu cần giới hạn tập API, không mặc định mọi T hợp lệ.
- **V03 Managed reference:** `KeyEntryPath` có field interface `_keys` nhưng được lấy con trỏ trong FromString/Destroy, sinh CS8500. Không đưa struct này vào unmanaged pool của P2P. Cần nghiên cứu tách managed owner khỏi unmanaged payload khi làm biến thể.
- **V04 GetHandle:** `Memory.GetHandle<T>(object,int)` dùng địa chỉ biến object local; không có hợp đồng pin/lifetime chứng minh trả pointer hợp lệ tới nội dung object. Không sử dụng trong đường truyền.
- **V05 Pointer ổn định:** `RawList.GrowUp()` đổi vùng nhớ và trả vùng cũ. Một queue không được trao pointer tới phần tử cho worker rồi grow danh sách.
- **V06 I/O:** `Win32File` dùng con trỏ vị trí handle; phải bảo vệ toàn cặp Seek+Write. `Open()` bật WRITE_THROUGH; `Mapping()` ép capacity sang uint nên cần rà soát nếu tệp >4 GiB. `GetSize()` gọi Seek tới cuối nên cũng thay vị trí hiện tại.
- **V07 Async bridge:** API Memory<byte> không tự nhận byte*. Adapter cần MemoryManager/IMemoryOwner tương ứng và chỉ dispose sau khi tác vụ đã hoàn tất; yêu cầu hủy chưa chứng minh I/O đã ngừng.

Đây là audit tĩnh và warning đã quan sát, chưa phải mọi lỗi runtime được tái hiện. Không sửa snapshot; bản điều chỉnh để tích hợp là source ứng dụng riêng, có test và diff.

## Trình tự áp dụng

1. Có framing, truyền tệp và baseline managed đúng trước; source tham chiếu chỉ được build để nghiên cứu.
2. Thiết kế lease có một owner, Memory hợp lệ tới khi hoàn tất consumer; test dispose, cancel, timeout, OOM và double return.
3. Adapter nhỏ chỉ dùng tập API đã review, Windows guard và kích thước nhỏ phù hợp; test riêng với pattern dữ liệu và tranh chấp.
4. Nếu cần, xây pool block lớn có giới hạn; không sửa ngưỡng 8 KiB đơn giản rồi kết luận toàn allocator đã đúng.
5. So buffer per-connection/ArrayPool/Vst cùng workload, block size, pipeline, scheduler, I/O và runtime; warm-up và ghi GC allocations/live unmanaged/high-water.

Baseline per-connection là đối chứng hợp lý. Cấp mảng mới mỗi block có thể là đối chứng phụ để giải thích allocation pressure, không là đối chứng duy nhất.

Tài liệu bổ trợ: [Memory/Span ownership](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines), [ReceiveAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync), [MemoryManager](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.memorymanager-1). C# có GC và Vst dùng unmanaged; không gọi cả chương trình là “NoGC” khi chưa đo đường code và allocation thực tế.
