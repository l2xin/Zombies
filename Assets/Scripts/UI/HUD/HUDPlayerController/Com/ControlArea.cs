using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 输入区域模拟
/// 1.接受区域输入
/// 2.控制模拟摇杆
/// </summary>
public class ControlArea : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler 
{


	public enum STATE
	{
		pointerUp,
		pointerDown,
		beginDrag,
		drag,
	}
	public string AreaName = "";
	public STATE state = STATE.pointerUp;
	/// <summary>
	/// 受控摇杆
	/// </summary>
	public HRYJoyStick STICK;


	void Awake()
	{
//		PlayerController.Instance.Register (this);
	}

	#region -> 事件 <-
	public void OnPointerDown(PointerEventData eventData)
	{
//		Debug.Log (name+ ",OnPointerDown");
		this.state = STATE.pointerDown;
		STICK.OnPointerDown (eventData);
	}
	public void OnPointerUp(PointerEventData eventData)
	{
		this.state = STATE.pointerUp;
		STICK.OnPointerUp (eventData);
	}
	public void OnBeginDrag(PointerEventData eventData)
	{
		this.state = STATE.beginDrag;
		STICK.OnBeginDrag (eventData);
	}
	public void OnDrag(PointerEventData eventData)
	{
		this.state = STATE.drag;
		STICK.OnDrag (eventData);
	}
	#endregion
}
