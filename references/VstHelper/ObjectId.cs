
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
    /// <summary>
    /// Struct ObjectId 12 bytes đại diện cho một định danh duy nhất, cung cấp các phương thức để tạo và so sánh các ObjectId mà không cần tạo bản sao.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public unsafe struct ObjectId : IEquatable<ObjectId>
    {
        /// <summary>
        /// Con trỏ trực tiếp đến khối bộ nhớ chứa ObjectId
        /// </summary>
        [FieldOffset(0)] public byte* Handle;
        /// <summary>
        /// Timestamp đại diện cho thời gian tạo ObjectId, được lưu trữ trong 4 bytes đầu tiên.
        /// </summary>
        [FieldOffset(0)] private uint _timestamp;
        /// <summary>
        /// Dữ liệu bổ sung đại diện cho thông tin máy
        /// Machine 3 bytes. Hash tên máy tính (2^24 = 16 triệu máy khác nhau)
        /// Process 2 bytes. ID của tiến trình (2^16 = 65 nghìn tiến trình khác nhau)
        /// Counter 3 bytes. Bộ đếm tăng dần (2^24 = 16 triệu giá trị khác nhau)
        /// </summary>
        [FieldOffset(4)] private ulong _data;

        private static ulong _info;
        private static int _incrementCounter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetStoreHash()
        {
            fixed (uint* p = &_timestamp)
            {
                return (long)Vst.XXHash64.ComputeHash((byte*)p, 12);
            }
        }

        static ObjectId()
        {
            // 1. Lấy Machine Name và Hash để lấy 3 bytes đầu
            var machine = (uint)Environment.MachineName.GetHashCode() & 0xffffff;
            var pid = (uint)Process.GetCurrentProcess().Id & 0xffff;

            _info = (((ulong)machine) << 40) | ((ulong)pid << 24);
            _incrementCounter = new Random().Next();
        }

        /// <summary>
        /// Sinh ID duy nhất dựa trên thời gian hiện tại 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectId NewId()
        {
            uint timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            uint increment = (uint)Interlocked.Increment(ref _incrementCounter) & 0xffffff;

            return new ObjectId(timestamp, _info | increment);
        }

        private ObjectId(uint t, ulong d)
        {
            Handle = null;
            _timestamp = t; _data = d;
        }

        public byte* RawPtr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (uint* p = &_timestamp) return (byte*)p; }
        }

        #region TIMESTAMP
        public int Timestamp => (int)_timestamp;
        public long Data => (long)_data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte GetTimestampByte(int index)
        {
            return (byte)((_timestamp >> (index << 3)) & 0xff);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte GetDataByte(int index)
        {
            return (byte)((_data >> (index << 3)) & 0xff);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)GetStoreHash();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is ObjectId other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ObjectId other)
        {
            return Timestamp == other.Timestamp && Data == other.Data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ObjectId a, ObjectId b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ObjectId a, ObjectId b) => !a.Equals(b);

        #region PARSE & TOSTRING (Direct Memory Access)

        public override string ToString()
        {
            string result = new string('\0', 24);
            fixed (char* pDst = result)
            {
                byte* pOut = (byte*)pDst;
                Vst.HexHelper.WriteHex(ref pOut, RawPtr, 12);
            }

            return result;
        }

        #endregion
    }
}
