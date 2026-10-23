using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace Vst.Data
{
    public unsafe interface IIndexComparer
    {
        int Compare(long* a, long* b);
    }

    public unsafe struct SimpleIndexComparer : IIndexComparer
    {
        public int Compare(long* a, long* b)
        {
            if (*a > *b) return 1;
            if (*a < *b) return -1;
            return 0;
        }
    }
    public unsafe struct ComplexIndexComparer : IIndexComparer
    {
        public int Compare(long* a, long* b)
        {
            if (*a > *b) return 1;
            if (*a < *b) return -1;

            ++a; ++b;
            if (*a > *b) return 1;
            if (*a < *b) return -1;

            return 0;
        }
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct IndexRange
    {
        public static int SmallRange { get; set; } = 128;
        public static int HugeRange { get; set; } = 128;

        [FieldOffset(0)] public int Left;
        [FieldOffset(4)] public int Right;

        [FieldOffset(0)] public long Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IndexRange Create(int left, int right)
        {
            return new IndexRange { Left = left, Right = right };
        }

        public int Length => Right - Left;
    }

    public unsafe class KeyIndexer<TComparer> 
        where TComparer : struct, IIndexComparer {

        private KeyEntry* _items;
        private int _count;

        private TComparer _comparer;

        public long* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (long*)(_items + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyIndexer(KeyEntry* items, int count)
        {
            _items = items;
            _count = count;
            _comparer = new TComparer();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Find(KeyEntry* e)
        {
            int low = 0; // Khởi đầu của mảng
            int high = _count - 1; // Kết thúc của mảng

            e->EnsureToCompare(); // Nếu key là string thì sẽ được hash thành 16 bytes
            long* target = (long*)e;

            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);
                long* comp = (long*)(_items + mid); // Lấy phần tử ở vị trí mid

                var res = _comparer.Compare(comp, target); // So sánh target với giá trị ở mid
                if (res == 0)
                    return mid;

                if (res < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            e->IndexPosition = low; // Chèn vào vị trí low nếu không tìm thấy
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyEntry* GetElementPtrAt(int index) => _items + index;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindInsertPosition(KeyEntry* e, int low, int high)
        {
            long* target = (long*)e;
            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);
                long* comp = (long*)(_items + mid);

                var res = _comparer.Compare(comp, target);
                if (res == 0)
                    return mid;

                if (res < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return low;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            if (_count < 2) return;
            ParallelSort(0, _count - 1);
        }
        

        /// <summary>
        /// Hàm này sử dụng Insertion Sort để sắp xếp. Kết hợp 
        /// thủ thuật dịch cả khối RAm bằng Memcopy để dọn chỗ 
        /// chèn item mới. 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ShiftSort(int current, int low, int high)
        {
            var item = _items[current];
            var pos = FindInsertPosition(&item, low, high);
            if (current == pos) { return pos; }

            var lengthToMove = (current - pos) * sizeof(KeyEntry);

             Buffer.MemCopy(this[pos], this[pos + 1], lengthToMove);
            _items[pos] = item;

            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParallelSort(int left, int right)
        {
            if (right - left < IndexRange.HugeRange)
            {
                QuickSort(left, right);
                return;
            }

            int pIndex = Partition(left, right);

            Parallel.Invoke(
                    () => ParallelSort(left, pIndex),
                    () => ParallelSort(pIndex + 1, right)
                );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QuickSort(int left, int right)
        {
            IndexRange* stack = stackalloc IndexRange[64];
            int top = 0;
            stack[top++] = IndexRange.Create(left, right);

            while (top > 0)
            {
                IndexRange range = stack[--top];
                left = range.Left;
                right = range.Right;

                // Insertion Sort cho đoạn nhỏ
                if (right - left < IndexRange.SmallRange)
                {
                    for (int i = left + 1; i <= right; i++)
                        ShiftSort(i, left, i);
                    continue;
                }

                int pIndex = Partition(left, right);

                // Tối ưu stack depth: xử lý đoạn nhỏ trước
                int leftSize = pIndex - left;
                int rightSize = right - pIndex;

                if (leftSize > rightSize)
                {
                    if (left < pIndex) stack[top++] = IndexRange.Create(left, pIndex);
                    if (pIndex + 1 < right) stack[top++] = IndexRange.Create(pIndex + 1, right);
                }
                else
                {
                    if (pIndex + 1 < right) stack[top++] = IndexRange.Create(pIndex + 1, right);
                    if (left < pIndex) stack[top++] = IndexRange.Create(left, pIndex);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Partition(int left, int right)
        {
            int mid = left + ((right - left) >> 1);

            // Median-of-three
            Median(left, mid);
            Median(left, right);
            Median(mid, right);

            int i = left - 1;
            int j = right + 1;

            var pivot = _items[mid];
            var pKey = (long*)&pivot;

            while (true)
            {
                while (_comparer.Compare(this[++i], pKey) < 0) ;
                while (_comparer.Compare(this[--j], pKey) > 0) ;

                if (i >= j) return j;
                Swap(i, j);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Median(int i, int j)
        {
            if (_comparer.Compare(this[i], this[j]) > 0) 
                Swap(i, j);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j)
        {
            KeyEntry temp = _items[i];
            _items[i] = _items[j];
            _items[j] = temp;
        }
    }
}
