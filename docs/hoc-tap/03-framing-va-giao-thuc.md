# Bài 03 Header và codec có thể kiểm chứng

Đọc Socket PDF 8–9, 20–21; Link Layer PDF 9–14 cho ý tưởng framing. Thiết kế phải theo [giao thức v1](../thiet-ke/Giao-thuc-v1.md), không sao chép bit stuffing lớp 2 vào TCP application.

## Biểu diễn byte

Số 0x01020304 có byte network big-endian `01 02 03 04`; layout int trên máy không phải hợp đồng wire. C# có BinaryPrimitives đọc/ghi số theo endian xác định; raw pointer cast hoặc `StringWriter.Endian<T>` của Vst không tự tạo wire format đúng.

Header v1 dài 16 byte. Magic/version/type/flags được kiểm tra trước payload. `payloadLength` từ mạng chưa đáng tin để cấp phát; chỉ thuê vùng nhớ sau khi kiểm tra giới hạn chung và type cụ thể. Chuyển uint32 sang int phải kiểm tra trước.

## Bài nhỏ A

Tự điền offset cho từng trường header; encode bằng giấy vector HAVE piece 2 trong [protocol-vectors](../../tests/protocol-vectors.json). Giải thích vì sao ID=0 và payloadLength=4. Chỉ viết hàm encode header sau khi bảng byte đã đúng.

## Bài nhỏ B

Thiết kế test với byte stream chứa hai frame. Cấp input theo các phần: 1 byte, 3 byte, còn lại; sau đó thử một chunk chứa cả hai frame. Cùng input byte phải tạo cùng thông điệp dù ranh giới đọc khác nhau.

## Bài nhỏ C

Tự viết decoder theo chặng header → validate → payload. Kiểm thử EOF sau 0, 5, 15 byte header và EOF giữa payload. Trường length 0xffffffff phải bị từ chối trước allocation. Invalid version/type/flags có hành vi được định nghĩa.

Codec nên có thể chạy trên stream giả/những đoạn Memory để test mà không cần hai máy. Sau đó nối vào socket đã làm ở bài 02; test codec trong memory không thay integration socket.

Hoàn thành khi T01–T03 pass thật, test fail trả nonzero, và tác giả giải thích được xử lý byte còn dư/thiếu cùng vòng đời payload. Tiếp [Bài 04](04-truyen-tep-va-integrity.md).
