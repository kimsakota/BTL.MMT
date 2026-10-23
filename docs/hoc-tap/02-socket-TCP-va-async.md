# Bài 02 · TCP là dòng byte, socket là tài nguyên

**Học trước:** Bài 01; các mục reference, Memory, Task và cancel trong [Nền tảng C#](CSharp-nen-tang.md). **Mục tiêu:** giải thích được mỗi socket do ai tạo/đóng, mỗi lần gửi/nhận đã làm được bao nhiêu byte và buffer còn phải sống đến lúc nào.

Slide: `04-1 UDP, TCP.pdf` trang PDF 18–27; `04-3 Socket.pdf` trang 11–21, 27, 32–42. Kiến thức TCP được đối chiếu ở [RFC 9293](https://www.rfc-editor.org/rfc/rfc9293.html), phần 2 và phần mở/đóng kết nối. Bài này không yêu cầu tự cài TCP của OS.

## 1. TCP giải quyết gì, còn thiếu gì?

Trong một kết nối đang hoạt động, TCP cung cấp dòng byte có thứ tự; các cơ chế ở stack xử lý mất/đảo thứ tự ở mạng. Khi không tiếp tục được, ứng dụng vẫn có thể nhận lỗi hoặc kết nối kết thúc. TCP không hứa mọi chương trình ở máy kia sẽ luôn chạy xong.

TCP không giữ ranh giới các lần Send như ranh giới thông điệp. Gửi `ABC` rồi `DEFG` tạo dòng `ABCDEFG`; bên nhận có thể thấy `AB`, `CDE`, `FG`, hoặc các cách chia khác, vẫn đúng.

```text
Ứng dụng gửi:     [ABC] [DEFG]
Dòng byte:         A B C D E F G
Ứng dụng nhận:    [A] [BCDE] [FG]   ← một khả năng hợp lệ
```

Do đó TCP không tự biết một BLOCK dài bao nhiêu. **Framing** ở Bài 03 sẽ giải quyết. TCP ACK cũng không chứng minh writer ứng dụng đã ghi/hash tệp xong; điều ấy cần trạng thái/completion ở tầng ứng dụng.

## 2. Hai loại socket ở phía nhận kết nối

| Socket | Tạo bằng cách nào? | Trách nhiệm |
|---|---|---|
| Listener | Tạo → Bind → Listen | Chờ và Accept kết nối |
| Connected socket | Accept trả về hoặc Connect thành công | Gửi/nhận byte của một kết nối |

Pseudocode, chưa phải implementation:

```text
Server: tạo listener → bind endpoint → listen
        lặp accept → lấy connected socket → giao handler quản lý
Client: tạo socket → connect server endpoint → gửi/nhận → dọn socket
```

Listener vẫn tồn tại sau accept. Dispose connected socket A không tự dispose listener hoặc kết nối B. Nếu server chỉ accept một lần rồi kết thúc scope listener, client thứ hai có thể không được phục vụ; phải xác định vòng đời có chủ đích.

Một accept loop có thể chờ kết nối tiếp trong khi handler cũ đang chờ dữ liệu. Cần giữ và quan sát các handler, giới hạn số connection, xử lý lỗi và đợi cleanup lúc dừng; bỏ Task không theo dõi không phải thiết kế concurrent server hoàn chỉnh.

## 3. Đọc đúng ý nghĩa giá trị trả về

[ReceiveAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync?view=net-10.0) trả số byte nhận được; có thể ít hơn vùng đệm yêu cầu. Với yêu cầu có độ dài dương, trả 0 là đầu kia đã đóng chiều gửi bình thường và dữ liệu đệm đã được đọc hết. Đừng diễn giải một lần đọc buffer rỗng như cùng trường hợp đó. Reset/lỗi kết nối có thể đi qua exception.

[SendAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendasync?view=net-10.0) trả số byte đã gửi trong thao tác; helper gửi đủ phải cập nhật phần còn lại. Không coi một lần gọi là toàn bộ frame đã được chuyển cho ứng dụng nhận.

Ví dụ cần đọc header 16 byte:

| Lần | Offset trước đọc | Số còn thiếu | Receive trả | Offset sau |
|---:|---:|---:|---:|---:|
| 1 | 0 | 16 | 5 | 5 |
| 2 | 5 | 11 | 7 | 12 |
| 3 | 12 | 4 | 4 | 16 |

Lần 2 phải điền vào phần bắt đầu từ offset 5, không ghi đè 5 byte đầu. Chỉ sau lần 3 mới có đủ header để decode.

Với gửi 10 byte, nếu lần đầu gửi 4 thì lần sau gửi slice bắt đầu tại byte 4, dài 6. Gửi lại từ byte 0 tạo dữ liệu lặp. Nếu thao tác không tiến triển hoặc lỗi, phải kết thúc bằng lỗi theo hợp đồng; không tạo vòng lặp vô hạn.

## 4. Tự thiết kế SendAll và ReceiveExactly

Đừng chép một vòng while khi chưa định nghĩa đầu ra. Với một helper **dự kiến**, điền trước:

| Hợp đồng | Input | Thành công | Thất bại |
|---|---|---|---|
| SendAll | Connected socket, vùng byte, token | Mọi byte đã gửi theo API | Lỗi/cancel/không tiến triển |
| ReceiveExactly | Socket, vùng đích cần điền, token | Vùng đích đủ byte | EOF trước đủ, lỗi, cancel |

Có thể dùng exception cho truncated frame hoặc result phân biệt EOF sạch; cả nhóm phải thống nhất. Parser đầu frame cần phân biệt EOF trước khi bắt đầu frame mới với EOF giữa một frame. Socket helper và protocol parser không nhất thiết dùng cùng một enum kết quả.

Pseudocode nhận đủ:

```text
filled = 0
trong khi filled < targetLength:
    đọc vào vùng [filled, targetLength)
    nếu EOF: xử lý thiếu dữ liệu theo hợp đồng
    nếu lỗi/hủy: kết thúc, không báo success
    filled += số byte đã nhận
```

Hãy tự viết implementation sau khi giải bảng trace. Tài liệu không cung cấp sẵn cả transport của đề tài.

## 5. Một reader và một luồng phát frame

Hai reader cùng nhận trên một connected socket có thể chia byte cho hai parser theo cách không mong muốn. Bản cơ sở dùng một receive loop hiểu framing trên mỗi connection.

Hai producer gửi trực tiếp cũng nguy hiểm:

```text
A gửi header A
B gửi header B
A gửi payload A
B gửi payload B
```

Từng Send đúng vẫn tạo stream sai đối với parser mong header A → payload A. Cần một send queue/owner serialize **toàn bộ frame**. Khóa riêng từng lần Send chưa bảo vệ cả cặp header/payload. Không giữ custom lock qua await.

Ở hai chiều của TCP có thể có hoạt động gửi/nhận đồng thời; điều đó khác nhiều reader tranh cùng một chiều. Thread safety của API không tự làm giao thức của mình đúng.

## 6. EOF, shutdown và dispose

TCP có hai chiều độc lập. A đóng chiều gửi có thể làm B đọc tới EOF, trong khi B còn gửi dữ liệu theo giao thức. Shutdown biểu diễn kết thúc chiều phù hợp; Dispose kết thúc sử dụng handle local. Thời điểm peer quan sát EOF/error phụ thuộc dữ liệu còn đệm và diễn biến kết nối.

Không bắt buộc mọi ca đóng có đúng bốn packet; ACK/FIN có thể kết hợp, reset là tình huống khác. Xem `04-1 UDP, TCP.pdf` trang PDF 22 cùng ghi chú chuẩn hóa trước khi dùng tiêu đề slide để suy ra số gói cố định.

Với receive trả 0 ở giữa header, không chờ mãi cho 11 byte còn lại như trường hợp tạm chưa có dữ liệu. Với lỗi đĩa phía nhận, TCP có thể vẫn đã ACK byte; app vẫn phải báo phiên truyền thất bại.

## 7. Cancel chưa phải kết thúc

Timeline minh họa:

```text
t0 thuê buffer → t1 ReceiveAsync đang dùng → t2 yêu cầu cancel
               → t3 operation kết thúc/throw → t4 dọn hoặc chuyển ownership
```

Không Return ở t2. Một Task đang dùng Memory phải kết thúc trước khi vùng ấy được tái sử dụng. Nếu chỉ bỏ chờ bằng timeout nhưng thao tác còn chạy, scope cleanup phải xử lý thao tác ấy, không giả định nó đã biến mất. Đối chiếu [cooperative cancellation](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads).

Dừng một runtime cần ngừng nhận việc mới, yêu cầu các thao tác dừng, quan sát kết quả rồi dọn tài nguyên theo thứ tự. Dispose socket có thể là một phần cơ chế ngắt I/O được thiết kế, nhưng vẫn phải quan sát các task và không trả buffer sớm.

## 8. Bài luyện theo bốn mức

**Mức 1 · Không code.** Trace nhận 12 byte với các lần trả 2, 3, 7. Viết offset/remaining. Sau lần 2, peer EOF thì đã nhận bao nhiêu và còn thiếu bao nhiêu?

**Mức 2 · Hàm nhỏ.** Tự viết helper đếm phần đã gửi/nhận qua một abstraction cho test có thể trả ít byte. Test không cần timing mạng để buộc phân mảnh.

**Mức 3 · Lab socket khi được giao.** Echo một payload cố định nhỏ, log endpoint và số byte thực tế mỗi thao tác. Thử client đóng sau một phần payload; handler không lặp vô hạn và listener vẫn phục vụ được client tiếp theo.

**Mức 4 · Failure path.** Hủy khi receive đang chờ; giải thích task nào đã kết thúc, ai dispose socket và khi nào trả buffer. Chưa thêm unsafe pool.

<details>
<summary>Gợi ý tự kiểm tra</summary>

Mức 1: các offset 0→2→5→12, remaining 12→10→7→0. EOF sau lần 2 có 5/12 byte, thiếu 7; không báo message đủ. Mức 2 phải thử nhiều cách chia cùng một dãy byte, không chỉ một trường hợp thuận lợi. Mức 3 không kiểm tra “một send bằng một receive”. Mức 4 cần phân biệt thời điểm phát tín hiệu và thời điểm operation thực sự settle.

</details>

Bạn hiểu bài khi tự giải thích send/receive count, listener/accepted socket, half-close và lifetime qua await. Tiếp [Bài 03](03-framing-va-giao-thuc.md); thực hành không tự ghi rằng runtime peer đã hoàn thành.
