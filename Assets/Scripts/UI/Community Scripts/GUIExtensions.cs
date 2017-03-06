using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public static class GUIExtensions
{
	public static void AddListener(this EventTrigger eventTrigger, EventTriggerType type, UnityAction<BaseEventData> action)
	{
		if (eventTrigger.triggers == null)
		{
			eventTrigger.triggers = new List<EventTrigger.Entry>();
		}
		var entry = eventTrigger.triggers.Find(e => e.eventID == type);
		
		if (entry == null)
		{
			entry = new EventTrigger.Entry();
			entry.eventID = type;
			entry.callback = new EventTrigger.TriggerEvent();
			
			eventTrigger.triggers.Add(entry);
		}
		entry.callback.AddListener(action);
		
	}

	public static void SetSelected(this EventSystem eventSystem, MonoBehaviour selected)
	{
		var pointer = new BaseEventData(eventSystem);
		eventSystem.SetSelectedGameObject(selected.gameObject, pointer);
	}
	
	public static void SetSelected(this EventSystem eventSystem, GameObject selected)
	{
		var pointer = new BaseEventData(eventSystem);
		eventSystem.SetSelectedGameObject(selected, pointer);
	}
}