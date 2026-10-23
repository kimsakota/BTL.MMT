# Bài 11 Bàn giao và bảo vệ cá nhân

Mục tiêu: biến source/config/evidence thành sản phẩm nộp đúng yêu cầu. Báo cáo cuối và video chưa được tạo ở bộ khởi tạo; [hướng dẫn báo cáo và demo](../Bao-cao-va-demo.md) xác định nội dung cần chuẩn bị từ kết quả thật.

## Báo cáo

Trình bày vấn đề, kiến thức ET4230, thiết kế/giao thức, implementation, test, thí nghiệm, kết quả và giới hạn. Khuyến nghị 15–25 trang không tính phụ lục theo đề. Những module/chức năng chưa làm phải ghi đúng phạm vi; không đưa sơ đồ đích vào mục “đã triển khai” nếu source chưa có.

Mỗi hình/bảng có đơn vị, cấu hình, số lần chạy và nguồn raw data. Tài liệu kế thừa/AI có khai báo. Linh, Minh, Cường, Quân, Tú tự xác nhận phần đóng góp và khả năng giải thích; không lấy tên trong bảng phân công đề xuất làm evidence.

## Demo 3–5 phút đề xuất

30 s giới thiệu topology → 45 s tracker/seed/peer ready → 60 s tải và chia sẻ nhiều nguồn → 45 s ngắt một peer, thấy giao lại → 45 s hash và kết quả/giới hạn. Chỉ diễn phần đã chạy thật, giữ phương án log/pcap/video dự phòng; đừng bịa kết quả để bám kịch bản thời gian.

## Câu hỏi từng người phải trả lời

Giải thích một frame từ header tới BLOCK; vì sao receive có thể thiếu; ai sở hữu buffer khi đang await/ghi; khi nào piece Verified; mất nguồn cuối thì kết quả gì; peer selection khác routing IP thế nào; số đo nào chứng minh kết luận và giới hạn ở đâu.

Người phụ trách module phải chỉ ra API/caller, test lỗi và một quyết định thiết kế có trade-off. Người làm harness phải giải thích denominator, random seed và thu log, không chỉ mở biểu đồ.

## Kiểm tra gói nộp

Clone sạch ở thư mục khác, theo README build/chạy lại, chọn một test/cấu hình nhỏ bảo vệ được. Nộp source/config/report/data/scripts cần thiết, video hoặc minh chứng tương đương, slide và nhật ký. Không đóng gói bin/obj, VM lớn, toàn bộ slide gốc hoặc dữ liệu không cần thiết. Tên kỳ/nhóm chỉ điền khi có số thực tế.

Hoàn thành G7 khi checklist yêu cầu R01–R09 có evidence, đóng góp được xác nhận và người khác tái lập được bản demo đã chọn.
