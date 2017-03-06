using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 模拟摇杆,主要控制器
/// 全部采用 Position 坐标
/// </summary>
public class HRYJoyStick : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
{
	public enum TYPE
	{
		/// <summary>
		/// 十字键 + 拖拽
		/// </summary>
		Cross,
		/// <summary>
		/// 重定位 + 拖拽
		/// </summary>
		Anchor,
		/// <summary>
		/// 从定位 + 更随
		/// </summary>
		Follow,
	}

	public enum STATE
	{
		pointerUp,
		pointerDown,

		beginDrag,
		drag,
	}

	public string JoyName = "";
	public TYPE type = TYPE.Anchor;
	public STATE state = STATE.pointerUp;
	public Image Joy;
	public Image Stick;
	public Image JoyEdge;
	public UIPresentTool UIStick;
	public float Inner = 30;
	public float Outer = 150;
    public bool Delay = false;
    float MaxDelay = 0.3f;


    #region -> UnityEvent <-
    [System.Serializable] public class OnPointerDownHandler : UnityEvent{}
	[System.Serializable] public class OnPointerUpHandler : UnityEvent{}
	[System.Serializable] public class OnBeginDragHandler : UnityEvent{}
	[System.Serializable] public class OnDragHandler : UnityEvent{}

	public OnPointerDownHandler PointerDown = new OnPointerDownHandler();
	public OnPointerUpHandler PointerUp = new OnPointerUpHandler ();
	public OnBeginDragHandler BeginDrag = new OnBeginDragHandler ();
	public OnDragHandler Drag = new OnDragHandler ();

	private UnityAction UAPointerDown;
	private UnityAction UAPointerUp;
	private UnityAction UABeginDrag;
	private UnityAction UADrag;


	public OnPointerDownHandler SubPointerDown = new OnPointerDownHandler ();
	public OnPointerUpHandler SubPointerUp = new OnPointerUpHandler ();
	public OnBeginDragHandler SubBeginDrag = new OnBeginDragHandler ();
	public OnDragHandler SubDrag = new OnDragHandler ();

	#endregion
	private float mDistence = 0;
	public float DISTANCE
	{
		get{return mDistence;}
		set{mDistence = value;}
	}
	private Vector3 mDirection = Vector3.forward;
	public Vector3 DIRECTION
	{
		get{return mDirection;}
		set{mDirection = value;}
	}

	public Vector3 ANCHOR = Vector3.zero;
	public Vector3 OFFSET = Vector3.zero;
	public float Percent = 0;
	public Vector3 DRAGAMOUNT = Vector3.zero;
	public Vector3 SPACE = Vector3.zero;
	public Vector3 INPUTAMOUNT = Vector3.zero;

	public float HORIZENTAL
	{
		get
		{
			return DRAGAMOUNT.x;
		}
	}

	public float VERTICAL
	{
		get
		{
			return DRAGAMOUNT.y;
		}
	}

	public Vector3 StartAnchor = Vector3.zero;
	Transform TJoy;
	Transform TStick;
	Transform TEdge;

	public void Online()
	{
		TJoy = transform;
		Joy = TJoy.gameObject.GetComponent<Image> ();
		TStick = transform.Find ("Stick");
		Stick = TStick.gameObject.GetComponent<Image> ();
		TEdge = transform.Find ("JoyEdge");
		JoyEdge = TEdge.gameObject.GetComponent<Image> ();
		Inner = 30;
		Outer = 150;
		type = TYPE.Cross;
	}
	void Start()
	{
		StartAnchor = Joy.rectTransform.position;
	}

	#if UNITY_EDITOR
	void FixedUpdate()
	{
		if(state == STATE.pointerUp)
		{
			float KeyBoardH = Input.GetAxis ("Horizontal");
			float KeyBoardV = Input.GetAxis ("Vertical");
			SimulateDrag(KeyBoardH,KeyBoardV);
		}
	}

	void SimulateDrag(float x,float y)
	{
		OFFSET = new Vector3 (x,y);
		DIRECTION = OFFSET.normalized;
		DRAGAMOUNT = DIRECTION;
	}
	#endif

	#region -> 事件 <-
	public void OnPointerDown(PointerEventData eventData)
	{
		Begin (eventData.position);
		this.state = STATE.pointerDown;
		this.PointerDown.Invoke ();
		this.SubPointerDown.Invoke ();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		End();
		this.state = STATE.pointerUp;
		this.PointerUp.Invoke ();
		this.SubPointerUp.Invoke ();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		this.state = STATE.beginDrag;
	}

	public void OnDrag(PointerEventData eventData)
	{
		this.state = STATE.drag;
		this.Drag.Invoke ();
		this.SubDrag.Invoke ();
		Move (eventData.position);
	}
	#endregion

	/// <summary>
	/// 定位 模拟摇杆
	/// </summary>
	public void Begin(Vector2 _ScreenPoint)
	{		
		CheckOffsetInfo (GetRectlocalposition(_ScreenPoint));
		switch(type)
		{
		    case TYPE.Cross:
			        Stick.rectTransform.localPosition = GetStickOffset (OFFSET);
			        break;
			case TYPE.Anchor:
			    Joy.rectTransform.localPosition = GetJoyAnchor (OFFSET);
		        break;
			case TYPE.Follow:
			    Joy.rectTransform.localPosition = GetJoyAnchor (OFFSET);
		        break;
		}
		JoyEdge.rectTransform.up = DIRECTION;
	}

	public void Move(Vector3 screenPoint)	
	{
		CheckOffsetInfo (GetRectlocalposition(screenPoint));
		switch(type)
		{
			case TYPE.Cross:
			    Stick.rectTransform.localPosition = GetStickOffset (OFFSET);
			    break;
			case TYPE.Anchor:
			    Stick.rectTransform.localPosition = GetStickOffset (OFFSET);
			    break;
		    case TYPE.Follow:
			    Joy.rectTransform.localPosition = GetFollowSpace ();
			    Stick.rectTransform.localPosition = GetStickOffset (OFFSET);
			    break;
		}
		JoyEdge.rectTransform.up = DIRECTION;
	}

	public void End()
	{
        if (this.Stick != null && this.Stick.rectTransform != null)
        {
			this.Stick.rectTransform.localPosition = Vector3.zero;
        }
		this.DISTANCE = 0;
		this.DRAGAMOUNT = Vector3.zero;
		this.SPACE = Vector3.zero;
	}

	#region -> 坐标判断 <-
	/// <summary>
	/// 计算 ScreenPoint and Joy 的偏移数据
	/// </summary>
	private void CheckOffsetInfo(Vector3 _Rectlocalposition)
	{
		ANCHOR = _Rectlocalposition;
		OFFSET = ANCHOR - Joy.rectTransform.localPosition;
		DISTANCE = OFFSET.magnitude;
		DIRECTION = OFFSET.normalized;
	}
	/// <summary>
	/// 计算圈的数据
	/// 返回 Stick 合适的偏移坐标
	/// </summary>
	Vector3 GetStickOffset(Vector3 _offset)
	{
		if (DISTANCE > Inner || DRAGAMOUNT.magnitude > 0) 
		{
			if (DISTANCE > Outer) 
			{
				DRAGAMOUNT = DIRECTION * Outer;
			} 
			else 
			{
				DRAGAMOUNT = _offset;
			}
		}
		Percent = DRAGAMOUNT.magnitude / Outer;
		return DRAGAMOUNT;
	}
	/// <summary>
	/// 返回 Joy 的偏移量
	/// </summary>
	Vector2 GetJoyAnchor(Vector3 _offset)
	{
		return ANCHOR;
	}
	/// <summary>
	/// 获取跟随后的摇杆位置
	/// </summary>
	Vector3 GetFollowSpace()
	{
		float space = DISTANCE - Outer;
		if (DISTANCE > Outer) 
		{
			if (space > 0) 
			{
				//超出范围跟随修正
				SPACE = space * DIRECTION;
			}
		} 
		else 
		{
			SPACE = Vector3.zero;
		}
		return Joy.rectTransform.localPosition + SPACE;
	}
	//----------------------------------------
	public Vector2 GetRectlocalposition(Vector2 _screenPoint)
	{
		Vector2 Rectlocalposition = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (UIManager.RectTransform,_screenPoint, UIManager.UICamera, out Rectlocalposition);
		return Rectlocalposition;
	}
	#endregion

	public void Change(TYPE _type)
	{
		type = _type;
		Joy.rectTransform.localPosition = StartAnchor;

	}
	public void Reset()
	{
        FightManager.RemoveDelaySynchronizedAction(ClearDelay);
        End ();
	}

	/// <summary>
	/// 初始模式
	/// 带有 初始 Handler 
	/// 清除子 handler
	/// 保存备份
	/// </summary>
	public void FirstMode(UnityAction pointerDown,UnityAction pointerUp,UnityAction beginDrag,UnityAction drag)
	{
		UAPointerDown = pointerDown;
		UAPointerUp = pointerUp;
		UABeginDrag = beginDrag;
		UADrag = drag;

		PointerDown.AddListener (pointerDown);
		PointerUp.AddListener (pointerUp);
		BeginDrag.AddListener (beginDrag);
		Drag.AddListener (drag);

		this.ClearSub (true,true,true,true);
	}

	/// <summary>
	/// 恢复原状态
	/// 载入备份
	/// </summary>
	public void BackToFirstMode()
	{
		PointerDown.AddListener (UAPointerDown);
		PointerUp.AddListener (UAPointerUp);
		BeginDrag.AddListener (UABeginDrag);
		Drag.AddListener (UADrag);
		this.ClearSub (true,true,true,true);
	}
	/// <summary>
	/// 投掷弹道模式
	/// 1.绑定子侦听器
	/// 2.清除部分主侦听器
	/// </summary>
	public void ThrowMode(UnityAction pointerDown,UnityAction pointerUp,UnityAction beginDrag,UnityAction drag)
	{
		SubPointerDown.AddListener(pointerDown);
		SubPointerUp.AddListener(pointerUp);
		SubBeginDrag.AddListener(null);
		SubDrag.AddListener(drag);
		this.ClearMain(true,false,true,false);
	}

	public void ClearMain(bool pointerDown = true,bool pointerUp = true,bool beginDrag = true,bool drag = true)
	{
		if (pointerDown) {PointerDown.RemoveAllListeners ();}
		if (pointerUp) {PointerUp.RemoveAllListeners ();}
		if (beginDrag) {BeginDrag.RemoveAllListeners ();}
		if (drag) {Drag.RemoveAllListeners ();}
	}

	public void ClearSub(bool pointerDown = true,bool pointerUp = true,bool beginDrag = true,bool drag = true)
	{
		if (pointerDown) {SubPointerDown.RemoveAllListeners ();}
		if (pointerUp) {SubPointerUp.RemoveAllListeners ();}
		if (beginDrag) {SubBeginDrag.RemoveAllListeners ();}
		if (drag) {SubDrag.RemoveAllListeners ();}
	}

	public void Continue(bool pointerDown = true,bool pointerUp = true,bool beginDrag = true,bool drag = true)
	{
		if (pointerDown) PointerDown.Invoke ();
		if (pointerUp) PointerUp.Invoke ();
		if (beginDrag) BeginDrag.Invoke ();
		if (drag) Drag.Invoke ();
	}
	///////////////////////////////////////////////////////////////////////////
	////////////////////////////////  临时版本 ////////////////////////////
	///////////////////////////////////////////////////////////////////////////
	public enum Mode
	{
		Attack1 = 0,
		Attack2 = 1,
		Attack3 = 2,
	}

	public List<Sprite> ICONS = new List<Sprite> ();


	/// <summary>
	/// 改变模式
	/// 1.普通模式
	/// 2.抛射模式
	/// </summary>
	/// <param name="mode">Mode.</param>
	public void ChangeMode(Mode mode)
	{
		if(ICONS.Count != 0)
		{
			int index = (int)mode;
			Stick.sprite = ICONS[index];	
		}
	}

	public void SetDelay()
	{
		Delay = true;
        FightManager.RemoveDelaySynchronizedAction(ClearDelay);
        FightManager.DelaySynchronizedAction(ClearDelay, MaxDelay);
	}

    private void ClearDelay()
    {
        Delay = false;
    }
}
