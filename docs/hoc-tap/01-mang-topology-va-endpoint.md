# Bài 01 Vẽ đường đi dữ liệu trước khi viết socket

Đọc Intro PDF 52–55, 63–74, 101–106; IP-address 21–43; Internet Protocol 9–11, 44–50; Socket 2–9. Trang là PDF, không phải footer.

## Địa chỉ ở các lớp

MAC phục vụ chuyển frame trên link; IP định danh interface ở lớp mạng; port giúp đưa dữ liệu tới endpoint ứng dụng. Một connection TCP được phân biệt bằng hai đầu IP/port cùng giao thức. Một peer process có thể có listener nhận kết nối và nhiều socket đã kết nối riêng; accept không biến listener thành socket dữ liệu duy nhất.

`127.0.0.1` là loopback local. Bind `0.0.0.0` cho phép lắng nghe trên các interface phù hợp nhưng địa chỉ này không dùng làm địa chỉ peer công bố cho máy khác. IP riêng có thể dùng trong LAN khi đường đi và firewall cho phép; NAT/router không tự làm một listener sau NAT truy cập được từ Internet.

Tracker là control ở tầng ứng dụng của hệ thống P2P; nó không phải router và không thay OSPF/BGP. Chọn một peer làm nguồn là quyết định ứng dụng, còn đường IP tới peer do mạng/OS quyết định.

## Từ REQUEST_BLOCK đến dây mạng

Scheduler chọn dữ liệu thiếu → codec tạo frame ứng dụng → TCP byte stream → IP datagram → Ethernet/Wi-Fi frame. Chiều nhận làm ngược lại. Khi router thay link kế tiếp, frame lớp 2 thay; nội dung ứng dụng vẫn được xử lý ở host cuối qua TCP.

Tệp 64 MiB có thể chia piece 1 MiB; block 16 KiB là lượng data mỗi yêu cầu. Đừng vẽ 16 KiB block như một Ethernet frame duy nhất. Một receive trả 1000 byte không chứng minh TCP mất 15384 byte còn lại.

## Bài nhỏ A

Vẽ ba process: tracker T, seed A, receiver B. Ghi listener IP/port, socket được accept và kết nối outgoing. Nếu cả ba chạy trên một máy thì listener port nào cần khác nhau? Nếu B sang máy khác, địa chỉ nào phải đổi?

## Bài nhỏ B

Tự điền bảng cho 2 máy trước lab: tên máy, IPv4, prefix/subnet, default gateway, tracker endpoint, peer advertised endpoint. Dùng địa chỉ thực tế của lab, không chép một địa chỉ ví dụ rồi gọi đó là cấu hình máy mình.

Tính thời gian chỉ để đưa 1 MiB lên link 100 Mbit/s theo `8L/R`; giải thích vì sao completion time thực tế còn có RTT, queue, CPU/hash và I/O. Phân biệt link rate với goodput.

Hoàn thành khi tự vẽ/giải thích được một request và một block qua các lớp, rồi phân biệt đúng data/control của P2P. Tiếp [Bài 02](02-socket-TCP-va-async.md).
