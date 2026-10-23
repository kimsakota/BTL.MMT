using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    /// <summary>
    /// Struct DataSlot đại diện cho một ô dữ liệu trong bộ nhớ, có thể
    /// chứa nhiều loại dữ liệu khác nhau như con trỏ, số nguyên, số thực,
    /// boolean hoặc thông tin về các chỉ số con.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public unsafe struct DataSlot
    {
        /// <summary>
        /// Đọc toàn bộ 8 Bytes dưới dạng địa chỉ RAM 
        /// </summary>
        [FieldOffset(0)] public byte* PointerValue;
        /// <summary>
        /// Đọc toàn bộ 8 Bytes dưới dạng số nguyên 64-bit
        /// </summary>
        [FieldOffset(0)] public long LongValue;
        /// <summary>
        /// Đọc toàn bộ 8 Bytes dưới dạng số thực 64-bit
        /// </summary>
        [FieldOffset(0)] public double DoubleValue;
        /// <summary>
        /// Đọc 1 Byte đầu tiên dạng boolean (true/false)
        /// </summary>
        [FieldOffset(0)] public bool BooleanValue;

        [FieldOffset(0)] public int FirstChildIndex;
        [FieldOffset(4)] public int LastChildIndex;

        /// <summary>
        /// Hàm này copy trực tiếp chuỗi byte src vào thẳng 
        /// DataSlot, không cần phải tạo một mảng trung gian.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="length"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetString(byte* src, int length)
        {
            fixed (DataSlot* dst = &this)
                Buffer.MemCopy(src, dst, length);
        }

        /// <summary>
        /// Hàm này copy count ô dữ liệu từ con trỏ src vào DataSlot,
        /// sử dụng con trỏ để truy cập trực tiếp vào bộ nhớ.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="count"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(byte* src, int count)
        {
            fixed (DataSlot* dst = &this)
            {
                var s = (long*)src;
                var d = (long*)dst;
                for (int i = 0; i < count; i++)
                    *d++ = *s++;
            }
        }

        /// <summary>
        /// Hàm này nhận một giá trị nguyên thủy (primitive) và 
        /// lưu trữ nó vào DataSlot.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonType SetPrimitive(object v)
        {
            switch (v)
            {
                // Nhóm 64-bit: Dùng con trỏ ép kiểu trực tiếp (Fastest)
                case double d:
                    DoubleValue = d;
                    break;

                case long l:
                    LongValue = l;
                    break;
                
                case ulong ul: 
                    LongValue = (long)ul;
                    break;

                case int i: LongValue = i; break;
                case uint ui: LongValue = ui; break;
                case float f: DoubleValue = f; break;

                case bool b:
                    BooleanValue = b;
                    return JsonType.Boolean;

                default:
                    LongValue = Convert.ToInt64(v);
                    break;
            }
            return JsonType.Number;
        }
    }
}
