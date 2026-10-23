# Lõi P2P sẽ được nhóm tự triển khai

Project thư viện hiện chưa có implementation. Bắt đầu từ hợp đồng trong [giao thức](../../docs/thiet-ke/Giao-thuc-v1.md) và [kiến trúc](../../docs/thiet-ke/Kien-truc-du-an.md).

Các module đích, tạo file khi đến task: Protocol, Transport, Manifest, Discovery, Scheduling, Storage, Buffers, Diagnostics. Không tạo hàng loạt class trống hoặc ném `NotImplementedException` để báo đã hoàn thành kiến trúc.

Core cơ sở dùng BCL .NET, không phụ thuộc snapshot VstHelper. Adapter Vst sẽ được viết sau khi framing và ownership đã có kiểm thử.
