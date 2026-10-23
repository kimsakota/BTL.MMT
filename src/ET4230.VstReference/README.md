# Build wrapper cho snapshot VstHelper

Project này biên dịch các file C# tham chiếu bằng .NET 10, giữ nguyên source snapshot. Đây là thư viện khảo sát, chưa được nối vào CLI/Core. Thư viện gốc vẫn giữ csproj `netstandard2.0` để truy nguyên; wrapper này không xác nhận build target gốc đó.

I/O và khóa của source có P/Invoke Windows. Build thành công không chứng minh chạy đa nền tảng, thread safety hoặc hiệu năng. Các warning phát hiện cần được ghi vào audit, không sửa snapshot hoặc tắt warning để che kết quả.

Xem [kế hoạch dùng lại VstHelper](../../docs/thiet-ke/Tai-su-dung-VstHelper.md).
