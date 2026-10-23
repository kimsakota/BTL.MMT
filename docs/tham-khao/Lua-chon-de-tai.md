# Cơ sở chọn Đề 14

Ghi lại kết luận từ giai đoạn khảo sát để giải thích lựa chọn. Phạm vi hiện hành nằm trong [Tổng quan](../Tong-quan.md), các hợp đồng kỹ thuật nằm ở [Thiết kế](../thiet-ke/Kien-truc-du-an.md).

Ban đầu Đề 13 được ưu tiên vì cân bằng khối lượng, socket và thí nghiệm buffer/I/O. Khi Linh bổ sung mục tiêu đề tài khó và áp dụng kiến thức VstHelper rộng hơn, hướng chuyển sang Đề 14. Linh sau đó yêu cầu dựng dự án C# cho nhóm 5 người và xác nhận C# được phép dùng.

## Đề 13 và Đề 14

| Tiêu chí | 13 Client–server | 14 P2P |
|---|---|---|
| Độ khó ước lượng khi tự lập trình đầy đủ | 6,5/10 | 8,5/10 |
| Bài toán chính | Một nguồn phục vụ nhiều client | Mỗi peer vừa nhận vừa chia sẻ, nhiều nguồn |
| Bộ nhớ/I/O | Dễ cô lập tác động buffer và cách ghi | Nhiều consumer, block đến khác thứ tự, giới hạn tài nguyên chung |
| Lỗi | Phiên ngắt, tệp thiếu; resume tùy phạm vi | Thêm nguồn rời, yêu cầu treo, kết quả muộn và giao lại |
| Thí nghiệm | Dễ kiểm soát biến hơn | Thêm upload peer, lựa chọn piece và lịch rời |
| Vai trò Vst | Đào sâu tối ưu đường truyền tệp | Áp dụng rộng ownership, collection, I/O và đồng bộ |

Điểm khó là nhận định tư vấn, không phải thang điểm môn học hoặc phép đo công sức. Đề 13 nâng cao có thể khó hơn một bản P2P tối giản. Mức dùng nguyên source Vst còn phụ thuộc adapter và kiểm chứng runtime; P2P không tự làm thư viện nhanh hơn.

## Đối chiếu danh mục đề

| Đề | Nội dung | Nhận định cho mục tiêu đã chọn |
|---|---|---|
| 01–02 | Pure/Slotted ALOHA | So lý thuyết rõ; ít liên hệ I/O và ownership hiện có |
| 03–04 | CSMA/CD, CSMA/CA | Có chiều sâu lớp truy nhập; cần mô hình thời gian/tranh chấp đúng |
| 05 | STP | Tốt cho lớp liên kết và hội tụ, ít dùng buffer tệp |
| 06 | AODV và DSR | Nặng mô phỏng/định tuyến di động; hướng kiến thức khác Vst |
| 07–08 | DV/LS, OSPF | Phù hợp hướng thuật toán/lab định tuyến |
| 09–10 | TCP flow/congestion control | Mạnh về cơ chế giao vận, có thể nghiên cứu buffer/window |
| 11 | RTP/RTCP | Thêm timestamp, jitter và hạn phát dữ liệu |
| 12 | Chat | Demo nhiều client nhưng thêm tài khoản/nhóm/nhắn riêng |
| 13 | Chia sẻ tệp client–server | Lựa chọn cân bằng và baseline của Đề 14 |
| **14** | **Chia sẻ tệp P2P** | **Đáp ứng mục tiêu khó và kết hợp nhiều kiến thức VstHelper** |
| 15 | WSN | Nặng mô hình năng lượng/lifetime; không gắn trực tiếp hướng hiện tại |

Lộ trình chọn là làm lõi truyền tệp một nguồn đúng trước, dùng lại codec/storage/hash cho P2P rồi bổ sung scheduling, peer rời và kiểm soát bộ nhớ. Hai bản phân tích ban đầu đã được gộp; tài liệu này giữ lý do lựa chọn, không lặp lại toàn bộ thiết kế và kế hoạch đo.
