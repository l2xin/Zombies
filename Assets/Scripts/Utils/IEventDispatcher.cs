public interface IEventDispatcher
{
    bool HasEventListener(string type);
    void AddEventListener(string type, System.Action<BaseEvent> listener);
    void RemoveAllEventListender();
    void DispatchEvent(BaseEvent evt);
    void RemoveEventListener(string type, System.Action<BaseEvent> listener);
}
