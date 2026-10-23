using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vst
{
    public unsafe static class JsonHelper
    {
        unsafe static readonly JsonType* JsonTypeTable;
        unsafe static JsonHelper()
        {
            // Tổng: 128 bytes
            IntPtr basePtr = Marshal.AllocHGlobal(128);
            JsonTypeTable = (JsonType*)basePtr;

            // JsonType
            for (int i = 0; i < 128; i++)
            {
                JsonTypeTable[i] = JsonType.None;
            }
            for (int i = 0; i < 10; i++)
            {
                JsonTypeTable[i | '0'] = JsonType.Number;
            }
            JsonTypeTable['"'] = JsonType.String;
            JsonTypeTable['{'] = JsonType.Object;
            JsonTypeTable['}'] = JsonType.CloseObject;
            JsonTypeTable['['] = JsonType.Array;
            JsonTypeTable[']'] = JsonType.CloseArray;
            JsonTypeTable['T'] = JsonType.Boolean;
            JsonTypeTable['t'] = JsonType.Boolean;
            JsonTypeTable['F'] = JsonType.Boolean;
            JsonTypeTable['f'] = JsonType.Boolean;
            JsonTypeTable['N'] = JsonType.Null;
            JsonTypeTable['n'] = JsonType.Null;
            JsonTypeTable['.'] = JsonType.Number;
            JsonTypeTable['+'] = JsonType.Number;
            JsonTypeTable['-'] = JsonType.Number;
            JsonTypeTable[':'] = JsonType.Colon;
            JsonTypeTable[','] = JsonType.Comma;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static JsonType GetType(char c) => JsonTypeTable[c];
    }
}