using System;
using System.CodeDom;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace Vst
{
    public unsafe static class StringHelper
    {
        public static readonly byte* LowerCaseTable;
        public static readonly byte* EscapeTable;
        public static readonly byte* UnescapeTable;
        public static readonly ushort* DecDigitTable;

        [ThreadStatic]
        private static byte* _threadBuffer; // Lưu con trỏ unmanaged cho mỗi thread
        private const int MaxStackLimit = 64 * 1024; // 64KB
        private static byte* GetThreadBuffer()
        {
            if (_threadBuffer == null)
            {
                _threadBuffer = (byte*)Marshal.AllocHGlobal(MaxStackLimit);
            }
            return _threadBuffer;
        }

        static StringHelper()
        {
            // Tổng: 256 + 256 + 256 + 256 = 1024 bytes
            IntPtr basePtr = Marshal.AllocHGlobal(1024);
            LowerCaseTable = (byte*)basePtr;
            EscapeTable = LowerCaseTable + 256;
            UnescapeTable = EscapeTable + 256;
            DecDigitTable = (ushort*)(UnescapeTable + 256);

            for (int i = 0; i < 256; i++)
            {
                LowerCaseTable[i] = 0;
                EscapeTable[i] = 0;
                UnescapeTable[i] = (byte)i;
            }

            for (int i = 'A'; i <= 'Z'; i++) LowerCaseTable[i] = 0x20;


            EscapeTable['\"'] = (byte)'\"'; EscapeTable['\\'] = (byte)'\\';
            EscapeTable['\b'] = (byte)'b'; EscapeTable['\f'] = (byte)'f';
            EscapeTable['\n'] = (byte)'n'; EscapeTable['\r'] = (byte)'r';
            EscapeTable['\t'] = (byte)'t';

            // --- 2. JSON Unescape Map ---
            UnescapeTable['\"'] = (byte)'\"'; UnescapeTable['\\'] = (byte)'\\';
            UnescapeTable['/'] = (byte)'/'; UnescapeTable['b'] = (byte)'\b';
            UnescapeTable['f'] = (byte)'\f'; UnescapeTable['n'] = (byte)'\n';
            UnescapeTable['r'] = (byte)'\r'; UnescapeTable['t'] = (byte)'\t';

            // --- 3. Fast 2-Digit UTF-8 Look-up ---
            for (int i = 0; i < 100; i++)
            {
                byte d1 = (byte)((i / 10) | 0x30);
                byte d2 = (byte)((i % 10) | 0x30);
                DecDigitTable[i] = (ushort)((d2 << 8) | d1);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToLowerCase(byte* handle, int length)
        {
            byte* p = handle;
            int rem = length;

            // Unroll vòng lặp 4 lần để giảm bớt lệnh nhảy (jmp)
            while (rem >= 4)
            {
                p[0] |= LowerCaseTable[p[0]];
                p[1] |= LowerCaseTable[p[1]];
                p[2] |= LowerCaseTable[p[2]];
                p[3] |= LowerCaseTable[p[3]];
                p += 4;
                rem -= 4;
            }

            // Xử lý các byte lẻ còn lại
            while (rem > 0)
            {
                *p |= LowerCaseTable[*p];
                p++;
                rem--;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static long ToLong(this string s)
        //{
        //    long a = 0;
        //    bool minus = (s[0] == '-');

        //    int i = minus ? 1 : 0;
        //    while (i < s.Length)
        //    {
        //        char c = s[i++];
        //        switch (c)
        //        {
        //            case '+':
        //            case '-':
        //            case '.':
        //                return 0;
        //        }
        //        if (JsonHelper.GetType(c) != JsonType.Number)
        //        {
        //            return 0;
        //        }    
        //        a = (a << 1) + (a << 3) + (c & 15);
        //    }
        //    return minus ? -a : a;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EncodeUTF8(byte* buffer, string s)
        {
            fixed (char* pSrc = s)
            {
                byte* dst = buffer;

                char* src = pSrc;
                char* end = pSrc + s.Length;
                while (src < end)
                {
                    char c = *src++;
                    if (c >= 0xD800 && c <= 0xDBFF && src < end)
                    {
                        char low = *src;
                        if (low >= 0xDC00 && low <= 0xDFFF)
                        {
                            WriteSurrogate(ref dst, c, *src++);
                            continue;
                        }
                    }
                    WriteCodePoint(ref dst, c);
                }

                return (int)(dst - buffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EncodeUTF8KeepEscaped(ref byte* dst, char* src, int srcLen)
        {
            byte* start = dst;
            char* s = src;
            char* end = src + srcLen;

            while (s < end)
            {
                char c = *s++;

                // Một câu if duy nhất cho 99% dữ liệu JSON (ASCII)
                if (c < 0x80)
                {
                    *dst++ = (byte)c;
                }
                else if (c < 0x800)
                {
                    *dst++ = (byte)(0xC0 | (c >> 6));
                    *dst++ = (byte)(0x80 | (c & 0x3F));
                }
                else
                {
                    *dst++ = (byte)(0xE0 | (c >> 12));
                    *dst++ = (byte)(0x80 | ((c >> 6) & 0x3F));
                    *dst++ = (byte)(0x80 | (c & 0x3F));
                }
            }


            return (int)(dst - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAscii(string s)
        {
            fixed (char* p = s)
            {
                int len = s.Length;
                int i = 0;
                ulong* lptr = (ulong*)p;

                while (i <= len - 4)
                {
                    if ((*lptr & 0xFF80FF80FF80FF80L) != 0) return false;
                    lptr++;
                    i += 4;
                }

                for (; i < len; i++)
                {
                    if ((p[i] & 0x80) != 0) return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCodePoint(ref byte* d, int cp)
        {
            if (cp < 0x80) *d++ = (byte)cp;
            else if (cp < 0x800)
            {
                *d++ = (byte)(0xc0 | (cp >> 6));
                *d++ = (byte)(0x80 | (cp & 0x3f));
            }
            else if (cp <= 0xFFFF)
            {
                *d++ = (byte)(0xe0 | (cp >> 12));
                *d++ = (byte)(0x80 | ((cp >> 6) & 0x3f));
                *d++ = (byte)(0x80 | (cp & 0x3f));
            }
            else
            {
                *d++ = (byte)(0xf0 | (cp >> 18));
                *d++ = (byte)(0x80 | ((cp >> 12) & 0x3f));
                *d++ = (byte)(0x80 | ((cp >> 6) & 0x3f));
                *d++ = (byte)(0x80 | (cp & 0x3f));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ParseUnicodeEscape(ref byte* src)
        {
            uint ch1 = HexHelper.GetByte(*src++);
            uint ch2 = HexHelper.GetByte(*src++);
            uint ch3 = HexHelper.GetByte(*src++);
            uint ch4 = HexHelper.GetByte(*src++);
            return (int)((ch1 << 12) | (ch2 << 8) | (ch3 << 4) | ch4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unescape(ref byte* dst, byte* src, int length)
        {
            byte* end = src + length;
            while (src < end)
            {
                byte b = *src++;
                if (b == '\\' && src < end)
                {
                    byte next = *src++;
                    if (next == 'u')
                    {
                        if (src + 4 <= end)
                        {
                            int cp = ParseUnicodeEscape(ref src);
                            if (cp >= 0xD800 && cp <= 0xDBFF && src + 6 <= end && src[0] == '\\' && src[1] == 'u')
                            {
                                src += 2;
                                int low = ParseUnicodeEscape(ref src);
                                WriteSurrogate(ref dst, (char)cp, (char)low);
                            }
                            else WriteCodePoint(ref dst, cp);
                        }
                    }
                    else *dst++ = UnescapeTable[next];
                }
                else *dst++ = b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSurrogate(ref byte* dst, char high, char low)
        {
            int cp = (((high & 0x3FF) << 10) | (low & 0x3FF)) + 0x10000;
            *dst++ = (byte)(0xf0 | (cp >> 18));
            *dst++ = (byte)(0x80 | ((cp >> 12) & 0x3f));
            *dst++ = (byte)(0x80 | ((cp >> 6) & 0x3f));
            *dst++ = (byte)(0x80 | (cp & 0x3f));
        }

    }
}
