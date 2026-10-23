using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Vst
{
    /// <summary>
    /// Struct Buffer 16 Bytes đại diện cho một khối bộ nhớ liên tục, cung cấp 
    /// các phương thức để thao tác trực tiếp với dữ liệu trong bộ 
    /// nhớ mà không cần tạo bản sao.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct Buffer
    {
        /// <summary>
        /// Con trỏ trực tiếp đến khối bộ nhớ
        /// </summary>
        [FieldOffset(0)] public byte* Handle;
        /// <summary>
        /// Kích thước của khối bộ nhớ
        /// </summary>
        [FieldOffset(8)] public int Length;
        /// <summary>
        /// Vị trí của con trỏ hiện tại trong khối bộ nhớ 
        /// </summary>
        [FieldOffset(12)] public int Iterator;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Buffer(byte* handle, int length)
        {
            Handle = handle;
            Length = length;
            Iterator = 0;
        }

        /// <summary>
        /// Hàm so sánh trực tiếp dữ liệu trong bộ nhớ với một 
        /// con trỏ khác mà không cần tạo bản sao.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RawEquals(byte* handle, int length)
        {
            if (length != Length) return false;

            byte* p = Handle;

            while (length >= 8)
            {
                if (*(long*)p != *(long*)handle) return false;

                p += 8;
                handle += 8;
                length -= 8;
            }
            while (length >= 4)
            {
                if (*(int*)p != *(int*)handle) return false;

                p += 4;
                handle += 4;
                length -= 4;
            }
            while (length > 0)
            {
                if (*p != *handle) return false;
                p++; handle++; length--;
            }
            return true;
        }

        /*// So sánh 32 Bytes trong đúng 1 lệnh CPU AVX2 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RawEqualsAVX2(byte* ptr1, byte* ptr2)
        {
            
        }*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RawEquals(byte* handle) => RawEquals(handle, Length);

        /// <summary>
        /// Hàm dùng để dịch chuyển con trỏ Handle đi một khoảng offset mà không thay đổi Length.
        /// </summary>
        /// <param name="offset"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shift(int offset) => Handle += offset;

        /// <summary>
        /// Hàm này dùng để cắt bỏ một phần đầu của bộ nhớ, giảm Length đi offset và dịch chuyển Handle đi offset.
        /// </summary>
        /// <param name="offset"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Slice(int offset)
        {
            Handle += offset;
            Length -= offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Encoding.UTF8.GetString(Handle, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToHex()
        {
            // Thuê một bộ nhớ tạm thời để lưu trữ chuỗi hex
            var slot = Memory.Rent(Length * 3);

            // Sử dụng StringWriter để viết dữ liệu hex vào bộ nhớ tạm thời
            var w = new StringWriter(slot.RawData, slot.Capacity);
            for (int i = 0; i < Length; i++)
            {
                byte b = Handle[i];
                w.Write((byte)HexHelper.GetHexDigit((uint)b >> 4));
                w.Write((byte)HexHelper.GetHexDigit((uint)b & 15));
                w.Write(' ');
            }
            var s = Encoding.ASCII.GetString(slot.RawData, w.CurrentPosition);
            Memory.Return(slot.RawData);
            return s;
        }

        #region Statics

        /// <summary>
        /// Hàm này dùng để sao chép dữ liệu từ một con trỏ nguồn (src) sang một con trỏ đích (dst) với độ dài xác định (length).
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="length"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCopy(void* src, void* dst, long length)
        {
            System.Buffer.MemoryCopy(src, dst, length, length);
        }
        
        #endregion
    }
}
