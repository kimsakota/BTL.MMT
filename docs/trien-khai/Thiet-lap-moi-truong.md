# Thiết lập môi trường C# và lab

Kiểm tra máy Linh ngày 23/10/2026: Windows x64, .NET SDK 10.0.301, runtime 10.0.9. Solution target net10.0; không cần workload MAUI/mobile. Dùng .NET 10 stable theo [tài liệu Microsoft](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview). Máy của bốn thành viên khác chưa được kiểm tra.

## Công cụ

| Cần khi nào | Công cụ | Kiểm tra |
|---|---|---|
| Mọi người, G0 | .NET 10 SDK, Visual Studio hoặc VS Code/C# Dev Kit | `dotnet --info` và build solution |
| Làm việc nhóm | GitHub Desktop và Git | Clone, nhánh, PR thử sau khi có repo/remote |
| Kiểm tra repo | Python 3.10+ | `python tools/check_project.py` |
| Đến test hai máy | Wireshark/Npcap hoặc công cụ capture tương đương | NIC đúng, capture thấy SYN và data |
| Đến benchmark | iperf3 trên host hỗ trợ và công cụ tài nguyên OS | Chạy riêng để đo đường tham chiếu |
| Đọc/trích lại slide | pypdf + Poppler | Chỉ tác vụ nghiên cứu, không phải runtime của peer |

## Mở dự án

Visual Studio: Open a project or solution → `ET4230.P2P.sln`. VS Code: File → Open Workspace from File → `BTL.code-workspace`. Task `ET4230: build` gọi dotnet trực tiếp; không dùng WSL để chạy thư viện native Windows.

Trong PowerShell từ root BTL:

```powershell
dotnet build ET4230.P2P.sln --configuration Debug
dotnet run --project src/ET4230.Cli -- --help
dotnet run --project src/ET4230.Cli -- --version
python tools/check_project.py
```

CLI bootstrap chưa có command tracker/peer; gõ các tên đó trả mã 2. Hướng dẫn chạy mạng thật sẽ được bổ sung cùng G1/G2, không cung cấp lệnh giả có vẻ chạy được.

## Đến lúc làm lab

Local: mỗi process có port peer riêng, tracker 42300, peer ví dụ 42301–42308; đây là lựa chọn ứng dụng, không phải port chuẩn của giao thức nào. Dùng loopback để debug framing.

Hai máy: ghi bảng máy, OS/SDK, IPv4, subnet, port listen và endpoint công bố. `0.0.0.0` chỉ có nghĩa lắng nghe mọi interface; địa chỉ công bố phải là IP truy cập được. `127.0.0.1` chỉ quay lại chính máy đang chạy. Kiểm tra IP/subnet/gateway, network isolation của Wi-Fi/VM và firewall khi không kết nối được. Chỉ mở đúng port/phạm vi lab khi cần, không tắt firewall toàn máy.

Vst native chạy trên Windows. Có thể dùng Linux VM/router cho điều kiện mạng, nhưng không mặc định gọi `kernel32.dll` được trong Linux. Đo managed/Vst trên cùng Windows; số liệu cross-OS là một thí nghiệm khác.

## Khi gặp lỗi

Gửi command, terminal/cwd, SDK, lỗi đầu tiên, exit code và diff liên quan. SDK không đủ → setup; compiler lỗi → C#/reference; connection refused → listener/endpoint/port; timeout → đường đi/firewall hoặc peer không tiến triển; hash sai → offset/length/state/lifetime. Không sửa thuật toán chọn piece để che một lỗi bind hoặc byte order.
