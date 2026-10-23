using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vst
{
    /// <summary>
    /// Struct StringWriter đại diện cho một bộ ghi các loại dữ liệu
    /// vào RAM mà không cho GC can thiệp, giúp tăng tốc độ ghi dữ liệu
    /// </summary>
    public unsafe struct StringWriter
    {
        public byte* Start;
        public byte* Current;
        public byte* End;

        /// <summary>
        /// Hàm này trả về số lượng byte đã được ghi vào RAM 
        /// </summary>
        public int CurrentPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]

            get => (int)(Current - Start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int offset)
        {
            Current = Start + offset;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringWriter(byte* handle, int maxLen)
        {
            Current = Start = handle;
            End = Current + maxLen;
        }

        /// <summary>
        /// Hàm này ghi một giá trị kiểu T vào bộ nhớ tại vị 
        /// trí hiện tại của con trỏ Current
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Endian<T>(T value)
        {
            *(T*)Current = value;
            Current += sizeof(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char c) => *Current++ = (byte)c;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte b) => *Current++ = b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write2Digit(int i)
        {
            *(ushort*)Current = StringHelper.DecDigitTable[i];
            Current += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            fixed (char* pSrc = s)
            {
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
                            StringHelper.WriteSurrogate(ref Current, c, *src++);
                            continue;
                        }
                    }
                    if (c < 128)
                    {
                        var b = StringHelper.EscapeTable[c];
                        if (b != 0)
                        {
                            *Current++ = (byte)'\\';
                            *Current++ = b;
                            continue;
                        }
                    }
                    StringHelper.WriteCodePoint(ref Current, c);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* s, int len)
        {
            Buffer.MemCopy(s, Current, len);
            Current += len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            if (value == 0) { *Current++ = (byte)'0'; return; }
            if (value == long.MinValue)
            {
                fixed (char* p = "-9223372036854775808") { for (int i = 0; i < 20; i++) *Current++ = (byte)p[i]; }
                return;
            }
            if (value < 0) { *Current++ = (byte)'-'; value = -value; }

            byte* sp = End;
            while (value >= 100)
            {
                long v = value / 100;
                int r = (int)(value - v * 100);
                *(--sp) = (byte)r;

                value = v;
            }

            if (value >= 10) Write2Digit((int)value);
            else *Current++ = (byte)(value | 0x30);

            while (sp < End) Write2Digit(*sp++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value, int maxPrecision = 9)
        {
            if (value < 0) { *Current++ = (byte)'-'; value = -value; }

            long integral = (long)value;
            Write(integral);

            double fractional = value - integral;
            if (fractional < 1e-15) return;

            byte* lastNonZero = Current;
            *Current++ = (byte)'.';

            for (int i = 0; i < maxPrecision; i++)
            {
                fractional *= 10;
                int digit = (int)fractional;

                if (digit != 0)
                {
                    lastNonZero = Current;
                }

                *Current++ = (byte)(digit | '0');


                fractional -= digit;
                if (fractional < 1e-12) break;
            }

            Current = lastNonZero;
            if (*Current != '.') ++Current;
        }

    }
}
