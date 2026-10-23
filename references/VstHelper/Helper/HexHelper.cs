using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    public unsafe static class HexHelper
    {
        // 1024 bytes = 256 uints for ByteToHexTable 
        private static readonly uint* ByteToHexTable;
        // 256 bytes for HexToByteTable
        private static readonly byte* HexToByteTable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetHexDigit(uint nibble)
        {
            return (nibble < 10 ? nibble | 0x30 : nibble + 87);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetByte(byte hexDigit)
        {
            return HexToByteTable[hexDigit];
        }

        // Ghi 1 byte thành 2 ký tự hex (4 bytes trong string)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteHex(ref byte* dst, byte b)
        {
            *(uint*)dst = ByteToHexTable[b];
            dst += 4;
        }

        // Ghi một mảng byte thành chuỗi hex vào vùng nhớ chỉ định
        public static void WriteHex(ref byte* dst, byte* src, int length)
        {
            while (length > 0)
            {
                WriteHex(ref dst, *src++);
                --length;
            }
        }

        // Extension method cho byte[]
        public static string ToHexString(this byte[] data)
        {
            if (data == null) return null;
            int len = data.Length;
            if (len == 0) return string.Empty;

            string result = new string('\0', len << 1);
            fixed (byte* pData = data)
            fixed (char* pResult = result)
            {
                byte* pOut = (byte*)pResult;
                WriteHex(ref pOut, pData, len);
            }
            return result;
        }

        // Giải mã hex từ con trỏ char sang con trỏ byte
        public static void WriteBytes(ref byte* dst, byte* src, int length)
        {
            while (length > 0)
            {
                uint h = HexToByteTable[*src++];
                uint l = HexToByteTable[*src++];
                *dst++ = (byte)((h << 4) | l);

                --length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SwapEndian(ulong v) =>
            ((v & 0xff) << 56) |
            ((v & 0xff00) << 40) |
            ((v & 0xff0000) << 24) |
            ((v & 0xff000000) << 8) |
            ((v & 0xff00000000) >> 8) |
            ((v & 0xff0000000000) >> 24) |
            ((v & 0xff000000000000) >> 40) |
            ((v & 0xff00000000000000) >> 56);

        // Hàm bổ trợ Swap cho uint
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SwapEndian(uint v) =>
            ((v & 0xff) << 24) |
            ((v & 0xff00) << 8) |
            ((v & 0xff0000) >> 8) |
            ((v & 0xff000000) >> 24);

        static HexHelper()
        {
            IntPtr basePtr = Marshal.AllocHGlobal(1280);
            ByteToHexTable = (uint*)basePtr;
            HexToByteTable = (byte*)(ByteToHexTable + 256);

            for (uint i = 0; i < 256; i++)
            {
                ByteToHexTable[i] = GetHexDigit(i >> 4) | (GetHexDigit(i & 15) << 16);
            }

            for (int i = 0; i < 256; i++) HexToByteTable[i] = 0xff;
            for (int i = 0; i < 10; i++) HexToByteTable['0' + i] = (byte)i;
            for (int i = 0; i < 6; i++)
            {
                byte val = (byte)(10 + i);
                HexToByteTable['A' + i] = val;
                HexToByteTable['a' + i] = val;
            }
        }
    }
}
