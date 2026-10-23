# Báo cáo và demo cuối kỳ

Hướng dẫn bàn giao Đề 14 cho Linh, Minh, Cường, Quân, Tú. Hiện chưa có báo cáo kết quả hoặc video đã nộp. Yêu cầu chính thức và rubric ở [Tổng quan](Tong-quan.md); tiến độ đang làm theo dõi trong [Kế hoạch](Ke-hoach.md).

## Báo cáo PDF

Khuyến nghị 15–25 trang không tính phụ lục theo đề BTL. Cấu trúc nội dung:

1. **Bìa và nhóm:** ET4230, kỳ/số nhóm thực tế, tên đề, đủ họ tên/MSSV, giảng viên và ngày nộp.
2. **Vấn đề và mục tiêu:** phân phối tệp nhiều nguồn, điều kiện lab, câu hỏi nghiên cứu và phạm vi được duyệt.
3. **Cơ sở môn học:** CS/P2P, phân lớp, TCP/socket, RTT/window/goodput; dẫn tên slide và trang PDF.
4. **Thiết kế:** topology, tracker/peer, manifest/piece/block, giao thức, state machine, ownership và giới hạn tài nguyên.
5. **Hiện thực:** module đã có source, quyết định C#/.NET, phần Vst sử dụng/điều chỉnh và provenance; tách tính năng đã chạy khỏi thiết kế dự kiến.
6. **Kiểm thử:** ca biên, EOF/cancel, hash sai, peer rời/resume theo phạm vi và minh chứng hai máy.
7. **Thiết kế thực nghiệm:** baseline, biến khảo sát, tham số cố định, môi trường, lặp/seed, định nghĩa metric và cách xử lý run lỗi.
8. **Kết quả:** bảng/biểu đồ từ dữ liệu thô, độ biến thiên, giải thích bất thường và nút thắt.
9. **Giới hạn và kết luận:** điều kết quả chứng minh được trong môi trường đã đo, phần chưa làm và hướng tiếp theo.
10. **Nguồn và phụ lục:** source kế thừa, khai báo AI hỗ trợ, hướng dẫn tái lập, đóng góp từng thành viên và evidence.

Có thể dành khoảng 2 trang mở đầu, 3 trang nền tảng, 4 trang thiết kế, 4 trang hiện thực/kiểm thử, 6 trang thực nghiệm, 1–2 trang kết luận; điều chỉnh theo kết quả thật. Mỗi biểu đồ phải truy được tới CSV, script, config và revision. Thành viên xác nhận đóng góp của mình; phân công đề xuất không tự trở thành thành tích.

## Demo 3–5 phút

| Thời lượng gợi ý | Nội dung | Minh chứng |
|---|---|---|
| 30 s | Tên đề, topology, hai máy và điều kiện | Sơ đồ, bảng endpoint |
| 45 s | Tracker/seed/receiver sẵn sàng, manifest | Log role/connection/file ID |
| 60 s | Tải nhiều nguồn và chia sẻ piece đã xác minh | Nguồn từng block/piece, pcap đại diện |
| 45 s | Ngắt một peer theo lịch, thấy giao lại | Disconnect/timeout/reassign |
| 45 s | Hash đúng, kết quả và giới hạn | Completion status, hash, một biểu đồ thật |

Chỉ trình diễn tính năng đã chạy; chuẩn bị cấu hình nhỏ, command đúng revision và log/video dự phòng. Mỗi người cần giải thích API, ownership, một failure path, test phần mình và đường đi một tệp.

## Trước khi nộp

- [ ] Đủ 5 họ tên/MSSV và đóng góp đã xác nhận.
- [ ] README clone/build/run tái lập, dependency/version rõ; thử ở thư mục sạch.
- [ ] Source/config/test/evidence đúng phạm vi được duyệt.
- [ ] Có ít nhất 3 kịch bản, 2 tham số hoặc phương án tương đương được phê duyệt.
- [ ] Có so sánh client–server/P2P và công bố tài nguyên upload bổ sung.
- [ ] Biểu đồ có đơn vị, chú giải, số mẫu/độ phân tán; dữ liệu và script dựng lại.
- [ ] Có minh chứng socket thực tế theo yêu cầu; hash và trạng thái đúng.
- [ ] Nguồn slide/Vst/AI được khai báo, kết quả và đóng góp phản ánh việc thực.
- [ ] Bỏ bin/obj/tmp, VM và dữ liệu lớn không cần; kiểm tra nội dung gói nộp.
- [ ] Tên zip theo kỳ/số nhóm thật; deadline và lịch bảo vệ được xác nhận.
