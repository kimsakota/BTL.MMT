# Quy tắc ghi nhớ

CURRENT_STATE giữ sự thật hiện hành, LOGBOOK giữ lý do/mốc, RECOVERY_NOTES giữ bước tiếp theo, UPSTREAM giữ nguồn kế thừa; chi tiết kỹ thuật ở docs. Ghi sau quyết định, kiểm tra hoặc đổi chặng có ý nghĩa, không tạo checkpoint cho mọi lần đọc file.

Phân biệt yêu cầu user, đề xuất assistant, chức năng có source, compile, test, benchmark và bài học đã được người dùng xác nhận. Mọi môi trường/test cần ngày, lệnh và phạm vi. Không tự tick lesson, gán việc nhận hoặc góp code cho thành viên.

Chỉ xuất transcript khi được yêu cầu, không xuất system/developer/reasoning/tool outputs hoặc thu gom chat khác. Giữ raw transcript ngoài Git. Không có tự động đồng bộ nền.
