# Nền tảng C# để đọc và tự viết phần của mình

Đọc trước Bài 02 nếu chưa hiểu hàm, reference, collection hoặc async. Tài liệu dùng cú pháp đơn giản trên SDK của dự án; chưa cần unsafe. Ví dụ là bài học, chưa được nối vào ứng dụng P2P.

## 1. Một hàm là một hợp đồng nhỏ

```csharp
static long CountPieces(long fileBytes, int pieceBytes)
{
    if (fileBytes < 0) throw new ArgumentOutOfRangeException(nameof(fileBytes));
    if (pieceBytes <= 0) throw new ArgumentOutOfRangeException(nameof(pieceBytes));
    return fileBytes / pieceBytes + (fileBytes % pieceBytes == 0 ? 0 : 1);
}
```

`static` cho phép gọi mà không tạo object chứa hàm; `long` trước tên là kiểu kết quả; hai mục trong ngoặc là tham số. Caller đưa giá trị thực vào, hàm kiểm tra rồi tính và return. Throw chuyển sang đường lỗi, không tiếp tục tới return.

Ví dụ học tập `CountPieces(10,4)`: chia nguyên được 2, dư 2, nên cần thêm 1 piece → 3. Với `(0,4)` được 0. Với `(-1,4)` có exception, không trả 0 để che input sai. Bài 04 giải thích công thức tránh overflow do cộng trước khi chia.

Trước khi viết hàm, trả lời: input/output là gì, input nào bị từ chối, có sửa dữ liệu của caller không, có giữ tài nguyên sau khi trả về không? Hợp đồng không chỉ là tên hàm.

## 2. Kiểu dữ liệu và phép tính

| Kiểu | Nghĩa cần biết | Dùng trong đề tài |
|---|---|---|
| byte | Số nguyên 0–255 | Byte dữ liệu/header |
| int | Số nguyên có dấu 32-bit | Độ dài vùng nhớ mà API nhận bằng int |
| uint | Số nguyên không dấu 32-bit | Trường wire v1; vẫn phải validate |
| long | Số nguyên có dấu 64-bit | Kích thước tệp, offset tệp |
| bool | true/false | Điều kiện, trạng thái |
| string | Văn bản Unicode, immutable | Tên hiển thị/ID; cần encode để gửi |
| byte[] | Object mảng byte | Buffer hoặc dữ liệu nhị phân |

`var` cho compiler suy ra kiểu; `var count = 3` vẫn là int. C# không tự coi bool là 0/1 như một số cách dùng trong C.

**Bẫy nhân trước khi ép kiểu:** `long x = intA * intB` có thể overflow ở phép nhân int trước khi gán. Chuyển một toán hạng sang long trước: `checked((long)intA * intB)`. Checked phát hiện overflow số nguyên, không thay kiểm tra input âm hoặc offset vượt tệp.

Wire length `0xffffffff` là 4294967295 theo uint, không vừa int và không hợp lệ với payload cap 1 MiB của thiết kế. Kiểm tra cap/khả năng biểu diễn trước khi cast hoặc cấp phát.

Đối chiếu [hệ kiểu C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/) và [checked/unchecked](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/checked-and-unchecked).

## 3. Value, reference và alias

Value type như int/struct được sao chép giá trị khi gán. Với reference type như mảng/class, phép gán sao chép tham chiếu tới object. Hai biến có thể cùng trỏ tới một object; đây là alias.

```csharp
byte[] first = { 10, 20, 30, 40 };
byte[] alias = first;
alias[0] = 99;
Memory<byte> view = first.AsMemory(1, 2);
view.Span[0] = 77;
byte[] copy = first.ToArray();
copy[2] = 55;
Console.WriteLine(string.Join(",", first));
Console.WriteLine(string.Join(",", copy));
```

Dự đoán hai dòng output trước khi xem giải thích:

<details>
<summary>Giải từng bước</summary>

Alias và first cùng mảng, nên first[0]=99. View bắt đầu ở vị trí 1, nên view.Span[0] sửa first[1]=77. ToArray tạo mảng khác; copy[2]=55 không sửa first. Output: `99,77,30,40` rồi `99,77,55,40`.

</details>

Tham số thông thường được truyền **theo giá trị**: khi giá trị ấy là reference, caller/callee vẫn cùng truy cập object. Trong hàm, sửa `buffer[0]` có thể được caller nhìn thấy; gán tham số sang mảng mới không đổi biến của caller. Ref/out là hợp đồng khác, chưa cần dùng để giải mọi bài.

Struct chứa field byte[] được copy thì field reference vẫn trỏ cùng mảng. Struct chứa pointer được copy cũng không nhân đôi allocation. Không coi hai bản sao struct là hai owner độc lập được free cùng vùng nhớ. Cũng không học thuộc “struct luôn ở stack, class luôn ở heap”; điều cần ở đây là cách sao chép và lifetime.

## 4. Class, record, enum và interface

```csharp
enum PieceState { Missing, Partial, Verifying, Verified }
record PeerEndpoint(string PeerId, string Host, int Port);
interface IElapsedClock
{
    TimeSpan Elapsed { get; }
}
```

Enum đặt tên cho tập trạng thái. Record ở ví dụ là reference type có equality theo dữ liệu do compiler sinh; nó không làm mọi field bên trong tự immutable. Class thích hợp cho object có hành vi/lifetime. Interface mô tả thứ caller được yêu cầu, không cung cấp mọi implementation.

Registry nhận clock qua hợp đồng để lúc chạy dùng elapsed time thật, lúc test dùng clock điều khiển được. Caller chỉ cần thời gian đã trôi. Không cần tạo interface cho mọi class; chỉ tạo khi có ranh giới cần thay hoặc kiểm thử.

`PeerEndpoint?` cho phép thiếu giá trị theo nullable annotations. Phải xử lý null; dấu `!` chỉ tắt cảnh báo, không tạo object hoặc kiểm tra runtime.

## 5. Collection: chọn theo câu hỏi

| Cấu trúc | Câu hỏi | Ví dụ |
|---|---|---|
| Array | Lấy theo index liên tục, kích thước biết trước | State piece 0…N−1 |
| List | Duyệt tập phần tử có thể tăng | Danh sách kết quả đã chụp |
| Dictionary | Tìm/cập nhật theo key | `(fileId,peerId)` → đăng ký |
| HashSet | Phần tử có tồn tại không? | Request ID đang sống |
| Queue | Lấy theo thứ tự vào trước | Bài học hàng chờ đơn luồng |

Generic T là kiểu phần tử; `Dictionary<string,int>` không nhận một Socket thay cho int. Dictionary/List thường không tự bảo đảm an toàn khi nhiều thao tác đồng thời sửa dữ liệu.

“Thấy chưa có rồi thêm” là hai bước: hai worker có thể cùng thấy chưa có. Cần một owner xử lý tuần tự hoặc đồng bộ cả chuỗi. Collection concurrent cũng không tự giữ invariant của nhiều cấu trúc liên quan.

## 6. Buffer, Span và Memory

Buffer là vùng byte tạm. Span/Memory mô tả một đoạn dữ liệu; slice thường không copy byte. ReadOnlyMemory ngăn sửa qua view ấy, không ngăn owner khác sửa vùng nền.

Span là ref struct: không giữ Span sống qua ranh giới await. Memory có thể truyền tới API async, nhưng **không tự kéo dài hợp đồng thuê** vùng nhớ. Mảng trả pool quá sớm có thể bị consumer khác ghi đè; Memory cũ vẫn nhìn cùng mảng.

Trong ví dụ mục 3, view dài 2 byte nhưng không sở hữu một bản sao 2 byte. Capacity của buffer khác số byte hợp lệ: receive trả 3 vào buffer dài 1024 thì chỉ 3 byte đầu thuộc dữ liệu mới.

Đọc [owner/consumer của Memory](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines) cùng Bài 07; tự vẽ timeline một lần sử dụng.

## 7. Exception, Dispose và using

Exception là đường lỗi ngoài kết quả bình thường. Input sai, EOF giữa frame, lỗi đĩa và cancel có nghĩa khác nhau. Đừng catch mọi thứ rồi báo thành công; chỉ catch ở nơi có thể xử lý, dọn hoặc thêm ngữ cảnh.

IDisposable.Dispose biểu diễn kết thúc sử dụng tài nguyên. Using statement/declaration giúp gọi Dispose khi ra khỏi scope, cả khi có exception. `using System.Net;` ở đầu file lại là import namespace, không dọn Socket nào.

GC thu hồi object managed theo cơ chế runtime; không hứa đóng handle đúng thời điểm cần, và không tự biết phải Return một vùng unmanaged theo quy ước của thư viện. Với I/O async, phải đợi consumer kết thúc trước khi dispose tài nguyên nó còn dùng. Using không sửa được scope sở hữu sai.

Bài suy nghĩ: một hàm trả Memory trỏ vào vùng đã được owner dispose trước return. Caller còn biến Memory nhưng lifetime đã hỏng. Chưa crash không phải bằng chứng an toàn.

## 8. Task, await và concurrency

Task đại diện cho thao tác đang chạy hoặc đã thành công/lỗi/hủy. Await lấy kết quả khi thao tác kết thúc; nếu chưa xong, hàm async có thể nhường điều khiển. Await không có nghĩa tạo thread mới.

So hai trình tự pseudocode:

```text
Bắt đầu I/O A → đợi A xong → bắt đầu I/O B → đợi B xong
Bắt đầu I/O A → bắt đầu I/O B → đợi cả A và B
```

Trình tự thứ hai cho phép hai thao tác cùng đang chờ, không bảo đảm CPU chạy song song. Nếu chúng cùng ghi vào một buffer, code có thể sai dù mỗi hàm riêng nhìn đúng. Concurrency cần thiết kế dữ liệu, không chỉ bỏ await.

I/O async khác CPU-bound hash. Không bọc mọi Socket async bằng Task.Run. Tránh Result/Wait tùy tiện trên đường async. Khi chạy nhiều Task, quản lý lỗi và đợi chúng kết thúc trước cleanup. Với ValueTask từ API, cách cơ bản là await trực tiếp một lần, chưa dùng như đối tượng được đợi lặp tùy ý.

Đối chiếu [async/await của C#](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/); Bài 02 dùng mô hình ấy vào socket.

## 9. CancellationToken và timeout

CancellationToken truyền tín hiệu yêu cầu dừng tới thao tác hợp tác. Gọi Cancel không chờ thao tác dừng. Sau yêu cầu cancel vẫn phải await/quan sát tác vụ trước khi tái sử dụng tài nguyên nó còn dùng.

Timeout là chính sách chờ quá một khoảng. Bỏ chờ Task không có nghĩa Task đã ngừng. Deadline request, TTL registry và TCP timer có chủ sở hữu khác nhau.

Vẽ ba mốc: bắt đầu receive → yêu cầu cancel → receive kết thúc/throw. Buffer phải hợp lệ tới mốc thứ ba. Bài 08 phân tích lỗi và kết quả tới muộn.

## 10. Tự luyện và đối chiếu

1. Giải CountPieces cho `(0,4)`, `(8,4)`, `(9,4)`, `(9,0)`.
2. Viết hàm sửa phần tử 0 của byte[]; hàm khác gán tham số sang mảng mới. Dự đoán dữ liệu caller trước khi chạy.
3. Registry dùng key chỉ peerId có vấn đề gì khi peer chia sẻ hai tệp?
4. Vẽ một owner và hai consumer dùng chung buffer. Khi nào được trả?
5. Vì sao hai thao tác async cùng ghi mảng có thể sai dù mỗi hàm riêng đúng?

<details>
<summary>Gợi ý tự chấm</summary>

1. Kết quả 0, 2, 3; pieceBytes=0 bị từ chối trước chia. 2. Sửa phần tử đổi object chung; gán tham số chỉ đổi bản sao reference trong hàm. 3. Cần phân biệt đăng ký bằng cặp `(fileId,peerId)`. 4. Trả sau khi consumer cuối kết thúc, hoặc transfer theo hợp đồng rõ. 5. Các ghi có thể xen nhau; Task không tự cô lập bộ nhớ.

</details>

Đủ nền khi giải thích được output, caller/callee, alias và lifetime. Không cần thuộc mọi API. Trở lại [Bài 00](00-mo-du-an-va-CSharp.md) hoặc [chọn hướng học sâu](Lo-trinh-theo-chuyen-mon.md).
