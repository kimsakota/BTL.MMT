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
    /// Struct DataPage đại diện cho một trang dữ liệu trong bộ nhớ,
    /// có thể chứa nhiều ô dữ liệu (DataSlot) và quản lý các chỉ số
    /// của chúng.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct DataPage
    {
        /// <summary>
        /// Con trỏ vào địa chỉ bắt đầu của DataPage 
        /// </summary>
        [FieldOffset(0)] public byte* Start;
        /// <summary>
        /// Tổng số lượng hộp DataSlot 
        /// </summary>
        [FieldOffset(8)] public int TotalSlot;
        /// <summary>
        /// Chỉ số của ô dữ liệu trống tiếp theo trong trang dữ liệu.
        /// </summary>
        [FieldOffset(12)] public int FreeSlotIndex;

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataPage(byte* handle, int capacity)
        {
            Start = handle;
            // Mỗi DataSlot chiếm 8 Bytes, nên tổng số ô dữ liệu là capacity / 8
            TotalSlot = capacity >> 3;
            FreeSlotIndex = 0;
        }

        /// <summary>
        /// Hàm này trả về số lượng ô dữ liệu còn trống trong DataPage 
        /// </summary>
        public int Remain
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TotalSlot - FreeSlotIndex;
        }

        /// <summary>
        /// Hàm này trả về kích thước bit của DataPage 
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TotalSlot << 3;
        }

        /// <summary>
        /// Hàm này trả về số lượng bit đã sử dụng trong DataPage 
        /// </summary>
        public int UsedCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FreeSlotIndex << 3;
        }

        /// <summary>
        /// Hàm này trả về con trỏ vào DataSlot trống đầu tiên 
        /// trong DataPage. *ptr = Start + (Số byte đã dùng) 
        /// </summary>
        public byte* FirstFreePointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Start + (FreeSlotIndex << 3);
        }

        /// <summary>
        /// Hàm này trả về con trỏ vào DataSlot tại chỉ số slotIndex 
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetSlotPointer(int slotIndex)
        {
            return Start + (slotIndex << 3);
        }
    }
}
