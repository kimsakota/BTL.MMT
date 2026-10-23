# Bài 06 Phân phối dữ liệu và lập lịch nhiều nguồn

Đọc Intro PDF 55, 72–74; Link Layer 39–48; Congestion 3–4 cùng ghi chú đơn vị. Phần ứng dụng tự viết là chọn piece/nguồn và giới hạn request; TCP của OS vẫn xử lý dòng byte.

## Availability và chọn piece

Mỗi peer giữ bitmap các piece nguồn khác công bố. Sequential chọn piece thiếu theo thứ tự ID và chọn nguồn đang có. Rarest-first chọn piece có ít nguồn khả dụng nhất theo thông tin local; bỏ qua piece có 0 nguồn, tie-break dùng random seed đã ghi để tái lập. Nguồn ban đầu có đủ file làm rarity tối thiểu 1 nếu còn online.

Nếu mọi peer mới cùng chọn piece 0, chia sẻ lại ban đầu ít đa dạng. Tie-break/phân tán lựa chọn giúp khảo sát vấn đề này; không khẳng định hiếm nhất luôn nhanh hơn trong mọi môi trường. Khi so hai policy giữ cùng buffer, connection và inflight.

## Pipeline

Một request rồi chờ toàn bộ response mới gửi request tiếp có thể để đường truyền rảnh trong RTT. Cho một số request đang chờ giúp dùng đường truyền tốt hơn, nhưng mỗi request tiêu thụ state/credit/buffer. Hạn mức phải hữu hạn; hết credit thì đợi completion hoặc rollback.

Bài tính: link 100 Mbit/s, RTT 50 ms có BDP khoảng 625000 byte. Tám request ×16 KiB chỉ có 131072 byte payload đang chờ trên connection. Giải thích vì sao đây có thể là giới hạn ứng dụng trong mô hình đơn giản, đồng thời CPU, I/O, cửa sổ TCP và nhiều connection còn ảnh hưởng. Không gọi số request này là cwnd.

## Bài nhỏ theo chặng

1. Cho A có {0,1,2,3}, B có {0}, C có {1,2}. Peer D thiếu tất cả: tự tính rarity và lựa chọn cho sequential/rarest-first.
2. Vẽ block state cho 3 nguồn, tối đa 2 request/connection; mọi lần phát request phải trừ credit, mọi đường cuối phải trả đúng một lần.
3. Ghép upload chỉ phục vụ piece Verified; B chưa có cả tệp vẫn upload piece đã đúng.
4. Chạy 3 peer với sequential và chứng minh có dữ liệu B↔C bằng log/capture; không chỉ ba receiver cùng tải A.
5. Sau đó thêm rarest-first, kiểm thử tie/no-source và cập nhật bitmap khi peer đi/mới vào.

Hoàn thành khi T07/T10 và invariant không double completion đúng, mọi peer nhận đủ file, log chỉ ra nguồn từng piece. Tiếp [Bài 07](07-ownership-buffer-va-Vst.md).
