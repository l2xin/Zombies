using System;
using System.Text;
/**
 * anthor J
 * 
 */
public class StringUtil
{
    public static string RenewZero(string str, uint len)
    {
        while(str.Length < len)
        {
            str = "0" + str;
        }
        return str;
    }

    public static string WatchFormat(uint time, bool showHour = true)
    {
        string h = RenewZero(Math.Floor(time / 3600f).ToString(), 2);
        string m;
        string s = RenewZero((time % 60f).ToString(), 2);

        if (showHour)
        {
            m = RenewZero((Math.Floor(time / 60f) % 60f).ToString(), 2);
            return h + ":" + m + ":" + s;
        }
        else
        {
            m = RenewZero(Math.Floor(time / 60f).ToString(), 2);
            return m + ":" + s;
        }
    }

    public static string TimeFormat(uint time)
    {
        return Math.Floor(time / 3600f) + "小时" + Math.Floor(time / 60f) % 60f + "分钟";
    }

    public static string CheckStrLen(string str, int maxChars)
    {
        byte[] byteStr = Encoding.Default.GetBytes(str);
        if (maxChars > 0 && byteStr.Length > maxChars)
        {
            str = str.Substring(0, str.Length - 1);
            while (Encoding.Default.GetBytes(str).Length > maxChars)
                str = str.Substring(0, str.Length - 1);
        }
        return str;
    }
}