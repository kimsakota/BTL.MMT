# Dữ liệu thực nghiệm

Chưa có dữ liệu đo. `raw/` bị Git bỏ qua mặc định vì có thể lớn; khi có bộ dữ liệu cuối kỳ, chọn tập cần bàn giao và cập nhật ignore/đóng gói có chủ đích. Không coi ignore là đã nộp dữ liệu.

Một run gồm metadata.json (revision/dirty diff, OS/SDK, NIC/đường truyền, config, seed, input hash, monotonic convention, start/end, exit/status), log từng process, receiver CSV, process metrics và pcap đại diện nếu có. Schema cột gợi ý ở [receiver-columns.csv](receiver-columns.csv) chỉ là header, chưa có một hàng số đo nào.

Status cần phân biệt completed/failed/timeout/departed-as-planned. Tên run ID và đường dữ liệu phải được plot ghi lại. Giữ dữ liệu thô trước khi tổng hợp, không ghi đè run lỗi bằng run tốt.
