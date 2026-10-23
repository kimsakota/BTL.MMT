# Hợp đồng giao thức ứng dụng v1

Đề xuất nội bộ cho các thành viên làm việc độc lập, chưa có codec hay socket implementation. Đây là giao thức riêng qua TCP. Manifest được chia sẻ ngoài băng ở bản cơ sở; một kết nối peer gắn với một file ID qua HELLO. Integers trên wire dùng unsigned big-endian; JSON là UTF-8 không BOM. Không gửi raw layout struct/con trỏ C#.

## Header 16 byte

| Offset | Byte | Trường | Giá trị |
|---:|---:|---|---|
| 0 | 4 | magic | ASCII `E423`, hex `45 34 32 33` |
| 4 | 1 | version | 1 |
| 5 | 1 | type | Bảng type bên dưới |
| 6 | 2 | flags | 0 trong v1 |
| 8 | 4 | requestId | ID không trùng request đang sống trên cùng connection |
| 12 | 4 | payloadLength | Số byte payload, không gồm header |

Request cần phản hồi dùng ID khác 0; phản hồi giữ nguyên ID. Notification như HAVE/BITFIELD dùng 0. IDs chỉ có ý nghĩa trên connection đó; khi quay vòng số phải tránh ID đang chờ. Timeout là chính sách ứng dụng, không phải TCP RTO.

Đọc đủ header trước, kiểm tra magic/version/type/flags/length rồi mới thuê payload. Đọc thiếu header hoặc payload và gặp EOF là frame không hoàn tất. Một receive có thể thiếu hoặc chứa nhiều frame; không dùng số lần receive để đếm message. Max payload chung 1 MiB, control JSON tối đa 64 KiB, và giới hạn theo từng type còn chặt hơn. Gửi cũng phải lặp đến đủ byte.

## Type và payload

| Type | Tên | Payload | ID/đáp ứng |
|---:|---|---|---|
| 1 | HELLO | JSON `{peerId, fileId}` | request ID >0 |
| 2 | HELLO_OK | JSON `{peerId, fileId}` | cùng ID HELLO |
| 3 | BITFIELD | Bitmap đúng `ceil(pieceCount/8)` byte | ID=0; sau handshake |
| 4 | HAVE | `pieceIndex:uint32` | ID=0, đúng 4 byte |
| 5 | REQUEST_BLOCK | `pieceIndex:uint32, offset:uint32, length:uint32` | ID>0, đúng 12 byte |
| 6 | BLOCK | Ba trường như request + `length` byte data | cùng ID request; payload đúng 12+length |
| 7 | CANCEL | Không có payload | ID của request muốn hủy; là best effort |
| 8 | ERROR | JSON `{code, message}` | giữ ID lỗi; 0 khi lỗi connection |
| 16 | REGISTER | JSON `{peerId, host, port, fileId}` | tracker; ID>0, nhận ACK |
| 17 | LIST_PEERS | JSON `{fileId}` | tracker; ID>0 |
| 18 | PEER_LIST | JSON `{peers:[{peerId,host,port}]}` | phản hồi LIST_PEERS |
| 19 | HEARTBEAT | JSON `{peerId,fileId}` | tracker; ID>0, nhận ACK |
| 20 | ACK | JSON `{ok:true}` | tracker ACK, không phải TCP ACK |

Peer ID là UUID dạng string, file ID là SHA-256 toàn tệp, 64 hex lowercase. Tracker registry key là `(fileId,peerId)`; REGISTER lại cập nhật endpoint và TTL. HELLO chỉ được dùng với manifest đã kiểm tra file ID. Tracker và peer dùng cùng header nhưng chỉ chấp nhận type đúng vai trò.

REGISTER host phải là địa chỉ mà các peer khác thực sự truy cập được. `0.0.0.0` là bind wildcard, không phải địa chỉ công bố. Peer trên hai máy không dùng `127.0.0.1` của máy kia. Peer list loại chính peer khi client tự lọc; không đồng nhất tracker sống với mọi peer trong cache đang sống.

## Quy tắc piece và block

Bit cao nhất byte 0 đại diện piece 0, tiếp theo piece 1; bit thừa cuối bitmap phải bằng 0. Bản đầu yêu cầu block offset là bội block size, length bằng `min(blockSize, pieceLength-offset)`. Piece cuối có thể ngắn hơn; length=0 không phải request block hợp lệ. Trường offset là offset trong piece; offset tệp = pieceIndex × pieceSize + offset, tính ở 64-bit có kiểm tra overflow.

BLOCK chỉ được chấp nhận nếu khớp bộ `(connection,requestId,pieceIndex,offset,length)` đang chờ hoặc được xử lý rõ như kết quả trễ. Sau CANCEL, vẫn có thể nhận dữ liệu đã được gửi; không giải phóng vùng nhớ mà I/O còn dùng. Một sender chỉ phục vụ piece Verified. Nhiều request đang chờ được phép, nhưng số lượng/bộ nhớ bị giới hạn bởi config.

## Manifest ngoài băng

JSON có `version:1`, `displayName`, `fileSizeBytes` (int64 không âm), `pieceSizeBytes`, `blockSizeBytes`, `fileId`, `pieceHashes` (mảng SHA-256 theo piece index). Mảng hash có `ceil(fileSize/pieceSize)` phần tử. Tệp rỗng có 0 piece, mảng rỗng và SHA-256 rỗng chuẩn; không chia cho 0. Block size là tham số phiên và phải thống nhất với manifest trong v1.

`displayName` chỉ để hiển thị; đường dẫn output do người chạy chọn. Bản đầu không dùng tên từ mạng để tự quyết định đường dẫn ghi. Hash danh sách/tệp giúp kiểm tra toàn vẹn theo manifest đã biết, không tự xác thực người gửi.

## Lỗi và giới hạn

Sai magic/version/flags/type-role, payload quá lớn hoặc EOF giữa frame → đóng connection và log lý do. Request hợp lệ về framing nhưng piece/offset không hợp lệ → ERROR có code rồi áp dụng retry policy. Tên code đề xuất: `INVALID_REQUEST`, `PIECE_UNAVAILABLE`, `FILE_MISMATCH`, `TIMEOUT`, `INTEGRITY_FAILED`, `PROTOCOL_ERROR`.

Không cố cấp RAM theo uint32 length chưa kiểm tra; không chấp nhận âm sau cast; không đánh dấu request thành công chỉ vì send hoàn tất. Các thay đổi header/type/bit order cần review chung và cập nhật vector ở [tests](../../tests/protocol-vectors.json) trước khi hai nhánh dùng khác nhau.

## Ví dụ và tiêu chí codec

Vector HAVE piece 2: header `45343233010400000000000000000004`, payload `00000002`. Vector REQUEST_BLOCK ID=7, piece 2, offset 0, length 16384: header `4534323301050000000000070000000c`, payload `000000020000000000004000`. Đây là dữ liệu thiết kế để viết test, không phải capture đã chạy.

Codec phải vượt test stream bị cắt tại mọi ranh giới byte, hai frame ghép, unknown version/type, length quá lớn, EOF giữa frame và piece cuối. Tham chiếu bài học [03](../hoc-tap/03-framing-va-giao-thuc.md), [04](../hoc-tap/04-truyen-tep-va-integrity.md).
