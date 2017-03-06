using UnityEngine;

public static class Global
{
    public static string moneyName = "小蘑菇";

    public static string maintenanceNotice = "";
    //首次登陆
    public static bool isFirstLogin = true;
    //platformString
    public static string platformString = "";
    //国旗
    public static bool isShowFlag = false;
    //开发模式
    public static bool IsDebug = true;
    //是否是离线模式
    public static bool IsOffLine = false;
    //是否单人模式
    public static bool IsSingleMode = true;
    //是否开启AI
    public static bool IsAI = false;
    //--------------以上由配置表控制---------------
    //是否打印socket log;
    public static bool IsTraceSocketLog = true;

    //是否显示micphone
    public static bool IsShowMicphone = true;

    //是否开启热更新
    public static bool IsOpenHotUpdate = false;
    //是否fps调试显示
    public static bool IsOpenFPSCounter = false;
    //是否使用AssetBundle
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    public static bool IsUseAB = false;
#else
    public static bool IsUseAB = true;
#endif
    /// <summary>
    /// 加载配置表的地址
    /// </summary>
    public static string cdnURL;
    public static string localCdnURL;
    public static string homeURL;

    /// <summary>
    /// 系统公告
    /// </summary>
    public static string noticeContent;
    /// <summary>
    /// 字体
    /// </summary>
    public static Font font;

    public static bool isShowProjectilePath = false;
}
