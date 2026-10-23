using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    public class SystemClock
    {
        private volatile bool _isRunning = false; // Dùng volatile để an toàn đa luồng

        int _ten, _sec, _min, _hour, _day;

        public event Action OnTick;
        public event Action OnSecond;
        public event Action OnMinute;
        public event Action OnHour;
        public event Action OnDay;

        public int Milliseconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ten;
        }
        public int Seconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sec;
        }
        public int Minutes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _min;
        }
        public int Hours
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hour;
        }
        public int Days
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _day;
        }

        public void Start()
        {
            if (_isRunning) return; // Tránh chạy lặp
            _isRunning = true;
            _ten = _sec = _min = _hour = _day = 0;

            Task.Run(async () => {
                // Sửa logic: Chạy khi _isRunning là true
                while (_isRunning)
                {
                    await Task.Delay(10); // Nhịp 10ms đúng ý bạn

                    OnTick?.Invoke();

                    // 10ms * 100 = 1000ms = 1 giây
                    if (++_ten >= 100)
                    {
                        _ten = 0;
                        _sec++;
                        OnSecond?.Invoke();

                        if (_sec >= 60)
                        {
                            _sec = 0;
                            _min++;
                            OnMinute?.Invoke();

                            if (_min >= 60)
                            {
                                _min = 0;
                                _hour++;
                                OnHour?.Invoke();

                                // Một ngày có 24 giờ, không phải 60
                                if (_hour >= 24)
                                {
                                    _hour = 0;
                                    _day++;
                                    OnDay?.Invoke();
                                }
                            }
                        }
                    }
                }
            });
        }

        public void Stop() => _isRunning = false;
    }
}
