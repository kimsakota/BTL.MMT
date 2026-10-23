# Bài 02 TCP byte stream và vòng đời tác vụ

Đọc Socket PDF 11–21, 27, 32–42 và TCP PDF 18–27, kèm [ghi chú chuẩn hóa](Ghi-chu-chuan-hoa-slide.md).

## Những thao tác cần hiểu

Listener tạo/bind/listen, accept tạo socket kết nối để trao đổi. Client connect tới listener. Send đưa một số byte vào đường gửi; receive trả số byte đã có, có thể ít hơn dự kiến. Với yêu cầu nhận số byte dương, 0 báo peer đóng chiều gửi một cách bình thường, không phải luôn là “chưa có dữ liệu, lặp tiếp”. Lỗi/cancel/timeout có failure path riêng.

C# dùng Socket/Task/ValueTask và await để biểu diễn thao tác có thể chưa xong; await không có nghĩa tạo một thread mới cho mỗi block. CPU hash nặng và I/O bất đồng bộ cũng là hai loại công việc khác nhau. Không dùng `.Result`/`.Wait()` để chặn tùy tiện một đường async đang cần tiến triển.

## Hợp đồng cần tự viết trước

Một thao tác SendAll dự kiến nhận socket, vùng dữ liệu và token; hoàn tất khi đã gửi đủ hoặc trả lỗi rõ. ReceiveExactly dự kiến nhận socket, buffer cần điền và token; phân biệt đủ dữ liệu với EOF giữa chừng. Đây là hợp đồng bài học, chưa có helper implementation trong Core.

Một connection chỉ có một reader hiểu framing, và một queue điều phối sender. Hai producer gửi header/payload trực tiếp vào cùng socket có thể xen byte; một khóa riêng cho từng lần send chưa đủ nếu header và payload bị tách qua nhiều lần gọi.

## Bài nhỏ theo chặng

1. Mô tả pseudocode listener/client và ai Dispose socket nào; vẽ accept loop và connection handler.
2. Tự làm echo một chuỗi byte cố định nhỏ, log số byte gửi/nhận thực tế. Thử tách phần gửi thành hai lần và nhận với buffer nhỏ.
3. Cho client đóng sau một phần dữ liệu; giải thích expected EOF và cleanup.
4. Thêm cancel có deadline; đợi operation đã settle rồi mới trả buffer. Vẽ owner trước/sau await.
5. Chạy nhiều client và kiểm tra listener vẫn accept trong khi handler khác đang chờ.

Không cần viết cả P2P trong bài này. Echo là bài tập hẹp và sẽ được tác giả tự viết; không coi một message echo thành giao thức truyền tệp.

## Câu hỏi và tiêu chí

Send thành công có chứng minh phía nhận đã ghi tệp không? Một receive có phải đúng một send không? Vì sao cần cả Shutdown/Dispose theo tình huống? Khi cancel token được phát, buffer đã được phép Return ngay chưa?

Hoàn thành khi echo/EOF/cancel có log, code cleanup được giải thích và hiểu vì sao cần framing. Tiếp [Bài 03](03-framing-va-giao-thuc.md). API đối chiếu: [Socket.SendAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendasync), [ReceiveAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync).
