# Bài 00 Mở dự án và hiểu C# chạy như thế nào

Mục tiêu: mở đúng solution, build/run bootstrap và giải thích được assembly/reference. Chưa viết socket hoặc allocator. Tài liệu thiết lập ở [môi trường](../trien-khai/Thiet-lap-moi-truong.md).

## Từ source đến process

`ET4230.P2P.sln` gom các project để IDE/build điều phối. `.csproj` chọn framework, loại đầu ra và references. Compiler biên dịch C# thành assembly; runtime .NET nạp và thực thi, JIT có thể tạo mã máy. `dotnet build` tạo đầu ra; `dotnet run` chạy chương trình. Hai thao tác có mục đích khác nhau.

Core là library, không có entry point; Cli là executable có Program.cs; VstReference là library build snapshot bằng target hiện tại. Cli tham chiếu Core, hiện không tham chiếu VstReference. Vì thế `--help` chạy được không chứng minh một lệnh Vst native đã được chạy.

Các file ở bin/obj là sản phẩm build và bị Git bỏ qua. Xóa thư mục build không xóa source, nhưng không cần làm lại clean/rebuild liên tục khi chỉ sửa tài liệu.

## C# và C++ cần phân biệt

C# có class/struct, generic, exception, Task và runtime GC. GC quản lý object managed, nhưng không tự trả một vùng `Marshal.AllocHGlobal` theo cách Vst sử dụng; IDisposable/lease cần quy định rõ. Struct copy theo giá trị có thể sao chép một con trỏ mà không tạo bản sao vùng nhớ. Cùng cú pháp dấu ngoặc không có nghĩa ownership, ABI và async của hai ngôn ngữ giống nhau.

`using` namespace giúp tìm tên; using/Dispose với tài nguyên lại là cơ chế vòng đời. `ProjectReference` giúp compiler/build thấy assembly khác; viết `using Vst` không tự thêm dependency hoặc bảo đảm platform hỗ trợ.

## Thực hành theo bước

1. Mở workspace/solution và tìm ba csproj. Chỉ ra reference thực tế trong Cli.
2. Chạy `dotnet --info`, ghi SDK/OS của mình; không dùng log của Linh thay cho máy mình.
3. Build solution. Nếu thấy CS8500 ở VstReference, đọc [audit](../thiet-ke/Tai-su-dung-VstHelper.md); đừng tự tắt warning để báo sạch.
4. Chạy `dotnet run --project src/ET4230.Cli -- --help`, rồi `--version`. Giải thích phần trước/sau `--`.
5. Chạy một command chưa triển khai và ghi exit code. Trong PowerShell, xem `$LASTEXITCODE` ngay sau lệnh; bootstrap kỳ vọng 2.
6. Đọc Program.cs, giải thích điều kiện, return và output. Không sửa nội dung để giả đã có tracker.

## Tự kiểm tra

- Vì sao Core build được khi chưa có implementation P2P?
- Vì sao namespace và project reference là hai việc khác nhau?
- Một assembly unsafe biên dịch được có bảo đảm pointer còn hợp lệ khi await không?
- `Buffer` struct được gán sang biến khác có tạo vùng nhớ thứ hai không?

Hoàn thành bài khi có log thật và giải thích được các câu hỏi bằng lời của mình. Bước tiếp theo là [Bài 01](01-mang-topology-va-endpoint.md), chưa phải sửa Memory.cs.
