using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Dòng này cho phép các thành phần nội bộ của assembly Vst được truy cập từ assembly "Test". Điều này hữu ích khi bạn muốn viết các bài kiểm tra đơn vị (unit tests) cho các thành phần nội bộ mà không cần phải thay đổi phạm vi truy cập của chúng.
[assembly: InternalsVisibleTo("Test")]

namespace Vst
{
    public unsafe interface IMemoryHelper
    {
        byte* Alloc(int length);
        byte* Realloc(void* handle, int newLength, int oldLength);
        void Free(void* handle);
    }
}

namespace Vst
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal unsafe struct MemorySlotHeader
    {
        // Là vị trí của slot trong trang bộ nhớ (MemoryPage) mà nó thuộc về. SlotIndex được sử dụng để xác định vị trí của slot trong mảng các slot của trang bộ nhớ.
        [FieldOffset(0)] internal int SlotIndex;
        // Chỉ số phân loại kích thước
        [FieldOffset(4)] internal int BinIndex;
        // Trỏ ngược lại MemoryPage. Thuận tiện cho việc trả lại slot vào trang bộ nhớ ban đầu.
        [FieldOffset(8)] internal MemoryPage* PageHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MemorySlot
    {
        /// <summary>
        /// Con trỏ đến byte đầu tiên của khối bộ nhớ (MemorySlot) mà slot này quản lý
        /// </summary>
        public byte* RawData;

        /// <summary>
        /// Kích thước của khối bộ nhớ (MemorySlot) mà slot này quản lý
        /// </summary>
        public int Capacity;

        /// <summary>
        /// Hàm này lấy địa chỉ của MemorySlotHeader. Trừ đi sizeof(MemorySlotHeader) từ 
        /// con trỏ handle để lấy địa chỉ của header. Header chứa thông tin về slot, bao 
        /// gồm SlotIndex, BinIndex và PageHandle.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemorySlotHeader* GetSlotHeader(byte* handle) => (MemorySlotHeader*)(handle - sizeof(MemorySlotHeader));

        /// <summary>
        /// Hàm này tính toán kích thước của một khối nhớ (MemorySlot) bao gồm cả header 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetSlotBlockSize(int size) => size + sizeof(MemorySlotHeader);

        /// <summary>
        /// Hàm này tạo một MemorySlot mới từ một con trỏ đến khối bộ nhớ (blockPtr) và 
        /// kích thước (size).
        /// </summary>
        /// <param name="blockPtr"></param>
        /// <param name="size"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MemorySlot(byte* blockPtr, int size)
        {
            RawData = blockPtr + sizeof(MemorySlotHeader);
            Capacity = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Clear()
        {
            //long* end = (long*)(RawData + Capacity);
            //long* p = (long*)RawData;
            //while (p < end) *p++ = 0;
        }

        /// <summary>
        /// Hàm này di chuyển về ô nhớ trước đó trong trang bộ nhớ (MemoryPage)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void MoveBack() => RawData -= (Capacity + sizeof(MemorySlotHeader));

        /// <summary>
        /// Hàm này di chuyển sang ô nhớ tiếp theo trong trang bộ nhớ (MemoryPage)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void MoveForward() => RawData += (Capacity + sizeof(MemorySlotHeader));

        // Hàm này giúp lấy thông tin header của slot hiện tại, bao gồm SlotIndex, BinIndex và PageHandle.
        internal MemorySlotHeader* Header
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetSlotHeader(RawData);
            }
        }

        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => *(RawData + index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => *(RawData + index) = value;
        }
    }
}

namespace Vst
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    internal unsafe struct MemoryPage : IDisposable
    {
        // Kích thước mặc định của một trang bộ nhớ (MemoryPage) là 64 slot. Mỗi slot có thể chứa một khối bộ nhớ (MemorySlot) với kích thước xác định bởi SlotSize.
        internal static int Capactity { get; set; } = 64;

        // Chỉ số phân loại kích thước của các slot trong trang bộ nhớ 
        [FieldOffset(0)] internal int BinIndex;
        // Kích thước của mỗi slot tính theo BinIndex. Vd: BinIndex 0 -> SlotSize = 16 bytes, BinIndex 1 -> SlotSize = 32 bytes, ...
        [FieldOffset(4)] internal int SlotSize;
        // Sử dụng long để lưu trữ số lượng slot trống 
        [FieldOffset(8)] internal long FreeSlots;
        // Con trỏ vào đầu của khối dữ liệu (DataHandle) mà trang bộ nhớ quản lý 
        [FieldOffset(16)] internal byte* DataHandle;

        /// <summary>
        /// Hàm này kiểm tra xem còn slot trống trong trang bộ nhớ hay không. 
        /// Nếu FreeSlots khác 0, có nghĩa là còn slot trống và trả về true, ngược lại trả về false.
        /// </summary>
        internal bool IsAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FreeSlots != 0;
        }

        /// <summary>
        /// Hàm này kiểm tra xem trang bộ nhớ đã đầy chưa. Nếu FreeSlots bằng 0, 
        /// có nghĩa là tất cả các slot đã được thuê và trả về true, ngược lại trả về false.
        /// </summary>
        internal bool IsFull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FreeSlots == 0;
        }

        /// <summary>
        /// Hàm này kiểm tra xem trang bộ nhớ có còn slot trống hay không
        /// </summary>
        internal bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FreeSlots == -1L;
        }

        /// <summary>
        /// Hàm này chọn slot 0 trong trang bộ nhớ mà không cần kiểm tra xem còn slot trống hay không.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemorySlot RentDirect()
        {
            const int bitIdx = 0;
            long mask = 1L << bitIdx;

            FreeSlots &= ~mask;

            var slot = new MemorySlot(DataHandle, SlotSize);
            slot.Clear();

            return slot;
        }

        /// <summary>
        /// Hàm này cố gắng thuê một slot từ trang bộ nhớ. Nếu thành công, trả về true 
        /// và gán slot đã thuê vào biến out. Nếu không còn slot trống, trả về false.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out MemorySlot slot)
        {
            slot = default;
            while (true)
            {
                // Đọc giá trị hiện tại của FreeSlots một cách nguyên tử
                long current = Interlocked.Read(ref FreeSlots);
                // Nếu không còn slot trống, trả về false
                if (current == 0) return false;

                // Tìm bit 1 cao nhất trong current
                int bitIdx = Binary.Log2(current);

                long mask = 1L << bitIdx;
                long next = current & ~mask;

                // Kiểm tra xem có luồng khác đã thay đổi FreeSlots trong khi chúng ta đang xử lý không
                if (Interlocked.CompareExchange(ref FreeSlots, next, current) == current)
                {
                    // địa chỉ của slot được tính bằng cách cộng bitIdx * kích thước khối slot vào DataHandle
                    byte* block = DataHandle + bitIdx * MemorySlot.GetSlotBlockSize(SlotSize);

                    slot = new MemorySlot(block, SlotSize);
                    slot.Clear();

                    return true;
                }
            }
        }

        /// <summary>
        /// Hàm này trả lại một slot đã thuê về trang bộ nhớ
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(byte* handle)
        {
            // Lấy thông tin meta của slot từ handle
            var meta = MemorySlot.GetSlotHeader(handle);
            int slotIdx = meta->SlotIndex;

            bool isEmpty = false;
            long mask = 1L << slotIdx;

            long current;
            long next;
            do
            {
                current = Interlocked.Read(ref FreeSlots);
                next = current | mask;

                isEmpty = (next == -1L);
            }
            while (Interlocked.CompareExchange(ref FreeSlots, next, current) != current);

            return isEmpty;
        }

        /// <summary>
        /// Hàm này giải phóng bộ nhớ mà trang bộ nhớ đang quản lý. Nó giải phóng 
        /// khối dữ liệu (DataHandle) và đặt DataHandle về null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (DataHandle != null)
            {
                Marshal.FreeHGlobal((IntPtr)DataHandle);

                DataHandle = null;
            }
        }

        /// <summary>
        /// Hàm này tạo một trang bộ nhớ mới (MemoryPage) với chỉ số phân 
        /// loại kích thước (binIndex) và kích thước slot (slotSize).
        /// </summary>
        /// <param name="binIndex"></param>
        /// <param name="slotSize"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemoryPage* CreatePage(int binIndex, int slotSize)
        {
            // Tạo một trang bộ nhớ mới bằng cách cấp phát bộ nhớ unmanaged cho MemoryPage
            MemoryPage* p = (MemoryPage*)Marshal.AllocHGlobal(sizeof(MemoryPage));
            p->SlotSize = slotSize;
            p->FreeSlots = -1; 

            int totalSlotSize = MemorySlot.GetSlotBlockSize(slotSize);
            // Cấp phát bộ nhớ unmanaged cho tất cả các slot trong trang bộ nhớ
            p->DataHandle = (byte*)Marshal.AllocHGlobal(totalSlotSize * Capactity);

            // Khởi tạo meta data cho từng slot
            byte* currentData = p->DataHandle;
            for (int i = 0; i < Capactity; i++)
            {
                MemorySlotHeader* h = (MemorySlotHeader*)currentData;
                h->SlotIndex = i;
                h->BinIndex = binIndex;
                h->PageHandle = p;

                currentData += totalSlotSize;
            }
            return p;
        }
    }
}

namespace Vst
{
    // MemoryPageGroup quản lý một nhóm các MemoryPage, mỗi nhóm có thể chứa nhiều trang bộ nhớ (MemoryPage) để phục vụ việc thuê và trả lại các khối bộ nhớ (MemorySlot).
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal unsafe struct MemoryPageGroup : IDisposable
    {
        // Mảng các con trỏ Page (Cấp phát unmanaged)
        [FieldOffset(0)] private MemoryPage** _pages; 
        [FieldOffset(8)] private int _top;        // Chỉ số Page hiện tại đang dùng để Rent
        [FieldOffset(12)] private int _capacity;   // Tổng số Page có thể chứa trong mảng _pages

        [FieldOffset(16)] private LockEngine* _lockHandle;

        [FieldOffset(32)] private readonly int _slotSize;
        [FieldOffset(36)] private readonly int _binIndex;


        // Hàm khởi tạo của MemoryPageGroup, nhận vào chỉ số phân loại kích thước (binIndex)
        // và kích thước slot (slotSize). Nó khởi tạo các trường dữ liệu và cấp phát bộ nhớ
        // unmanaged cho mảng _pages.
        internal MemoryPageGroup(int binIndex, int slotSize)
        {
            _binIndex = binIndex;
            _slotSize = (slotSize + 7) & ~7; // Làm tròn lên bội số của 8 để đảm bảo căn chỉnh bộ nhớ
            _top = -1;
            _capacity = 4; // Bắt đầu với 4 Page, thiếu thì Realloc

            _pages = (MemoryPage**)Memory.MallocArray<IntPtr>(_capacity);
            _lockHandle = LockEngine.Create();
        }

        /// <summary>
        /// Hàm này trả lại một slot đã thuê về trang bộ nhớ (MemoryPage) mà nó thuộc về 
        /// </summary>
        /// <param name="handle"></param>
        public void Return(byte* handle)
        {
            var meta = MemorySlot.GetSlotHeader(handle);
            var page = meta->PageHandle;

            page->Return(handle);
        }

        /// <summary>
        /// Hàm này thuê một slot từ nhóm các trang bộ nhớ (MemoryPageGroup) 
        /// </summary>
        /// <returns></returns>
        internal MemorySlot Rent()
        {
            if (_top >= 0 && _pages[_top]->TryRent(out var slot))
                return slot;

            using (new LockingWrite(_lockHandle))
            {
                if (_top >= 0 && _pages[_top]->TryRent(out slot))
                    return slot;

                for (int i = _top - 1; i >= 0; i--)
                {
                    var page = _pages[i];
                    if (page->TryRent(out slot))
                    {
                        _pages[i] = _pages[_top];
                        _pages[_top] = page;

                        return slot;
                    }
                }

                CreatePage();
                return _pages[_top]->RentDirect();
            }
        }

        /// <summary>
        /// Hàm này tạo một trang bộ nhớ mới (MemoryPage) và thêm nó vào mảng _pages. 
        /// Nếu mảng _pages đã đầy, nó sẽ tăng kích thước của mảng bằng cách sử dụng Realloc.
        /// </summary>
        private void CreatePage()
        {
            if (++_top >= _capacity)
            {
                int oldSize = _capacity;
                _capacity <<= 1;
                // Realloc mảng _pages để tăng kích thước
                _pages = (MemoryPage**)Memory.Realloc(_pages, _capacity * sizeof(IntPtr), oldSize * sizeof(IntPtr));
            }
            _pages[_top] = MemoryPage.CreatePage(_binIndex, _slotSize);
        }

        public void Dispose()
        {
            if (_pages != null)
            {
                for (int i = 0; i <= _top; i++)
                {
                    var p = _pages[i];
                    if (p != null)
                    {
                        p->Dispose();
                        Memory.Free(p);
                    }
                }

                Memory.Free(_pages);
                _pages = null;
            }
            _top = -1;

            LockEngine.Destroy(_lockHandle);
        }
    }
}

namespace Vst
{
    public unsafe struct MemoryPool
    {
        // Cố định 256 bins để quản lý từ 8 bytes đến 2KB
        private readonly MemoryPageGroup* _bins;

        //static long _rented;
        //static int _rentedCount;

        public MemoryPool(int binSize)
        {
            _bins = Memory.MallocArray<MemoryPageGroup>(binSize);
            for (int i = 0; i < binSize; i++)
                _bins[i] = new MemoryPageGroup(i, 16 << i);

            //_rented = 0;
            //_rentedCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemorySlot Rent(int size)
        {
            var required = size <= 16 ? 16 : Binary.RoundUpToPowerOf2(size);
            if (required >= (8 << 10))
                return AllocateHuge(required);

            int i = Binary.Log2(required) - 4;

            var bin = _bins + i;
            var slot = bin->Rent();

            //*MemorySlot.GetBlockPtr(slot.RawData) = ++_rentedCount;

            //Console.WriteLine($"+{_rentedCount}: Rent: {slot.Capacity}, Total: {_rented += slot.Capacity}");
            return slot;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(byte* handle)
        {
            if (handle == null)
                return;

            int binIdx = MemorySlot.GetSlotHeader(handle)->BinIndex;
            if (binIdx >= 0)
            {
                var bin = _bins + binIdx;
                bin->Return(handle);

                //int size = *(int*)bin;
                //Console.WriteLine($"-{*MemorySlot.GetBlockPtr(handle)} Return: {size}, Total: {_rented -= size}");
            }
            else
            {
                Marshal.FreeHGlobal((IntPtr)(handle - sizeof(MemorySlotHeader)));
            }
        }

        private MemorySlot AllocateHuge(int size)
        {
            size = (size + 7) & ~7;
            byte* data = (byte*)Marshal.AllocHGlobal(size + (sizeof(MemorySlotHeader)));
            var slot = new MemorySlot(data, size);

            slot.Header->BinIndex = -1;
            slot.Header->SlotIndex = size;

            return slot;
        }
    }
}

namespace Vst
{
    public unsafe static partial class Memory
    {
        // MemoryPool được sử dụng để quản lý bộ nhớ, với 16 bins để quản lý các khối bộ nhớ có kích thước từ 16 bytes đến 2KB.

        private static readonly MemoryPool _pool = new MemoryPool(16);

        /// <summary>
        /// Xin bộ nhớ theo số lượng byte yêu cầu. Nếu số lượng byte yêu cầu vượt quá 2KB, bộ nhớ sẽ được cấp phát trực tiếp từ hệ thống.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemorySlot Rent(int size) { return _pool.Rent(size); }

        // This method rents a block of memory for a specific unmanaged type T and returns a pointer to it.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Rent<T>() { return (T*)_pool.Rent(sizeof(T)).RawData; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetHandle<T>(this object obj, int offset)
        {
            void* p = &obj;
            return (T*)((byte*)p + offset);
        }

        /// <summary>
        /// Xin bộ nhớ theo số lượng byte yêu cầu và trả về con trỏ đến 
        /// khối bộ nhớ đã thuê. Nếu số lượng byte yêu cầu vượt quá 2KB, 
        /// bộ nhớ sẽ được cấp phát trực tiếp từ hệ thống.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* RentHandle(int size) { return _pool.Rent(size).RawData; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(void* rented) => _pool.Return((byte*)rented);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* MallocArray<T>(int capacity) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(capacity * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Malloc<T>() where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* Realloc(void* mem, long newLength, long oldLength)
        {
            if (mem == null) return (byte*)Marshal.AllocHGlobal((IntPtr)newLength);

            var newPtr = (byte*)Marshal.ReAllocHGlobal((IntPtr)mem, (IntPtr)newLength);

            if (newLength > oldLength)
            {
                byte* startClear = newPtr + oldLength;
                long bytesToClear = newLength - oldLength;

                ZeroMemory(startClear, bytesToClear);
            }

            return newPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory(void* ptr, long size)
        {
            byte* b = (byte*)ptr;
            // Tối ưu: Xóa theo từng 8-byte (long)
            while (size >= 8)
            {
                *(long*)b = 0;
                b += 8;
                size -= 8;
            }
            // Xóa nốt các byte lẻ còn lại
            while (size > 0)
            {
                *b = 0;
                b++;
                size--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* mem)
        {
            if (mem != null) Marshal.FreeHGlobal((IntPtr)mem);
        }
    }
}