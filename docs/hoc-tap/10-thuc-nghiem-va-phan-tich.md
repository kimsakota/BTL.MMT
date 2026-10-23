# Bài 10 · Từ số đo đến một kết luận có giới hạn

**Học trước:** test/integration ở Bài 09; throughput ở Bài 01; hiểu cơ chế đang so. **Mục tiêu:** tự lập một cặp đối chứng, tính metric đúng denominator và phân biệt quan sát với suy luận.

Slide: `01 Introduction.pdf` trang PDF 63–74; `04-2 Congestion control.pdf` trang 3–13, 22–37; `03-6 Routing Protocol for MANET.pdf` trang 23–25 chỉ như ví dụ chọn biến/metric. Không nhập giả định MANET vào lab LAN. [Thiết kế thực nghiệm](../kiem-chung/Thiet-ke-thuc-nghiem.md) và [experiments.json](../../configs/experiments.json) là kế hoạch hiện hành, chưa có run đo.

**Mọi dãy số tính bên dưới là ví dụ giả định để học công thức, không phải kết quả của repo.** Không chép chúng thành CSV/biểu đồ kết quả nhóm.

## 1. Câu hỏi nghiên cứu phải có thể bị trả lời ngược kỳ vọng

“P2P tốt hơn” quá mơ hồ. Một câu rõ hơn: với cùng seed ban đầu, tệp, số receiver và môi trường, thời gian hoàn tất thay đổi thế nào khi receiver được phép upload cho nhau?

Nếu P2P chậm hơn vẫn là kết quả có giá trị khi phép đo đúng. Có thể do uplink chia sẻ, hash/đĩa, quá nhiều connection, piece cuối ít nguồn hoặc implementation. Không sửa dữ liệu để khớp tên đề tài.

Các câu hỏi chính: CS/P2P; sequential/rarest-first; buffer strategy managed/Vst trong phạm vi có implementation đủ kiểm chứng. Mỗi câu cần nhóm so sánh riêng trước khi làm nhiều tổ hợp.

## 2. Biến độc lập, metric và biến phải giữ cố định

| Loại | Nghĩa | Ví dụ so scheduler |
|---|---|---|
| Biến độc lập | Thứ chủ động thay để xem tác động | Sequential hay rarest-first |
| Metric | Thứ đo để đánh giá | Completion time, source upload |
| Biến kiểm soát | Thứ giữ như nhau trong cặp | File, N, block/piece, cap, buffer, I/O policy |
| Yếu tố nhiễu | Thứ có thể ảnh hưởng ngoài mục tiêu | Wi-Fi tải khác, cache, ứng dụng nền |

Đổi đồng thời scheduler, block size, allocator và flush rồi thấy nhanh hơn không tách được nguyên nhân. Trước hết chốt câu hỏi và một cặp cấu hình chỉ khác yếu tố cần khảo sát.

Không nhất thiết thử mọi tổ hợp. Ma trận quá lớn có thể ăn hết thời gian 8 tuần mà số liệu vẫn khó giải thích. Giữ tối thiểu yêu cầu môn học và chọn các giá trị có ý nghĩa qua calibration.

## 3. P2P dùng thêm tài nguyên phải được nói rõ

Baseline CS: chỉ seed upload. P2P: receiver upload thêm khi có piece đúng. Nếu tổng thời gian giảm, không kết luận chỉ do thuật toán “thông minh hơn” mà bỏ qua upload bổ sung.

Một nhóm so sánh có thể giữ cùng seed và công bố upload từng receiver. Nhóm phụ, nếu có thời gian, có thể giữ tổng upload budget tương đương. Hai nhóm trả lời câu khác nhau.

Giới hạn payload upload trong app không phải link capacity được shaping. Trong Wi-Fi nhiều peer có thể chia sẻ airtime; N tăng không có nghĩa thêm N đường độc lập. Loopback cùng máy cũng không đại diện đầy đủ cho LAN hai máy.

## 4. Tự tính metric và denominator

Tệp 64 MiB, hoàn tất sau 10 s, theo cửa sổ gồm ghi/hash đã chọn:

```text
goodput = 64 × 1,048,576 × 8 / (10 × 1,000,000)
        = 53.6870912 Mbit/s
```

Nếu đếm cả BLOCK gửi lặp, đó là lưu lượng ứng dụng, không phải số byte tệp hữu ích mới. Completion time cần định nghĩa mốc start/end và có tính setup/hash hay không; giữ cùng định nghĩa giữa các cặp.

Ví dụ hai receiver cùng tải tệp 64 MiB: B hoàn tất 8 s, C hoàn tất 16 s. Makespan=16 s. Goodput phân phối theo tổng hữu ích/makespan là 128 MiB×8/16/1e6≈67.11 Mbit/s. Nó **không bằng** cộng goodput riêng 67.11+33.55≈100.66 Mbit/s vì denominator khác nhau.

Không trừ hai wall-clock khác máy chưa được đồng bộ rồi gọi đó là duration chính xác. Runner dùng đồng hồ của mình và quy ước completion event có thể thêm độ trễ báo về; phải nêu giới hạn ấy.

## 5. Run lỗi và peer rời không được biến mất khỏi báo cáo

Ví dụ N=5 gồm 1 seed và 4 receiver. Một receiver rời theo lịch; trong 3 receiver còn được yêu cầu hoàn tất, 2 completed và 1 timeout.

- Tỉ lệ completed trên tập receiver ban đầu: 2/4.
- Tỉ lệ completed trên tập còn phải hoàn tất sau departure: 2/3.
- Planned departure và timeout là hai trạng thái khác nhau.

Chỉ lấy hai run/receiver completed để tính thời gian trung bình mà không báo thất bại sẽ làm kết luận thiên lệch. Cũng không tự gán timeout=0 hoặc coi deadline là thời gian hoàn tất thật. Giữ status và denominator rõ.

## 6. Vì sao phải lặp và lưu seed?

Một lượt có thể gặp scheduling/cache/network thuận lợi. Lặp giúp thấy biến thiên. Config đề xuất 5 lần/cấu hình và các random seed khác nhau; đó là kế hoạch, chưa có dữ liệu.

Ví dụ học công thức với dãy giả định 10,12,8 s:

```text
mean = (10+12+8)/3 = 10 s
sample variance = ((10-10)^2+(12-10)^2+(8-10)^2)/(3-1) = 4 s²
sample standard deviation = sqrt(4) = 2 s
```

Min=8, max=12. Standard deviation không tự là khoảng tin cậy 95%. Error bar phải ghi đang biểu diễn gì, n là bao nhiêu. N nhỏ không đủ để khẳng định chênh lệch rất nhỏ luôn có ý nghĩa.

Seed tái lập một nguồn ngẫu nhiên trong cùng điều kiện, không điều khiển mọi thread/network timing. Lưu seed cùng revision, config, input hash, máy và raw events. Có thể đảo thứ tự A/B giữa các lượt để giảm ảnh hưởng thứ tự/cache; policy warm-up/cache phải được công bố.

## 7. Một lượt đo đáng tin gồm các bước nào?

1. Correctness và ca lỗi chính đã đạt; chọn revision/config cụ thể.
2. Calibration chọn kích thước tệp/thời gian đủ dài, tránh phiên kết thúc trước thời điểm fault định sẵn. Tách pilot khỏi số đo cuối nếu cấu hình khác.
3. Tracker/peer sẵn sàng → runner phát GO và ghi start theo cùng quy ước.
4. Thu log từng process, counters, status, hash và metrics; áp dụng departure đã ghi lịch.
5. Dừng/cleanup có kiểm soát; lưu run nguyên bản kể cả failed/timeout.
6. Chạy lặp, tổng hợp và dựng plot từ raw data bằng script, không nhập tay số đẹp.

Metadata một run cần revision/dirty diff, OS/SDK, topology/NIC, config, input SHA, seed, start/end convention, command và exit/status. Không dùng tên “run1” đơn lẻ khi không truy được cấu hình.

## 8. Đọc một biểu đồ bằng ba câu riêng

- **Quan sát:** metric nào đổi bao nhiêu trong các mẫu đã đo?
- **Suy luận:** cơ chế nào có thể giải thích, có log/counter phụ hỗ trợ không?
- **Giới hạn:** môi trường/biến chưa tách hoặc số mẫu khiến kết luận dừng ở đâu?

Ví dụ câu mẫu không chứa số giả: “Trong các run được báo, mode P2P có makespan thấp hơn; receiver upload tăng theo log. Kết quả trên topology LAN này chưa chứng minh cùng mức giảm trên Wi-Fi/Internet.” Chỉ dùng câu đó khi dữ liệu thật hỗ trợ.

Vst giảm managed allocation nhưng goodput không đổi vẫn có thể hợp lý nếu link/đĩa là bottleneck. Pool có thể giữ RAM dự trữ, nên allocation giảm không đồng nghĩa peak working set giảm. So cùng I/O policy, block size và workload trước khi quy kết.

## 9. Bài tập tăng dần

**A.** Tính goodput tệp 32 MiB hoàn tất 8 s. Nêu đơn vị và byte nào được đếm.

**B.** Muốn so ArrayPool/Vst nhưng đổi cả block 16 KiB thành 4 KiB ở một bên. Vì sao phép so chưa cô lập allocator? Thiết kế lại cặp.

**C.** Lập bảng biến cho một câu hỏi, chưa chạy app. Thêm cách ghi failed/planned-departed và dữ liệu cần lưu.

**D.** Khi có run thật, viết script đọc schema/status, tạo một plot và ba câu quan sát/suy luận/giới hạn. Không tự điền thiếu thành0.

<details>
<summary>Gợi ý A–B</summary>

A: 32×1048576×8/(8×1000000)=33.554432 Mbit/s theo định nghĩa đã chọn. B: block đổi làm request count, pipeline và I/O đổi. Chọn cùng size hợp lệ cho cả hai, giữ các policy còn lại; nếu khảo sát size thì làm thành nhóm so sánh riêng, không gộp kết luận allocator.

</details>

Hiểu bài khi biết dựng phép so có thể thất bại giả thuyết ban đầu và đọc metric không lẫn denominator. Tiếp [Bài 11](11-bao-cao-demo-va-bao-ve.md).
