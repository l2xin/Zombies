using System;
using System.Timers;

public class TimeManager
{
    public const uint TIME_ZONE = 8;
    private static long _serverTime;
    private static DateTime _startDate;
    private static DateTime _serverDate;

    private static Timer _tickTimer;

    public static long ServerTime
    {
        get
        {
            return _serverTime;
        }
    }

    public static DateTime ServerDate
    {
        get
        {
            return _serverDate;
        }
    }

    public static void Setup()
    {
        _serverTime = 0;
        _startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(TIME_ZONE);
        _serverDate = _startDate.AddSeconds(_serverTime);

        if (_tickTimer == null)
        {
            _tickTimer = new Timer(1000);
            _tickTimer.Elapsed += new ElapsedEventHandler(OnTimer);
            _tickTimer.Start();
        }
    }

    public static void SetTime(long time)
    {
        DateTime oldDate = _serverDate;
        _serverTime = time;
        _serverDate = _startDate.AddSeconds(_serverTime);
    }

    private static void OnTimer(object obj, ElapsedEventArgs evt)
    {
        DateTime oldDate = _serverDate;
        _serverTime = _serverTime + 1;
        _serverDate = _serverDate.AddSeconds(1);
    }
}
