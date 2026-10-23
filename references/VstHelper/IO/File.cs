using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.IO
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe class Win32File : IDisposable
    {
        #region Win32 API Constants
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint OPEN_ALWAYS = 4;
        internal const uint CREATE_ALWAYS = 2;

        internal const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        internal const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        
        internal const uint PAGE_READWRITE = 0x04;
        internal const uint FILE_MAP_ALL_ACCESS = 0x000F001F;
        internal const uint FILE_BEGIN = 0;
        internal const uint FILE_CURRENT = 1;
        internal const uint FILE_END = 2;
        internal const uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;
        internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        #endregion

        [System.Security.SuppressUnmanagedCodeSecurity]
        internal class Native
        {
            #region DLL Imports
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern uint GetFileAttributes(string lpFileName);



            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool WriteFile(SafeFileHandle hFile, byte* lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool ReadFile(SafeFileHandle hFile, byte* lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetEndOfFile(SafeFileHandle hFile);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, IntPtr lpProgressRoutine, IntPtr lpData, ref bool pbCancel, uint dwCopyFlags);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, uint dwFlags);


            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool ReplaceFile(string lpReplacedFileName, string lpReplacementFileName, string lpBackupFileName, uint dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern IntPtr CreateFileMapping(SafeFileHandle hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern byte* MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool UnmapViewOfFile(byte* lpBaseAddress);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FlushViewOfFile(byte* lpBaseAddress, IntPtr dwNumberOfBytesToFlush); 
            
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool FlushFileBuffers(SafeFileHandle hFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool CloseHandle(IntPtr hObject);
            #endregion
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists(string path)
        {
            uint attributes = Native.GetFileAttributes(path);

            // Nếu thuộc tính không phải INVALID và KHÔNG PHẢI là thư mục thì là file tồn tại
            return (attributes != INVALID_FILE_ATTRIBUTES &&
                   (attributes & FILE_ATTRIBUTE_DIRECTORY) == 0);
        }

        private SafeFileHandle _handle;
        private IntPtr _hMapping = IntPtr.Zero;
        private byte* _mappedPtr = null;
        public string FilePath { get; private set; }
        public long FileCapacity { get; private set; }
        public byte* MappedHandle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _mappedPtr;
        }
        
        public Win32File(string path, bool overwrite = false)
        {
            FilePath = path;
            Open(overwrite);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Open(bool overwrite = false)
        {
            if (_handle == null || _handle.IsClosed)
            {
                uint disposition = overwrite ? CREATE_ALWAYS : OPEN_ALWAYS;
                _handle = Native.CreateFile(FilePath,
                    GENERIC_READ | GENERIC_WRITE,
                    0,
                    IntPtr.Zero,
                    disposition,
                    FILE_ATTRIBUTE_NORMAL | FILE_FLAG_WRITE_THROUGH,
                    IntPtr.Zero);

                if (_handle.IsInvalid)
                    throw new Exception($"Lỗi mở file: {Marshal.GetLastWin32Error()}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Close()
        {
            Unmap();

            _handle?.Close();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Seek(long offset, uint origin = FILE_BEGIN)
        {
            if (!Native.SetFilePointerEx(_handle, offset, out long newPos, origin))
                throw new Exception("SEEK");
            return newPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Write(byte* buffer, uint size)
        {
            if (!Native.WriteFile(_handle, buffer, size, out uint written, IntPtr.Zero))
                throw new Exception("WRITE");
            return written;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Read(byte* buffer, uint size)
        {
            if (!Native.ReadFile(_handle, buffer, size, out uint read, IntPtr.Zero))
                throw new Exception("READ");
            return read;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetSize(long size)
        {
            Seek(size);
            if (!Native.SetEndOfFile(_handle))
                throw new Exception("RESIZE");

            FileCapacity = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetSize()
        {
            return Seek(0, FILE_END);
        }

        #region MAPPING

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Mapping()
        {
            Unmap(); // Đảm bảo dọn dẹp trước khi map mới

            uint size = (uint)FileCapacity;

            _hMapping = Native.CreateFileMapping(_handle, IntPtr.Zero, PAGE_READWRITE, 0, size, null);
            if (_hMapping == IntPtr.Zero) throw new Exception("MAPPING");

            _mappedPtr = Native.MapViewOfFile(_hMapping, FILE_MAP_ALL_ACCESS, 0, 0, (UIntPtr)size);
            if (_mappedPtr == null) throw new Exception("MAPPING");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* Map()
        {
            Mapping();
            return _mappedPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Unmap()
        {
            if (_mappedPtr != null) { Native.UnmapViewOfFile(_mappedPtr); _mappedPtr = null; }
            if (_hMapping != IntPtr.Zero) { Native.CloseHandle(_hMapping); _hMapping = IntPtr.Zero; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool SyncMap()
        {
            return Native.FlushViewOfFile(_mappedPtr, (IntPtr)FileCapacity);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Flush()
        {
            if (!Native.FlushFileBuffers(_handle))
                throw new Exception($"FLUSH: {Marshal.GetLastWin32Error()}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        // --- Thao tác tĩnh (Static) ---
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(string src, string dest)
        {
            bool cancel = false;
            if (!Native.CopyFileEx(src, dest, IntPtr.Zero, IntPtr.Zero, ref cancel, 0))
                throw new Exception($"COPY: {Marshal.GetLastWin32Error()}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AtomicReplace(string target, string replacement, string backup = null)
        {
            if (!Native.ReplaceFile(target, replacement, backup, 0x01, IntPtr.Zero, IntPtr.Zero))
                throw new Exception($"REPLACE: {Marshal.GetLastWin32Error()}");
        }
    }
}
