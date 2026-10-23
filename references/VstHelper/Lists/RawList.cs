using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    using System;
    using System.Runtime.CompilerServices;

    public unsafe interface IList<T> : IDisposable where T : unmanaged
    {
        int Count { get; }

        // Indexer truy cập theo giá trị
        T this[int index] { get; set; }

        // Thao tác cơ bản
        void Push(T value);
        T Pop();
        ref T Peek();
        void Clear();

        // Thao tác vị trí
        void Swap(int i, int j);
        T GetElementAt(int index);
        void SetElementAt(int index, T node);
        void RemoveAt(int index);

        // Thao tác Pointer & Memory (Đặc trưng của RawList)
        T* GetPointerAt(int index);
        ref T GetReferenceAt(int index);
        void Mapping(T* handle, int count);

        T* Insert(int index, T value);
        void GrowUp();
    }
    public unsafe struct RawList<T> : IList<T> where T : unmanaged
    {
        /// <summary>
        /// 8 bytes cho 64-bit, 4 bytes cho 32-bit. Đây là con trỏ đến vùng nhớ chứa các phần tử của danh sách.
        /// </summary>
        private T* _items;
        /// <summary>
        /// Số lượng phần tử hiện tại trong danh sách. Đây là số lượng phần tử thực tế mà danh sách đang chứa.
        /// </summary>
        private int _count;
        /// <summary>
        /// Dung lượng tối đa của danh sách. Đây là số lượng phần tử mà danh sách có thể chứa trước khi cần phải mở rộng vùng nhớ.
        /// </summary>
        private int _capacity;

        public int Count => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawList(int capacity)
        {
            var c = sizeof(T);

            _capacity = capacity < 4 ? 4 : capacity;
            _items = (T*)Memory.RentHandle(_capacity * c);
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Mapping(T* handle, int count)
        {
            _items = handle;
            _count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_items != null)
            {
                Memory.Return((byte*)_items);
                
                _items = null;
                _count = 0;
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetElementAt(index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetElementAt(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Swap(int i, int j)
        {
            var t = _items[i]; 
            _items[i] = _items[j];
            _items[j] = t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetElementAt(int index)
        {
            return *(_items + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetElementAt(int index, T node)
        {
            *(_items + index) = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T value)
        {
            // Mảng đầy. Mở rộng vùng nhớ để chứa thêm phần tử mới.
            if (_count >= _capacity) GrowUp();
            _items[_count++] = value;
        }

        /// <summary>
        /// Hàm này chuẩn bị một vùng nhớ đủ lớn để chứa n phần tử mới 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Prepare(int n)
        {
            int total = _count + n;
            while (total >= _capacity) GrowUp();

            T* last = _items + _count;
            _count = total;

            return last;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Insert(int index, T value)
        {
            GrowUp();
            var lengthToMove = (_count - index) * sizeof(T);
            T* src = GetPointerAt(index);
            T* dst = src + 1;
            Vst.Buffer.MemCopy(src, dst, lengthToMove);

            _items[index] = value;
            _count++;

            return _items;
        }

        /// <summary>
        /// Hàm Pop lấy phần tử cuối cùng ra khỏi danh sách và giảm số lượng phần tử đi 1. Phần tử cuối cùng sẽ không bị xóa khỏi bộ nhớ, nhưng sẽ không còn được tính trong _count nữa.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            return *(_items + (--_count));
        }

        /// <summary>
        /// Hàm RemoveAt xóa phần tử tại vị trí index trong danh sách. Sau khi xóa, các phần tử phía sau sẽ được dịch chuyển lên để lấp đầy khoảng trống.
        /// </summary>
        /// <param name="index"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            // pre: [A] [B] [C] [D] [E] <- index = 2 (B), count = 5
            int moveCount = --_count - index;
            // moveCount = (5 - 1) - 2 = 2
            byte* dst = (byte*)(_items + index);
            // dst trỏ đến vị trí của phần tử cần xóa (B)
            byte* src = (byte*)(_items + index + 1);
            // src trỏ đến phần tử tiếp theo (C)

            long bytesToMove = (long)moveCount * sizeof(T);
            // bytesToMove = 2 * sizeof(T) = 2 * 4 = 8 bytes (nếu T là int)
            Buffer.MemCopy(src, dst, bytesToMove);
            // Kết quả: [A] [C] [D] [E] [E] (phần tử cuối cùng vẫn còn nhưng không được tính trong _count)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _count = 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek() => ref *(_items + _count - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPointerAt(int index) => _items + index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetReferenceAt(int index) => ref _items[index];

        /// <summary>
        /// Mở rộng mảng theo hệ số 1.5 lần so với dung lượng hiện tại 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GrowUp()
        {
            int newCap = _capacity == 0 ? 4 : (_capacity + (_capacity >> 1));

            byte* newBuf = Memory.Rent(newCap * sizeof(T)).RawData;
            Buffer.MemCopy(_items, newBuf, (long)_count * sizeof(T));

            Memory.Return(_items);

            _items = (T*)newBuf;
            _capacity = newCap;
        }
    }

    public unsafe interface IPointerList<T> : IDisposable where T : unmanaged
    {
        int Count { get; }

        T* this[int index] { get; set; }

        void Push(T* item);

        T* Pop();

        T* Peek();

        void Insert(int index, T* item);

        void Clear();

        ref T GetReferenceAt(int index);

        T* GetPointerAt(int index);

        void GrowUp();
    }
    public unsafe struct PointerList<T> : IPointerList<T> where T : unmanaged
    {
        // Lõi là RawList chứa các địa chỉ (con trỏ)
        private RawList<IntPtr> _inner;

        public int Count => _inner.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointerList(int capacity)
        {
            _inner = new RawList<IntPtr>(capacity);
        }

        public T* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T*)_inner.GetElementAt(index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _inner.SetElementAt(index, (IntPtr)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _inner.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T* item) => _inner.Insert(index, (IntPtr)item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPointerAt(int index, T* item) => _inner.SetElementAt(index, (IntPtr)item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T* item) => _inner.Push((IntPtr)item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Pop() => (T*)_inner.Pop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Peek() => (T*)_inner.Peek();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetReferenceAt(int index) => ref *(T*)_inner.GetElementAt(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _inner.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPointerAt(int index) => (T*)_inner.GetElementAt(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GrowUp() => _inner.GrowUp();

    }
}

