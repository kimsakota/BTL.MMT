using System;
using System.Collections.Generic;
using System.Text;

namespace Vst
{
    public enum KeyEntryState : byte {
        /// <summary>
        /// Chưa xác định
        /// </summary>
        None = 0,
        /// <summary>
        /// Bộ nhớ cố định. Không được giải phóng 
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// Thuê tạm từ bộ nhớ khác. Bắt buộc phải trả lại sau khi sử dụng xong 
        /// </summary>
        Rented = 2,
        /// <summary>
        /// Con trỏ mảng byte thô 
        /// </summary>
        Raw = 4,
        /// <summary>
        /// Bộ nhớ nằm trong Heap của Document. Không được giải phóng
        /// </summary>
        DocumentHeap = 8,
    }

    /// <summary>
    /// Hàm này đại diện cho trạng thái của một bản ghi 
    /// trong cơ sở dữ liệu
    /// </summary>
    public enum RecordState : byte
    {
        /// <summary>
        /// Chưa khởi tạo
        /// </summary>
        None,
        /// <summary>
        /// Đã nằm an toàn dưới đĩa .dat
        /// </summary>
        Indexed,
        /// <summary>
        /// Vừa bị sửa trên RAM (chưa lưu xuống đĩa) 
        /// </summary>
        Updated,
        /// <summary>
        /// Vừa mới thêm trên RAM (chưa lưu xuống đĩa)
        /// </summary>
        Inserted,
        /// <summary>
        /// Đã bị xóa 
        /// </summary>
        Deleted = 255,
    }

    /// <summary>
    /// Hàm này đại diện cho chế độ chính của một bản ghi trong 
    /// cơ sở dữ liệu, xác định cách thức lưu trữ và truy xuất 
    /// dữ liệu.
    /// </summary>
    public enum PrimaryMode : byte
    {
        /// <summary>
        /// Gán trực tiếp giá trị vào Key 
        /// </summary>
        Default, // AutoIncrement translated to VietNamese: Mặc định, tự động tăng
        /// <summary>
        /// Số nguyên long 64-bit. Gán trực tiếp vào Key
        /// </summary>
        Fixed,
        /// <summary>
        /// Mã 12-byte Hex (giống MongoDB). Gán trực tiếp vào Key
        /// </summary>
        ObjectId,
        /// <summary>
        /// Chuỗi ngắn <= 16Bytes. Gán trực tiếp vào Key
        /// </summary>
        String = 4,
        /// <summary>
        /// Chuỗi dài > 16Bytes. Dùng XXHash64 và gán vào Key 
        /// </summary>
        LongString = 8,
    }

    public enum JsonType : byte
    {
        None = 0,
        Object = 1,
        Array = 2,
        String = 3,
        Number = 4,
        Boolean = 5,
        Null = 6,
        DateTime = 7,
        ObjectId = 8,
        ObjectKey = 10,
        DocumentKey = 11,

        WhiteSpace = 0x20,
        Comma = 0x2C,
        Colon = 0x3A,

        OpenArray = 0x5B,   // '['
        CloseArray = 0x5D,

        OpenObject = 0x7B,  // '{'
        CloseObject = 0x7D,

        Completed = 128,
        Error = 255,
    }
    public enum JsonNodeState : byte
    {
        Raw = 0,
        Deserialized = 1,
    }
    public enum JsonValueFormat : byte
    {
        None = 0,
        Dot = 1,
        Exponent = 2,
        EscapedString = 4,
    }
}
