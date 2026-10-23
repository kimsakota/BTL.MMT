# Bài 04 Tệp một nguồn và dữ liệu đúng

Mục tiêu G2: cùng codec, truyền được tệp một nguồn, có manifest, hash và xử lý cuối tệp. Đây sẽ là baseline client–server cho P2P.

## Piece và offset

Với F byte, piece P byte: số piece bằng F/P cộng một nếu có dư. Tránh công thức cộng F+P−1 khi có thể overflow. Piece i có offset i×P và length=min(P,F−i×P). Block offset là tương đối trong piece; offset tệp cộng hai phần ở 64-bit.

Ví dụ F=1 MiB+3 byte, P=1 MiB, B=16 KiB: có 2 piece, piece cuối 3 byte và chỉ một block 3 byte. F=0 có 0 piece. Tự tính thêm F=B−1, B, B+1, P−1, P, P+1.

## Hash và công bố

Manifest ghi SHA-256 từng piece và toàn tệp từ một input ổn định; không hash một phiên bản rồi upload tệp đã bị sửa. Bản lab chuẩn bị input bất biến hoặc kiểm tra thay đổi trước khi phục vụ. Hash trong manifest là tiêu chuẩn để receiver đối chiếu, không lấy hash từ chính dữ liệu nhận rồi so với nó.

Đường nhận đề xuất: block → writer `.part` đúng offset → completion ghi → khi đủ piece đọc lại để hash → Verified → HAVE. Hết các piece thì kiểm tra toàn tệp và hoàn tất. Cách này giảm nhu cầu giữ toàn tệp trong RAM.

## Bài nhỏ

1. Tự lập bảng piece/block của các kích thước biên, chưa viết mạng.
2. Thiết kế manifest immutable và hàm kiểm tra tính nhất quán file size/hash count.
3. Viết phần đọc/ghi một block với offset và kiểm tra bytes thực tế. Nếu dùng Seek+Write, điều phối cả cặp trên cùng handle.
4. Truyền tệp nhỏ/nhị phân/tệp không chia hết block bằng một nguồn; đối chiếu SHA.
5. Ngắt giữa chừng: file ở trạng thái chưa hoàn tất, có log lỗi, không được đổi thành output thành công.

Các test quan trọng là T04, T05, T08, T09. Hoàn thành khi tất cả kích thước biên đúng và tác giả giải thích được vì sao receive xong không đồng nghĩa Verified. Tiếp [Bài 05](05-tracker-va-discovery.md).
