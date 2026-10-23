using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    public class Binary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundUpToPowerOf2(int size)
        {
            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            return size + 1;
        }

        // Trả về log2 của số nguyên dương, ví dụ: Log2(8) = 3, Log2(9) = 3, Log2(15) = 3, Log2(16) = 4
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(long number)
        {
            // number == 0 xử lý ở tầng ngoài để tránh check ở đây
            uint v = (uint)number;
            int index = 0;
            int shift;

            // Kiểm tra 32 bit thấp, nếu bằng 0 thì nhảy lên 32 bit cao
            shift = (number & 0xFFFFFFFF) == 0 ? 32 : 0;
            ulong v64 = (ulong)number >> shift;
            index += shift;

            // Tiếp tục chia để trị không dùng 'if' để CPU không phải rẽ nhánh
            shift = (v64 & 0xFFFF) == 0 ? 16 : 0; v64 >>= shift; index += shift;
            shift = (v64 & 0xFF) == 0 ? 8 : 0; v64 >>= shift; index += shift;
            shift = (v64 & 0xF) == 0 ? 4 : 0; v64 >>= shift; index += shift;
            shift = (v64 & 0x3) == 0 ? 2 : 0; v64 >>= shift; index += shift;
            shift = (v64 & 0x1) == 0 ? 1 : 0; index += shift;

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static uint GetInt24(byte* handle)
        {
            return (uint)handle & 0xFF000000;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void SetInt24(byte* handle, uint value)
        {
            *(uint*)handle = (*(uint*)handle & 0xFF000000) | (value & 0x00FFFFFF);
        }

    }
}

namespace Vst
{
    /// <summary>
    /// Class XXHash64 cung cấp các phương thức để tính toán giá trị băm (hash) 64-bit của dữ liệu nhị phân. Nó sử dụng thuật toán XXHash, một thuật toán băm nhanh và hiệu quả, đặc biệt phù hợp cho các ứng dụng yêu cầu tốc độ cao và độ phân tán tốt.
    /// </summary>
    public unsafe static class XXHash64
    {
        private const ulong PRIME64_1 = 11400714785074694791UL;
        private const ulong PRIME64_2 = 14029467366897019727UL;
        private const ulong PRIME64_3 = 1609587929392839161UL;
        private const ulong PRIME64_4 = 9650029242287828579UL;
        private const ulong PRIME64_5 = 2870177450012600216UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotL64(ulong x, int r) => (x << r) | (x >> (64 - r));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ComputeHash(byte* input, int length, ulong seed = 0)
        {
            //Console.WriteLine(new Buffer(input, length).ToString());

            byte* end = input + length;
            ulong hash;

            if (length >= 32)
            {
                byte* limit = end - 32;
                ulong v1 = seed + PRIME64_1 + PRIME64_2;
                ulong v2 = seed + PRIME64_2;
                ulong v3 = seed + 0;
                ulong v4 = seed - PRIME64_1;

                do
                {
                    v1 += *(ulong*)input * PRIME64_2;
                    v1 = RotL64(v1, 31) * PRIME64_1;
                    input += 8;

                    v2 += *(ulong*)input * PRIME64_2;
                    v2 = RotL64(v2, 31) * PRIME64_1;
                    input += 8;

                    v3 += *(ulong*)input * PRIME64_2;
                    v3 = RotL64(v3, 31) * PRIME64_1;
                    input += 8;

                    v4 += *(ulong*)input * PRIME64_2;
                    v4 = RotL64(v4, 31) * PRIME64_1;
                    input += 8;
                } while (input <= limit);

                hash = RotL64(v1, 1) + RotL64(v2, 7) + RotL64(v3, 12) + RotL64(v4, 18);

                // Merge v1..v4 chuẩn
                v1 = RotL64(v1 * PRIME64_2, 31) * PRIME64_1; hash ^= v1;
                hash = hash * PRIME64_1 + PRIME64_4;

                v2 = RotL64(v2 * PRIME64_2, 31) * PRIME64_1; hash ^= v2;
                hash = hash * PRIME64_1 + PRIME64_4;

                v3 = RotL64(v3 * PRIME64_2, 31) * PRIME64_1; hash ^= v3;
                hash = hash * PRIME64_1 + PRIME64_4;

                v4 = RotL64(v4 * PRIME64_2, 31) * PRIME64_1; hash ^= v4;
                hash = hash * PRIME64_1 + PRIME64_4;
            }
            else
            {
                hash = seed + PRIME64_5;
            }

            hash += (ulong)length;

            // Xử lý nốt phần còn lại (Tail)
            while (input + 8 <= end)
            {
                ulong k1 = RotL64(*(ulong*)input * PRIME64_2, 31) * PRIME64_1;
                hash ^= k1;
                hash = RotL64(hash, 27) * PRIME64_1 + PRIME64_4;
                input += 8;
            }

            if (input + 4 <= end)
            {
                hash ^= *(uint*)input * PRIME64_1;
                hash = RotL64(hash, 23) * PRIME64_2 + PRIME64_3;
                input += 4;
            }

            while (input < end)
            {
                hash ^= *input * PRIME64_5;
                hash = RotL64(hash, 11) * PRIME64_1;
                input++;
            }

            return Avalanche(hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComputeDualHashShort(byte* input, int length, long* key)
        {
            // 1. Khởi tạo 2 trạng thái với 2 Seed khác nhau
            // H1: Seed 0, H2: Seed là length kết hợp hằng số để tạo nhiễu
            ulong state1 = 0 + PRIME64_5;
            ulong state2 = ((ulong)length ^ 0xBF58476D1CE4E5B9L) + PRIME64_5;

            state1 += (ulong)length;
            state2 += (ulong)length;

            byte* end = input + length;

            // 2. Vòng lặp chính: Đọc 1 lần, tính cho cả 2
            while (input + 8 <= end)
            {
                ulong k = *(ulong*)input;

                // Xử lý cho State 1
                ulong k1 = RotL64(k * PRIME64_2, 31) * PRIME64_1;
                state1 ^= k1;
                state1 = RotL64(state1, 27) * PRIME64_1 + PRIME64_4;

                state2 ^= k1;
                state2 = RotL64(state2, 27) * PRIME64_1 + PRIME64_4;

                input += 8;
            }

            if (input + 4 <= end)
            {
                uint k4 = *(uint*)input;
                ulong p1 = k4 * PRIME64_1;
                state1 ^= p1; state1 = RotL64(state1, 23) * PRIME64_2 + PRIME64_3;
                state2 ^= p1; state2 = RotL64(state2, 23) * PRIME64_2 + PRIME64_3;
                input += 4;
            }

            while (input < end)
            {
                ulong p1 = (*input) * PRIME64_5;
                state1 ^= p1; state1 = RotL64(state1, 11) * PRIME64_1;
                state2 ^= p1; state2 = RotL64(state2, 11) * PRIME64_1;
                input++;
            }

            *key = (long)Avalanche(state1);
            *(key + 1) = (long)Avalanche(state2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Avalanche(ulong hash)
        {
            hash ^= hash >> 33;
            hash *= PRIME64_2;
            hash ^= hash >> 29;
            hash *= PRIME64_3;
            hash ^= hash >> 32;
            return hash;
        }
    }
}

namespace Vst
{
    public unsafe static class FastMD5
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Rol(uint x, int n) => (x << n) | (x >> (32 - n));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HashBlock(uint* x, uint* h)
        {
            uint a = h[0], b = h[1], c = h[2], d = h[3];

            // VÒNG 1: F(b,c,d) = (b & c) | (~b & d)
            a = b + Rol(a + ((b & c) | (~b & d)) + x[0] + 0xd76aa478, 7);
            d = a + Rol(d + ((a & b) | (~a & c)) + x[1] + 0xe8c7b756, 12);
            c = d + Rol(c + ((d & a) | (~d & b)) + x[2] + 0x242070db, 17);
            b = c + Rol(b + ((c & d) | (~c & a)) + x[3] + 0xc1bdceee, 22);
            a = b + Rol(a + ((b & c) | (~b & d)) + x[4] + 0xf57c0faf, 7);
            d = a + Rol(d + ((a & b) | (~a & c)) + x[5] + 0x4787c62a, 12);
            c = d + Rol(c + ((d & a) | (~d & b)) + x[6] + 0xa8304613, 17);
            b = c + Rol(b + ((c & d) | (~c & a)) + x[7] + 0xfd469501, 22);
            a = b + Rol(a + ((b & c) | (~b & d)) + x[8] + 0x698098d8, 7);
            d = a + Rol(d + ((a & b) | (~a & c)) + x[9] + 0x8b44f7af, 12);
            c = d + Rol(c + ((d & a) | (~d & b)) + x[10] + 0xffff5bb1, 17);
            b = c + Rol(b + ((c & d) | (~c & a)) + x[11] + 0x895cd7be, 22);
            a = b + Rol(a + ((b & c) | (~b & d)) + x[12] + 0x6b901122, 7);
            d = a + Rol(d + ((a & b) | (~a & c)) + x[13] + 0xfd987193, 12);
            c = d + Rol(c + ((d & a) | (~d & b)) + x[14] + 0xa679438e, 17);
            b = c + Rol(b + ((c & d) | (~c & a)) + x[15] + 0x49b40821, 22);

            // VÒNG 2: G(b,c,d) = (b & d) | (c & ~d)
            a = b + Rol(a + ((b & d) | (c & ~d)) + x[1] + 0xf61e2562, 5);
            d = a + Rol(d + ((a & c) | (b & ~c)) + x[6] + 0xc040b340, 9);
            c = d + Rol(c + ((d & b) | (a & ~b)) + x[11] + 0x265e5a51, 14);
            b = c + Rol(b + ((c & a) | (d & ~a)) + x[0] + 0xe9b6c7aa, 20);
            a = b + Rol(a + ((b & d) | (c & ~d)) + x[5] + 0xd62f105d, 5);
            d = a + Rol(d + ((a & c) | (b & ~c)) + x[10] + 0x02441453, 9);
            c = d + Rol(c + ((d & b) | (a & ~b)) + x[15] + 0xd8a1e681, 14);
            b = c + Rol(b + ((c & a) | (d & ~a)) + x[4] + 0xe7d3fbc8, 20);
            a = b + Rol(a + ((b & d) | (c & ~d)) + x[9] + 0x21e1cde6, 5);
            d = a + Rol(d + ((a & c) | (b & ~c)) + x[14] + 0xc33707d6, 9);
            c = d + Rol(c + ((d & b) | (a & ~b)) + x[3] + 0xf4d50d87, 14);
            b = c + Rol(b + ((c & a) | (d & ~a)) + x[8] + 0x455a14ed, 20);
            a = b + Rol(a + ((b & d) | (c & ~d)) + x[13] + 0xa9e3e905, 5);
            d = a + Rol(d + ((a & c) | (b & ~c)) + x[2] + 0xfcefa3f8, 9);
            c = d + Rol(c + ((d & b) | (a & ~b)) + x[7] + 0x676f02d9, 14);
            b = c + Rol(b + ((c & a) | (d & ~a)) + x[12] + 0x8d2a4c8a, 20);

            // VÒNG 3: H(b,c,d) = b ^ c ^ d
            a = b + Rol(a + (b ^ c ^ d) + x[5] + 0xfffa3942, 4);
            d = a + Rol(d + (a ^ b ^ c) + x[8] + 0x8771f681, 11);
            c = d + Rol(c + (d ^ a ^ b) + x[11] + 0x6d9d6122, 16);
            b = c + Rol(b + (c ^ d ^ a) + x[14] + 0xfde5380c, 23);
            a = b + Rol(a + (b ^ c ^ d) + x[1] + 0xa4beea44, 4);
            d = a + Rol(d + (a ^ b ^ c) + x[4] + 0x4bdecfa9, 11);
            c = d + Rol(c + (d ^ a ^ b) + x[7] + 0xf6bb4b60, 16);
            b = c + Rol(b + (c ^ d ^ a) + x[10] + 0xbebfbc70, 23);
            a = b + Rol(a + (b ^ c ^ d) + x[13] + 0x289b7ec6, 4);
            d = a + Rol(d + (a ^ b ^ c) + x[0] + 0xeaa127fa, 11);
            c = d + Rol(c + (d ^ a ^ b) + x[3] + 0xd4ef3085, 16);
            b = c + Rol(b + (c ^ d ^ a) + x[6] + 0x04881d05, 23);
            a = b + Rol(a + (b ^ c ^ d) + x[9] + 0xd9d4d039, 4);
            d = a + Rol(d + (a ^ b ^ c) + x[12] + 0xe6db99e5, 11);
            c = d + Rol(c + (d ^ a ^ b) + x[15] + 0x1fa27cf8, 16);
            b = c + Rol(b + (c ^ d ^ a) + x[2] + 0xc4ac5665, 23);

            // VÒNG 4: I(b,c,d) = c ^ (b | ~d)
            a = b + Rol(a + (c ^ (b | ~d)) + x[0] + 0xf4292244, 6);
            d = a + Rol(d + (b ^ (a | ~c)) + x[7] + 0x432aff97, 10);
            c = d + Rol(c + (a ^ (d | ~b)) + x[14] + 0xab9423a7, 15);
            b = c + Rol(b + (d ^ (c | ~a)) + x[5] + 0xfc93a039, 21);
            a = b + Rol(a + (c ^ (b | ~d)) + x[12] + 0x655b59c3, 6);
            d = a + Rol(d + (b ^ (a | ~c)) + x[3] + 0x8f0ccc92, 10);
            c = d + Rol(c + (a ^ (d | ~b)) + x[10] + 0xffeff47d, 15);
            b = c + Rol(b + (d ^ (c | ~a)) + x[1] + 0x85845dd1, 21);
            a = b + Rol(a + (c ^ (b | ~d)) + x[8] + 0x6fa87e4f, 6);
            d = a + Rol(d + (b ^ (a | ~c)) + x[15] + 0xfe2ce6e0, 10);
            c = d + Rol(c + (a ^ (d | ~b)) + x[6] + 0xa3014314, 15);
            b = c + Rol(b + (d ^ (c | ~a)) + x[13] + 0x4e0811a1, 21);
            a = b + Rol(a + (c ^ (b | ~d)) + x[4] + 0xf7537e82, 6);
            d = a + Rol(d + (b ^ (a | ~c)) + x[11] + 0xbd3af235, 10);
            c = d + Rol(c + (a ^ (d | ~b)) + x[2] + 0x2ad7d2bb, 15);
            b = c + Rol(b + (d ^ (c | ~a)) + x[9] + 0xeb86d391, 21);

            h[0] += a; h[1] += b; h[2] += c; h[3] += d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EncodingMD5(byte* src, int length, long* dst)
        {
            int totalByteLen = (length + 72) & ~63;

            byte* pBuffer = stackalloc byte[totalByteLen];

            long* pClear = (long*)pBuffer;
            for (int i = 0; i < (totalByteLen >> 3); i++) pClear[i] = 0;

            System.Buffer.MemoryCopy(src, pBuffer, totalByteLen, length);

            // Padding 0x80
            pBuffer[length] = 0x80;

            // Ghi độ dài bit vào 8 byte cuối cùng
            *(ulong*)(pBuffer + totalByteLen - 8) = (ulong)(length << 3);

            // Tiến hành Hash từng block 64-byte
            uint* h = stackalloc uint[4];
            h[0] = 0x67452301; h[1] = 0xEFCDAB89; h[2] = 0x98BADCFE; h[3] = 0x10325476;

            for (int offset = 0; offset < totalByteLen; offset += 64)
            {
                HashBlock((uint*)(pBuffer + offset), h);
            }

            // Output
            uint* pDst = (uint*)dst;
            pDst[0] = h[0]; pDst[1] = h[1]; pDst[2] = h[2]; pDst[3] = h[3];
        }
    }
}
