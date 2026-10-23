using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vst
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public unsafe struct KeyEntry
    {
        [FieldOffset(0)] public long Key; 
        [FieldOffset(8)] public long SecondaryKey;

        [FieldOffset(16)] public long Offset;       // Trỏ đến file .dat
        [FieldOffset(24)] public int RecordLength;  // Số byte của bản ghi trong .dat
        [FieldOffset(24)] public int IndexPosition;   // Vị trí cần insert trong .idx

        [FieldOffset(28)] public RecordState State; 
        [FieldOffset(29)] public PrimaryMode Type;

        [FieldOffset(16)] public Buffer Comparer;               // Vùng KeyPool
        /// <summary>
        /// Field này xác định Key được cấp phát từ đâu, ví dụ từ bộ nhớ tạm, từ heap của document hay là giá trị cố định.
        /// </summary>
        [FieldOffset(30)] public KeyEntryState KeyState;        // Dùng cho Destroy
        /// <summary>
        /// Xác định key đã hash về 16 bytes hay chưa 
        /// </summary>
        [FieldOffset(31)] public bool Calculated;

        public byte* KeyHandle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (KeyEntry* p = &this) return (byte*)p;
            }
        }

        /// <summary>
        /// Hàm này dùng để Hash key và SecondaryKey từ Comparer
        /// </summary>
        public void EnsureToCompare()
        {
            if (Calculated) return;

            byte* dst = KeyHandle;
            byte* handle = Comparer.Handle;
            int length = Comparer.Length;

            Key = 0;
            SecondaryKey = 0;
            Calculated = true;

            switch (Type)
            {
                case PrimaryMode.String:
                    if (length > 16)
                    {
                        XXHash64.ComputeDualHashShort(handle, length, (long*)dst);
                        Type = PrimaryMode.LongString;
                        return;
                    }

                    Buffer.MemCopy(handle, dst, length);
                    break;

                case PrimaryMode.Fixed:
                case PrimaryMode.Default:
                    var reader = new StringReader(handle, length);
                    Key = reader.GetLong();
                    return;

                case PrimaryMode.ObjectId:
                    HexHelper.WriteBytes(ref dst, handle, Math.Min(12, length >> 1));
                    return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(KeyEntry* e)
        {
            return e->Key == Key && e->SecondaryKey == SecondaryKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(long* e)
        {
            return Key == *e && SecondaryKey == *(e + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Mapping(byte* handle, int length)
        {
            Comparer.Handle = handle;
            Comparer.Length = length;

            Calculated = false;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public DirtyMode SyncDocument(long offset, RecordEntry* header)
        //{
        //    var res = DirtyMode.Clean;
        //    if (RecordLength != header->RecordLength)
        //    {
        //        RecordLength = header->RecordLength;
        //        res = DirtyMode.IndexChanged;
        //    }
        //    if (Offset != offset)
        //    {
        //        Offset = offset;
        //        res = DirtyMode.IndexChanged;
        //    }
        //    return res;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyEntry* FromString(string key)
        {
            var buffer = Memory.RentHandle(key.Length << 1);
            int length = StringHelper.EncodeUTF8(buffer, key);

            var e = FromHandle(buffer, length);
            e->KeyState = KeyEntryState.Rented;
            return e;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyEntry* FromHandle(byte* handle, int length)
        {
            var e = Create(PrimaryMode.String);
            StringHelper.ToLowerCase(handle, length);

            e->Mapping(handle, length);

            return e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyEntry* FromObjectId(byte* handle)
        {
            var e = Create(PrimaryMode.ObjectId);
            e->Key = *(long*)handle;
            e->SecondaryKey = ((long)*(uint*)(handle + 8)) << 32;
            return e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyEntry* FromFixedValue(long value)
        {
            var e = Create(PrimaryMode.Fixed);
            e->Key = value;
            return e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyEntry* Create(PrimaryMode type)
        {
            var e = Memory.Rent<KeyEntry>();
            *e = new KeyEntry
            {
                Type = type,
                Key = 0,
                SecondaryKey = 0,
                Calculated = true,
            };

            return e;
        }

        /// <summary>
        /// Hàm này dùng để giải phóng bộ nhớ của KeyEntry, tùy 
        /// thuộc vào trạng thái KeyState mà sẽ giải phóng các 
        /// vùng bộ nhớ khác nhau.
        /// </summary>
        /// <param name="e"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(KeyEntry* e)
        {
            switch (e->KeyState)
            {
                case KeyEntryState.Rented: 
                    Memory.Return(e->Comparer.Handle);
                    break;

                case KeyEntryState.DocumentHeap:
                case KeyEntryState.Fixed:
                    return;
            }
            Memory.Return(e);
        }
    }
}
