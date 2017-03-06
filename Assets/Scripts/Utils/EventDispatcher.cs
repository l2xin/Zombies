using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventDispatcher
{
    private Dictionary<string, System.Action<BaseEvent>> _eventDictionary = new Dictionary<string, System.Action<BaseEvent>>();

    public bool HasEventListener(string type)
    {
        return _eventDictionary.ContainsKey(type);
    }

    public void AddEventListener(string type, System.Action<BaseEvent> listener)
    {
        if(_eventDictionary.ContainsKey(type))
        {
            System.Action<BaseEvent> function = _eventDictionary[type];
            function -= listener;
            function += listener;
            _eventDictionary.Remove(type);
            _eventDictionary.Add(type, function);
        }
        else
        {
            _eventDictionary.Add(type, listener);
        }
    }

    public void RemoveAllEventListender()
    {
        _eventDictionary.Clear();
    }

    public void DispatchEvent(BaseEvent evt)
    {
        if(_eventDictionary.ContainsKey(evt.Type))
        {
            System.Action<BaseEvent> function = _eventDictionary[evt.Type];
            function.Invoke(evt);
        }
    }

    public void RemoveEventListener(string type, System.Action<BaseEvent> listener)
    {
        if (_eventDictionary.ContainsKey(type))
        {
            System.Action<BaseEvent> function = _eventDictionary[type];
            function -= listener;
            _eventDictionary.Remove(type);
            if(function != null)
            {
                _eventDictionary.Add(type, function);
            }
        }
    }
}
