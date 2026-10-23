# Bài 04 · Tệp được chia, ghi và xác minh như thế nào?

**Học trước:** Bài 03, số nguyên/array/reference trong nền C#. **Mục tiêu:** tính chính xác mọi offset, giải thích vì sao nhận đủ byte chưa đồng nghĩa tệp đúng, và nêu thời điểm một piece được phép upload.

Nguồn môn: `01 Introduction.pdf` trang PDF 52–55, 72–74 cho mô hình phân phối/throughput. Piece/block/manifest là thiết kế ứng dụng ở [Giao thức v1](../thiet-ke/Giao-thuc-v1.md). [BEP 3](https://www.bittorrent.org/beps/bep_0003.html) cung cấp một hệ P2P để đối chiếu ý tưởng; v1 của nhóm không tương thích wire BitTorrent và chọn SHA-256 riêng.

## 1. Vì sao không chỉ gửi toàn bộ tệp như một mảng?

Tệp có thể lớn hơn phần RAM muốn dành. Nhiều nguồn có thể giao phần khác nhau, một nguồn có thể ngắt giữa chừng. Cần biết phần nào thiếu/đã ghi/đã đúng để tải tiếp và chia sẻ lại.

- **Tệp:** đầu ra cuối cùng, có tổng size và file ID.
- **Piece:** đơn vị kiểm tra hash và công bố availability.
- **Block:** một lần yêu cầu/gửi dữ liệu bên trong piece.
- **Manifest:** metadata kỳ vọng để kiểm tra input và dữ liệu nhận.

Một piece có thể gồm nhiều block. Nhận đủ một block chưa cho phép công bố toàn piece Verified.

## 2. Tính bằng một tệp rất nhỏ

Ví dụ học tập: F=19 byte, P=8 byte/piece, B=4 byte/block. Đây là kích thước minh họa, không phải tham số benchmark.

```text
Tệp:   [0 1 2 3 |4 5 6 7] [8 9 10 11|12 13 14 15] [16 17 18]
Piece:         0                      1                   2
```

| Piece | Offset tệp | Piece length | Block offset trong piece | Block length |
|---:|---:|---:|---|---|
| 0 | 0 | 8 | 0, 4 | 4, 4 |
| 1 | 8 | 8 | 0, 4 | 4, 4 |
| 2 | 16 | 3 | 0 | 3 |

Tổng 3 piece và 5 block. Piece 2 không được đòi 4 byte chỉ vì B=4. Byte offset cuối hợp lệ là 18, không phải 19.

Công thức với F không âm, P/B dương:

```text
pieceCount = F / P + (F % P != 0 ? 1 : 0)
pieceStart(i) = i × P
pieceLength(i) = min(P, F - pieceStart(i))
fileOffset(i,o) = pieceStart(i) + o
blockLength(i,o) = min(B, pieceLength(i) - o)
```

Chỉ áp dụng với i trong range và o hợp lệ. V1 yêu cầu o là bội B, nằm trong piece, length đúng phần còn lại hoặc B; không chấp nhận block 0 byte. Dùng kiểm tra overflow/64-bit trước khi chuyển độ dài sang int. Công thức `(F+P-1)/P` có thể overflow do cộng trước; công thức chia/dư tránh trường hợp đó.

Tệp rỗng: 0 piece, mảng pieceHashes rỗng, vẫn có hash toàn tệp rỗng. Không chia cho pieceCount hoặc bắt buộc tồn tại piece 0.

## 3. Manifest là bản mô tả kỳ vọng

Manifest v1 gồm version, displayName, fileSizeBytes, pieceSizeBytes, blockSizeBytes, fileId và pieceHashes. Bản cơ sở chia sẻ manifest ngoài băng trước phiên; đây không phải chức năng tracker tải metadata tự động.

Trước khi dùng, kiểm tra số hash khớp số piece, kích thước hợp lệ, ID/hash đúng dạng và giới hạn metadata. DisplayName chỉ để hiển thị; caller chọn đường output. Không lấy tên từ mạng ghép thẳng thành đường ghi tùy ý.

Ví dụ tệp 1 MiB+3 byte, P=1 MiB, B=16 KiB: pieceCount=2; piece 0 có 64 block đầy, piece 1 chỉ có một block 3 byte. Có 65 block, không phải 128. Đây là ca dễ tìm lỗi làm tròn.

Manifest phải mô tả cùng phiên bản dữ liệu mà uploader đọc. Nếu hash xong rồi người dùng sửa tệp nguồn, TCP vẫn có thể truyền tốt nhưng receiver thấy hash sai. Lab cần input ổn định hoặc cơ chế phát hiện thay đổi.

## 4. Hash trả lời câu hỏi nào?

Hash SHA-256 biến dữ liệu thành digest cố định. Receiver hash **dữ liệu thực nhận/đã ghi**, so với digest **kỳ vọng từ manifest đã tin cậy**. Nếu lấy hash từ chính dữ liệu lỗi rồi so với nó, test luôn vô nghĩa.

Hash toàn tệp kiểm tra đầu ra; hash theo piece giúp biết vùng nào cần tải lại và khi nào có thể upload. Hash không mã hóa nội dung, không tự xác thực danh tính peer và không làm một manifest không đáng tin trở nên đáng tin.

Ví dụ: B nhận đủ 8 byte piece 1 nhưng một byte ở offset 10 bị thay. Byte count vẫn đúng; digest khác nên không được Verified. Một lỗi ứng dụng ghi nhầm offset cũng có thể gây tình huống này mà không có lỗi đường truyền.

## 5. Từ Receive tới Verified: bốn mốc khác nhau

```text
Received → đưa job ghi → Written → đủ piece, đọc/hash → Verified → HAVE/upload
```

Received chỉ nói dữ liệu đến bộ nhớ. Written nói thao tác ghi theo hợp đồng đã hoàn tất. Verified nói nội dung piece khớp hash kỳ vọng sau các completion ghi cần thiết. Durable nói dữ liệu sống qua một loại sự cố lưu trữ theo chính sách flush cụ thể; Written không tự đồng nghĩa Durable.

State piece cơ bản: Missing/Partial → Verifying → Verified. Trong lúc hash, không để một completion cũ ghi đè cùng vùng rồi vẫn dùng kết quả hash trước đó. Bài 08 bàn generation và resume.

Vì sao đọc lại `.part` để hash? Nó kiểm tra dữ liệu thực ở vị trí ghi và giảm nhu cầu giữ cả piece trong RAM. Đổi lại có thêm I/O; đây là trade-off cần giữ tương đương giữa các cấu hình thí nghiệm.

## 6. Ghi theo offset và cuộc đua Seek/Write

Giả sử cùng một handle có vị trí hiện hành:

```text
Worker A: Seek(0)
Worker B: Seek(8)
Worker A: Write(block A)  ← có thể ghi từ 8 thay vì 0
```

Từng API riêng lẻ hợp lệ nhưng chuỗi sai. Bản cơ sở có thể dùng một writer sở hữu handle và serialize Seek+Write, hoặc API ghi có offset tường minh như [RandomAccess.WriteAsync](https://learn.microsoft.com/en-us/dotnet/api/system.io.randomaccess.writeasync?view=net-10.0). API offset không miễn kiểm tra range, block trùng, concurrent overlapping writes hoặc lifetime buffer.

Writer nhận `(fileOffset, data, length)` và trả completion theo contract. Scheduler quyết định thiếu phần nào; writer không tự chọn nguồn. Nếu queue writer đầy, cần backpressure, không giữ mọi block mới trong RAM vô hạn.

## 7. Khi lỗi thì ai quyết định gì?

- EOF giữa block: frame chưa đủ; không tăng Written/Verified.
- Ghi đĩa lỗi: báo lỗi write, giữ trạng thái không hoàn tất; không coi Receive xong là thành công.
- Hash sai: piece không được quảng bá; reset đúng state và áp dụng retry budget.
- Block tới lặp: không tăng completion hai lần, không để write cũ phá piece đang xác minh.
- Tệp đích có size đúng nhưng có hole/byte chưa ghi: size chưa chứng minh nội dung đúng.

Thành công toàn phiên cần mọi piece đúng và hash toàn tệp khớp; chỉ đổi trạng thái/đầu ra cuối theo quy ước đã chốt.

## 8. Bài tập tăng dần

**A · Tính trên giấy.** Với F=17, P=8, B=4, ghi tất cả piece/block, offset tệp và length. Sau đó đổi F=0 và F=16.

**B · Hàm thuần.** Tự viết pieceCount/pieceLength/fileOffset, có test 0,1,B−1,B,B+1,P−1,P,P+1; thêm input âm/size bằng 0 và giá trị lớn gây overflow. Chưa cần mạng.

**C · I/O nhỏ.** Ghi các block tệp 19 byte theo thứ tự đảo rồi đọc lại đối chiếu byte kỳ vọng. Sau đó sửa một offset để thấy test thực sự fail. Không cố định thứ tự arrival theo thứ tự file.

**D · Tích hợp khi được giao.** Truyền tệp một nguồn, thử tệp nhị phân/cuối lẻ, ngắt giữa chừng, sai hash có chủ đích. Chỉ báo Done khi output/hash và failure path đều đúng.

<details>
<summary>Đáp án chính của A và điểm cần thấy ở C</summary>

F=17 có piece length 8,8,1; block file offsets 0,4,8,12,16, độ dài 4,4,4,4,1. F=0 không có piece/block; F=16 có hai piece đầy, bốn block. C phải so nội dung ở đúng vị trí, không chỉ tổng byte ghi. Ghi 19 byte vào sai chỗ vẫn có thể tạo file dài đủ nhưng sai digest.

</details>

Bạn hiểu bài khi tự tính mọi range và phân biệt Received/Written/Verified/Durable. Task ứng dụng liên quan F01/F02/F03, test T04/T08/T09; chưa mặc định ai nhận. Tiếp [Bài 05](05-tracker-va-discovery.md) hoặc [Bài 07](07-ownership-buffer-va-Vst.md) theo phần Linh giao.
