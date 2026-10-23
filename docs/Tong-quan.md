# Tổng quan Đề 14 và yêu cầu ET4230

Dự án xây hệ thống chia sẻ tệp P2P qua TCP: một peer có thể tải từ nhiều nguồn và chia sẻ lại các phân mảnh đã xác minh. Hướng nghiên cứu là phân phối dữ liệu khi số peer, khả năng upload và trạng thái kết nối thay đổi; dùng kiến thức VstHelper cho quản lý buffer, I/O và lifetime.

Nguồn: file `ET4230_Bai_tap_lon_Mo_phong_CAP_NHAT_2026.docx` do Linh cung cấp ở Downloads, bản ghi 10/2026. Hồ sơ nguồn ở [Tài liệu tham khảo](tham-khao/README.md). Thông tin mới từ Linh ngày 23/10/2026: dùng C# được phép; nhóm gồm Linh, Minh, Cường, Quân, Tú.

## Ma trận yêu cầu và bằng chứng dự kiến

| ID | Yêu cầu từ đề BTL | Đầu ra dự kiến | Chặng |
|---|---|---|---|
| R01 | 5 sinh viên; duyệt đề, phạm vi, phân công trước triển khai | Danh sách nhóm, bản đề xuất và xác nhận duyệt thực tế | G0 |
| R02 | Peer trao đổi chunk, discovery/tracker tối giản, chọn peer, integrity | Tracker + peer transport + scheduler + hash | G3–G5 |
| R03 | Khảo sát số peer, số chunk/tốc độ upload/peer rời | Config và ma trận thực nghiệm | G6 |
| R04 | So sánh P2P với client–server có kiểm soát | Cùng core truyền tệp, hai mode phân phối và dữ liệu thô | G2, G6 |
| R05 | Tối thiểu 3 kịch bản, 2 tham số có ý nghĩa | Ổn định, upload không đều, peer rời; tách nhóm so sánh | G6 |
| R06 | Chạy socket thực tế, log/gói tin minh chứng | Demo ít nhất hai máy; loopback dùng phát triển | G2, G7 |
| R07 | Source/config/dữ liệu/script/README tái lập | Repo và gói nộp | G7 |
| R08 | Báo cáo PDF khuyến nghị 15–25 trang; video 3–5 phút hoặc minh chứng tương đương; slide | Báo cáo, demo, slide và nhật ký đóng góp | G7 |
| R09 | Không giả dữ liệu; khai báo AI, trích nguồn kế thừa | Provenance, log thật, khai báo hỗ trợ | Mọi chặng |

Với thí nghiệm ngẫu nhiên, đề khuyến nghị từ 5 lần chạy với seed khác nhau, báo cáo trung bình/độ phân tán. Không tự biến khuyến nghị này thành một kết quả đã chạy. Câu quy định về tiến trình/VM/host và máy khác nhau chưa thật rõ; chuẩn bị demo hai máy là phương án chắc chắn, chưa coi hai process trên một laptop là minh chứng cuối kỳ đầy đủ.

## Phạm vi kỹ thuật đề xuất

- C#/.NET 10, console trước, TCP cho control và data, IPv4/LAN cho bản đầu.
- Một tracker discovery; peer vừa download vừa upload piece đã Verified.
- Manifest được chuẩn bị và phân phối cho các peer ngoài băng ở bản cơ sở; file ID là SHA-256 toàn tệp.
- Một tệp trong một phiên kết nối peer; nhiều kết nối để tải nhiều nguồn.
- Chọn tuần tự làm baseline, chọn piece hiếm nhất theo thông tin đang biết làm chính sách so sánh.
- Xử lý ngắt/timeout, giao lại yêu cầu hữu hạn, kiểm tra hash, hàng đợi/bộ nhớ có giới hạn.
- Resume và adapter Vst là chặng sau lõi đúng; nếu đăng ký chúng trong phạm vi được duyệt, phải giữ thành tiêu chí bắt buộc của phạm vi đó.

Đề không yêu cầu tương thích BitTorrent hoàn chỉnh. DHT, NAT traversal Internet, tit-for-tat đầy đủ, mã hóa tự thiết kế, GUI và nhiều tracker không nằm trong bản cơ sở. SHA-256 kiểm tra dữ liệu; thông tin manifest trong lab cần lấy từ nguồn nhóm tin cậy, không tự tuyên bố đã có xác thực hay TLS.

## Rubric và thời hạn

Theo tài liệu: báo cáo 30%; chương trình/mô phỏng 40%; bảo vệ cá nhân 30%. Mỗi người cần giải thích code và dữ liệu của phần mình, cả nhóm hiểu đường đi một tệp. Thời hạn trong đề là trước khi kết thúc học kỳ 3 tuần, lịch cụ thể do giảng viên thông báo. Linh xác nhận ngày 23/10/2026 rằng BTL có **8 tuần làm việc và tuần hiện tại là tuần 1**; đây là cơ sở lập lịch. Chưa có ngày nộp chính thức theo lịch lớp được kiểm chứng. Nhóm theo dõi task và [lịch 8 tuần](Ke-hoach.md); chưa có thông tin bắt buộc nộp báo cáo tuần.

Quy ước tệp nộp là `ET4230_HK[ky]_N[so-nhom]_D14.zip`; kỳ và số nhóm chỉ điền khi lớp công bố. Tên `ET4230.P2P` là tên solution kỹ thuật, không phải mã nhóm chính thức.

## Điều kiện hoàn thành toàn dự án

Mọi R01–R09 có evidence; không còn lỗi sai dữ liệu/lifetime đã biết ở đường chạy được dùng; có chạy hai máy; test và thí nghiệm tái lập từ revision rõ ràng; có so sánh baseline, giải thích giới hạn và đóng góp đủ 5 người. Build bootstrap không đáp ứng các điều kiện này.
