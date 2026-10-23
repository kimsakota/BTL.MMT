# Bài 00 · Từ một file C# đến chương trình đang chạy

**Câu hỏi:** vì sao có source nhưng chưa có chức năng P2P, và vì sao build thành công vẫn chưa nói được chương trình chạy đúng?

Chưa cần biết socket. Nếu chưa hiểu biến/hàm/reference, mở [Nền tảng C#](CSharp-nen-tang.md) song song. Bài này giải thích mô hình rồi mới hướng dẫn thao tác; chi tiết cài đặt ở [Môi trường](../trien-khai/Thiet-lap-moi-truong.md).

## 1. Phân biệt năm thứ đang nhìn thấy

| Thứ | Là gì? | Ví dụ trong repo |
|---|---|---|
| Source | Văn bản mô tả hành vi, compiler đọc | Program.cs |
| Project | Đơn vị build với tùy chọn và dependency | ET4230.Cli.csproj |
| Solution | Tập project để build/IDE điều phối | ET4230.P2P.sln |
| Assembly | Đầu ra .NET chứa mã và metadata | ET4230.Core.dll |
| Process | Một lần chương trình đang thực thi, có bộ nhớ/tài nguyên riêng | Một lần chạy CLI |

Một solution không phải một server. Một DLL không nhất thiết là thư viện không thể chạy: executable .NET cũng thường có assembly .dll kèm cơ chế host. Điều phân biệt ở project này là Core không có entry point, còn Cli có entry point.

```text
C# source + csproj + references
             ↓ compiler/build
       assembly + metadata
             ↓ .NET runtime
      process thực thi chương trình
```

SDK chứa công cụ phục vụ phát triển/build; runtime phục vụ thực thi .NET. Compiler thường sinh IL và metadata, runtime có thể JIT thành mã máy khi chạy. Không cần hiểu JIT nội bộ để bắt đầu, nhưng phải biết một câu lệnh C# chưa tự là byte chạy trên CPU. Đối chiếu [tổng quan .NET](https://learn.microsoft.com/en-us/dotnet/core/introduction).

## 2. Vì sao có ba project?

Hướng phụ thuộc hiện tại: **Cli → Core**. VstReference đứng riêng để biên dịch snapshot; Cli/Core chưa gọi nó. Core sẽ chứa giao thức, transport, storage…; Cli sẽ nhận lệnh/cấu hình và điều phối.

`ProjectReference` cho build biết phải tham chiếu assembly nào. `using Some.Namespace;` chỉ giúp tìm tên kiểu trong source; nó không tự tải một repo hay thêm project reference. Nếu có class tên giống nhau trong hai namespace, đó cũng không tự là cùng kiểu.

Ví dụ: thêm `using Vst;` vào Cli không khiến một hàm Win32 chạy. Cần assembly phù hợp, lời gọi thực sự và môi trường runtime đáp ứng API đó. VstReference build thành công chỉ là bằng chứng biên dịch wrapper, chưa phải bằng chứng allocator an toàn.

Core hiện chỉ là project thư viện khởi tạo. Compiler có thể build thư viện chưa có nghiệp vụ; vì vậy thông báo Build succeeded không chứng minh đã có peer/tracker.

## 3. Process, thread và Task khác nhau

- **Process:** một chương trình đang chạy, có không gian địa chỉ và tài nguyên riêng.
- **Thread:** dòng thực thi được hệ điều hành lập lịch trong process; nhiều thread có thể cùng truy cập dữ liệu process.
- **Task:** biểu diễn một thao tác trong .NET, có thể đang chờ I/O; không tương ứng cố định với một thread.

Chạy cùng CLI hai lần tạo hai process khác nhau. Tạo một Dictionary trong process A không làm process B nhìn thấy Dictionary ấy. Muốn trao đổi phải có cơ chế như socket và biểu diễn dữ liệu thành byte. Đây là lý do cần giao thức, không chỉ cần cùng định nghĩa class.

Một peer process có thể vừa chờ kết nối, vừa nhận từ nguồn A, vừa ghi đĩa. Điều đó đòi hỏi quản lý đồng thời và lifetime; không có nghĩa phải tạo một thread cho mỗi byte hoặc mỗi block.

## 4. Build và run qua một ví dụ thật của bộ khởi tạo

Mở terminal ở thư mục chứa solution:

```powershell
dotnet --info
dotnet build ET4230.P2P.sln --configuration Debug
dotnet run --project src/ET4230.Cli -- --help
dotnet run --project src/ET4230.Cli -- --version
dotnet run --project src/ET4230.Cli -- unknown-command
$LASTEXITCODE
```

Đây là lệnh để **người học tự chạy**. Hành vi bootstrap dự kiến: help/version exit 0; lệnh không hỗ trợ exit 2. Sau `--`, đối số được chuyển cho chương trình; trước nó là đối số của dotnet. Nếu một lệnh khác chạy trước khi xem LASTEXITCODE, giá trị có thể đã đổi.

Build đọc cấu hình từ csproj và Directory.Build.props, tạo đầu ra trong bin/obj. Run mặc định có thể build trước khi chạy; `--no-build` chỉ dùng đầu ra đã có, nên có thể chạy mã cũ nếu vừa sửa source mà chưa build. Không dùng no-build để bỏ qua lỗi build.

Build incremental có thể không biên dịch lại source Vst và không in lại warning cũ. Lượt build sạch trước có 10 CS8500 được ghi trong audit; một lượt incremental không warning không tự làm các vấn đề đó biến mất.

## 5. Đọc Program.cs như một luồng quyết định

Không học bằng cách đoán từ tên project. Mở [Program.cs](../../src/ET4230.Cli/Program.cs) và tự đi từng nhánh:

1. Một đối số bằng version → in version, kết thúc mã 0.
2. Không đối số, hoặc một đối số help → in hướng dẫn bootstrap, kết thúc mã 0.
3. Các trường hợp còn lại → in lỗi, kết thúc mã 2.

Stdout dùng cho output bình thường; stderr dùng cho chẩn đoán. Exit code là dữ liệu để terminal/runner/CI nhận biết thành công hay thất bại, không chỉ là dòng chữ trên màn hình. Một chương trình in “failed” nhưng trả 0 có thể làm CI tưởng đã đạt.

Các lệnh tracker/peer/send chưa có ở source hiện tại. Trong bài sau, pseudocode mô tả điều nhóm sẽ tự xây; không nhập một lệnh thiết kế rồi ghi rằng app đã chạy được.

## 6. Thực hành tăng dần

**A · Nhận diện, chưa sửa code.** Tìm entry point, project reference, target framework, nơi lưu vector giao thức. Vẽ ba project và chỉ mũi tên phụ thuộc đang có.

**B · Dự đoán rồi chạy.** Với không args, help, version, hai args và unknown-command, viết output category và exit code dự kiến; chạy từng ca để đối chiếu. Đây là thí nghiệm của máy người học, không copy log máy khác.

**C · Giải thích tài nguyên.** Phân loại một byte[], một Socket và vùng nhớ do thư viện native cấp phát. Cái nào GC quản lý object? Cái nào còn cần Dispose/Return theo hợp đồng? Nếu chưa rõ, đọc mục 3/6/7 của nền C#.

**D · Tự làm bài nhỏ khi Linh giao.** Viết một hàm tính kích thước piece cuối, chỉ trong bài thực hành phù hợp. Tự nêu input, output và input sai trước khi nghĩ đến socket. Chưa tạo cả cây class trống.

## 7. Câu hỏi tự kiểm tra có gợi ý

1. Hai lần chạy cùng assembly có cùng Dictionary không?
2. Vì sao Core chưa có tính năng vẫn build được?
3. Nếu help chạy được, có chứng minh Windows native I/O chạy được không?
4. Vì sao dùng namespace đúng mà compiler vẫn không tìm thấy kiểu?

<details>
<summary>Đối chiếu cách giải thích</summary>

1. Không; process riêng, trừ khi có cơ chế chia sẻ rõ. 2. Build kiểm tra/biên dịch source hiện có, không tự biết các chức năng đề tài còn thiếu. 3. Không; đường help hiện không gọi Vst. 4. Namespace không thay assembly reference, khả năng truy cập của kiểu hoặc lỗi tên. Phải kiểm tra dependency thực tế.

</details>

Bạn hiểu bài khi tự chỉ được source → assembly → process, đọc được một nhánh return và phân biệt build/run/test. Tiếp [Bài 01](01-mang-topology-va-endpoint.md); kiến thức thiếu được học trong [Nền tảng C#](CSharp-nen-tang.md), không bỏ qua vì đã mở được solution.
