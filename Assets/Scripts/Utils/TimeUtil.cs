using System;
/**
 * anthor J
 * 
 */
public class TimeUtil
{
    private const int TIME_ZONE = 8;
    public static DateTime ParseToDate(uint time)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(TIME_ZONE).AddSeconds(time);
    }

    public static int GetSecondsTime(uint time)
    {
        DateTime date = ParseToDate(time);
        return date.Hour * 3600 + date.Minute * 60 + date.Second;
    }

    public static string ParseToDateString(uint time)
    {
        DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(TIME_ZONE).AddSeconds(time);
        return date.Year + "年" + date.Month + "月" + date.Day + "日 " + StringUtil.RenewZero(date.Hour.ToString(), 2) + ":" + StringUtil.RenewZero(date.Minute.ToString(), 2) + ":" + StringUtil.RenewZero(date.Second.ToString(), 2);
    }
}