public class BaseEvent
{
    public static string DEBUG = "debug";
    public static string COMPLETE = "Complete";
    public static string UPDATE = "Update";
    public static string START = "Start";
    protected string _type;
    protected object _eventObj;

    public BaseEvent(string type, object eventObj = null)
    {
        _type = type;
        _eventObj = eventObj;
    }

    public string Type
    {
        get
        {
            return _type;
        }
    }

    public object EventObj
    {
        get
        {
            return _eventObj;
        }
    }
}
