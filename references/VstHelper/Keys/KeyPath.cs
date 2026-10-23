using System;
using System.Runtime.CompilerServices;

namespace Vst
{
    public unsafe struct KeyEntryPath : IDisposable
    {
        IPointerList<KeyEntry> _keys;
        KeyEntry* _bag;

        public KeyEntryPath(string path)
        {
            _bag = KeyEntry.FromString(path);
            _keys = new PointerList<KeyEntry>(4);

            byte* start = _bag->Comparer.Handle;
            byte* end = start + _bag->Comparer.Length;

            for (byte* p = start; p <= end; p++)
            {
                // Kiểm tra dấu phân cách hoặc điểm kết thúc
                if (p == end || *p == '.')
                {
                    int length = (int)(p - start);
                    if (length == 0) continue;

                    var e = KeyEntry.FromHandle(start, length);
                    //var e = KeyPool.Intern(piece);

                    _keys.Push(e);

                    start = p + 1;

                    //KeyEntry.Destroy(piece);

                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_bag != null)
            {
                for (int i = 0; i < _keys.Count; i++)
                {
                    KeyEntry.Destroy(_keys[i]);
                }

                _keys.Dispose();

                KeyEntry.Destroy(_bag);
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _keys.Count; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyEntry* GetKeyAt(int index) => _keys[index];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyEntryPath* FromString(string path)
        {
            var p = Memory.Rent<KeyEntryPath>();
            *p = new KeyEntryPath(path);

            return p;
        }
        public static void Destroy(KeyEntryPath* keyPath)
        {
            keyPath->Dispose();
            Memory.Return(keyPath);
        }
    }
}
