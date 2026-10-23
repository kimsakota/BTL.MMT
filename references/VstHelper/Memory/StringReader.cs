using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Vst
{
    /// <summary>
    /// Struct StringReader đại diện cho một bộ đọc chuỗi từ bộ nhớ,
    /// cung cấp các phương thức để đọc dữ liệu
    /// </summary>
    public unsafe struct StringReader
    {
        private byte* src;
        private int length;
        public byte* Handle => src;
        public int Length => length;
        public StringReader(byte* src, int length)
        {
            this.src = src;
            this.length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(bool unescape)
        {
            if (!unescape)
            {
                return Encoding.UTF8.GetString(src, length);
            }
            byte* buf = stackalloc byte[length];
            byte* dst = buf;

            StringHelper.Unescape(ref dst, src, length);

            int actualLength = (int)(dst - buf);
            return Encoding.UTF8.GetString(buf, actualLength);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetLong()
        {
            byte* buffer = stackalloc byte[length];

            bool neg = false;
            int srcIdx = 0;
            int dstIdx = 0;

            if (src[0] == '-')
            {
                neg = true;
                srcIdx = 1;
            }

            for (; srcIdx < length; srcIdx++)
            {
                byte b = src[srcIdx];
                if (b == 0) continue;
                
                buffer[dstIdx++] = b;
            }
            int cleanLen = dstIdx;

            long integral = 0;
            for (int i = 0; i < cleanLen; i++)
            {
                byte b = buffer[i];

                int digit = b & 15;
                if (digit < 0 || digit > 9) return 0; // Ký tự không hợp lệ

                integral = (integral << 1) + (integral << 3) + digit;
            }

            return (neg ? -integral : integral);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDouble()
        {
            byte* buffer = stackalloc byte[length];

            bool hasE = false;
            bool hasDot = false;
            bool neg = false;
            int srcIdx = 0;
            int dstIdx = 0;

            if (src[0] == '-')
            {
                neg = true;
                srcIdx = 1;
            }

            for (; srcIdx < length; srcIdx++)
            {
                byte b = src[srcIdx];
                if (b == 0) continue;
                if (b == 'e' || b == 'E') hasE = true;
                if (b == '.') hasDot = true;
                buffer[dstIdx++] = b;
            }
            int cleanLen = dstIdx;

            if (hasE || cleanLen >= 19)
            {
                // Vẫn phải dùng string ở đây nếu không có Utf8Parser
                // Nhưng ít nhất buffer đã nằm trên stack
                string s = Encoding.ASCII.GetString(buffer, cleanLen);
                if (double.TryParse(neg ? "-" + s : s, out double d))
                    return d;
                return 0;
            }

            long integral = 0;
            long fractional = 0;
            long divisor = 1;
            bool parsingFraction = false;

            for (int i = 0; i < cleanLen; i++)
            {
                byte b = buffer[i];
                if (b == '.')
                {
                    parsingFraction = true;
                    continue;
                }

                int digit = b - '0';
                if (digit < 0 || digit > 9) return 0; // Ký tự không hợp lệ

                if (!parsingFraction)
                {
                    integral = integral * 10 + digit;
                }
                else
                {
                    fractional = fractional * 10 + digit;
                    divisor *= 10;
                }
            }

            if (neg) integral = -integral;

            if (!hasDot || divisor == 1)
            {
                return integral;
            }    

            double result = (double)integral;
            double fracPart = (double)fractional / divisor;

            return (neg ? result - fracPart : result + fracPart);
        }
    }
}
