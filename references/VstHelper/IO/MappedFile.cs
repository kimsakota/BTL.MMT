using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Data
{
    using System;
    using System.Runtime.InteropServices;

    public static unsafe class MappedFile
    {
        #region NATIVE CONSTANTS
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;

        public const uint FILE_SHARE_DELETE = 0x00000004;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        
        public const uint OPEN_EXISTING = 3;
        public const uint OPEN_ALWAYS = 4;
        public const uint CREATE_ALWAYS = 2;

        // Nhóm Attributes & Flags
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;

        public const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;
        public const uint MOVEFILE_WRITE_THROUGH = 0x00000008; // Ép ghi xuống đĩa trước khi báo xong

        public const uint PAGE_READWRITE = 0x04;
        public const uint FILE_MAP_ALL_ACCESS = 0xF001F;

        public const int INVALID_HANDLE_VALUE = -1;
        #endregion

        #region KERNEL32 API
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(
            string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect,
            uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern byte* MapViewOfFile(
            IntPtr hFileMappingObject, uint dwDesiredAccess,
            uint dwFileOffsetHigh, uint dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(byte* lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FlushFileBuffers(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetFilePointerEx(
            IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEndOfFile(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(
            IntPtr hFile, byte* lpBuffer, uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetFileSize(IntPtr hFile, out uint lpFileSizeHigh);

        /// <summary>
        /// Đẩy các trang bộ nhớ đã thay đổi (dirty pages) xuống cache của hệ điều hành.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FlushViewOfFile(byte* lpBaseAddress, IntPtr dwNumberOfBytesToFlush);

        #endregion

        #region EXTENSIONS

        /// <summary>
        /// Thay thế hoàn toàn Stream.SetLength. Nở file vật lý trên đĩa.
        /// </summary>
        public static bool SetLength(IntPtr hFile, long length)
        {
            long newPtr;
            if (!SetFilePointerEx(hFile, length, out newPtr, 0 /* FILE_BEGIN */)) return false;
            return SetEndOfFile(hFile);
        }

        /// <summary>
        /// Lấy độ dài file hiện tại
        /// </summary>
        public static long GetLength(IntPtr hFile)
        {
            uint high;
            uint low = GetFileSize(hFile, out high);
            return ((long)high << 32) | low;
        }

        #endregion
    }
}
