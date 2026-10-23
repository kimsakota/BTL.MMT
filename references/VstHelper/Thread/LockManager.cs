using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vst
{
    public unsafe struct LockEngine
    {
        private int _state;
        private IntPtr _nativeEvent; // Handle của Windows Event

        // Khởi tạo Handle khi Collection được tạo ra
        public void Initialize()
        {
            _state = 0;
            _nativeEvent = CreateEvent(IntPtr.Zero, false, false, null);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        // --- CỔNG ĐỌC (SHARED) ---
        public void EnterRead()
        {
            int spin = 0;
            while (true)
            {
                int s = _state;
                if (s >= 0 && Interlocked.CompareExchange(ref _state, s + 1, s) == s)
                    return;

                if (spin++ < 1000) Thread.SpinWait(20);
                else
                {
                    // Thay vì Monitor.Wait, ta dùng Native Wait
                    WaitForSingleObject(_nativeEvent, 1);
                    spin = 0;
                }
            }
        }

        public void ExitRead()
        {
            if (Interlocked.Decrement(ref _state) == 0)
            {
                SetEvent(_nativeEvent); // Đánh thức thằng đang chờ
            }
        }

        // --- CỔNG GHI (EXCLUSIVE) ---
        public void EnterWrite()
        {
            int spin = 0;
            while (Interlocked.CompareExchange(ref _state, -1, 0) != 0)
            {
                if (spin++ < 1000) Thread.SpinWait(20);
                else
                {
                    WaitForSingleObject(_nativeEvent, 1);
                    spin = 0;
                }
            }
        }

        public void ExitWrite()
        {
            Interlocked.Exchange(ref _state, 0);
            SetEvent(_nativeEvent);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        public void Dispose()
        {
            if (_nativeEvent != IntPtr.Zero)
            {
                CloseHandle(_nativeEvent);
                _nativeEvent = IntPtr.Zero;
            }
        }
        public static LockEngine* Create()
        {
            var p = Memory.Malloc<LockEngine>();
            *p = new LockEngine();

            p->Initialize();
            return p;
        }

        public static void Destroy(LockEngine* engine)
        {
            engine->Dispose();
            Memory.Free(engine);
        }
    }

    // Lớp bọc cho quyền Đọc
    public unsafe struct LockingRead : IDisposable
    {
        private LockEngine* _engine;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LockingRead(LockEngine* engine)
        {
            _engine = engine;
            _engine->EnterRead();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _engine->ExitRead();
    }

    // Lớp bọc cho quyền Ghi
    public unsafe struct LockingWrite : IDisposable
    {
        private LockEngine* _engine;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LockingWrite(LockEngine* engine)
        {
            _engine = engine;
            _engine->EnterWrite();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _engine->ExitWrite();
    }
}
