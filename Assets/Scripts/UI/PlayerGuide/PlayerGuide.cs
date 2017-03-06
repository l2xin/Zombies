using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerGuide : BaseUI ,IDragHandler ,IPointerDownHandler, IPointerUpHandler {

	public enum STATE
	{
		None,
		Pick,
		Drag,
		Slide,
		Center,
	}
	private STATE state = STATE.None;
	private RectTransform mRect;
	private GuidePage [] TOTALPAGES;
	private Image CurrentIndexIcon;
	private Image[] IndexIcon;
	private List<Vector3> MOVETARGET = new List<Vector3>();
	private RectTransform Left;
	private RectTransform Right;
	private GameObject Skip1;
	private GameObject Skip2;

	private delegate void Call();
	private Call ChangeOverCall;

	private GuidePage PageInView;

	private float delta = 0;
	private Vector3 V = Vector3.zero;
	private bool Action = false;

	private void Change(STATE _state,Call _AnctionOverCall)
	{
		state = _state;
		ChangeOverCall = _AnctionOverCall;
	}

	void Awake()
	{
        isNeedCache = false;
        mRect = gameObject.GetComponent<RectTransform>();

        TOTALPAGES = new GuidePage[3];
        TOTALPAGES[0] = transform.Find("Page1").gameObject.AddComponent<GuidePage>();
        TOTALPAGES[1] = transform.Find("Page2").gameObject.AddComponent<GuidePage>();
        TOTALPAGES[2] = transform.Find("Page3").gameObject.AddComponent<GuidePage>();
        TOTALPAGES[0].Online();
        TOTALPAGES[1].Online();
        TOTALPAGES[2].Online();
        TOTALPAGES[0].PageManager = this;
        TOTALPAGES[1].PageManager = this;
        TOTALPAGES[2].PageManager = this;

        IndexIcon = new Image[3];
        IndexIcon[0] = transform.Find("Index/Index1").gameObject.GetComponent<Image>();
        IndexIcon[1] = transform.Find("Index/Index2").gameObject.GetComponent<Image>();
        IndexIcon[2] = transform.Find("Index/Index3").gameObject.GetComponent<Image>();

        CurrentIndexIcon = transform.Find("Index/Choose").gameObject.GetComponent<Image>();

        Left = transform.Find("Left").gameObject.GetComponent<RectTransform>();
        Right = transform.Find("Right").gameObject.GetComponent<RectTransform>();

        Skip1 = transform.Find("BtnSkip1").gameObject;
        Button btn1 = Skip1.GetComponent<Button>();
        btn1.onClick.AddListener(Close);

        Skip2 = transform.Find("BtnSkip2").gameObject;
        Button btn2 = Skip2.GetComponent<Button>();
        btn2.onClick.AddListener(Close);

        MoveScale = 4;
        BackScale = 4;
        SlidRange = Vector3.zero;
        HoldMaxRange = 1000f;
        PageInView = TOTALPAGES[0];
        delta = 0;
    }

	public void LateUpdate()
	{
		HoldTime += Time.deltaTime;
		CheckMove ();
		CheckView ();
	}
	#region -> 事件 <-
	Vector3 BeginPosition = Vector3.zero;
	Vector3 LastPosition = Vector3.zero;
	Vector3 OffsetRange = Vector3.zero;


	//------------------------------
	Vector3 SLIDDIRECTION = Vector3.zero;
	float SLIDDISTANCE = 0;
	float MoveScale = 1;
	float BackScale = 4;
	float CurrentScale = 1;
	float HoldTime = 0;
	Vector3 SlidRange = Vector3.zero;
	float HoldMaxRange = 300;
	//------------------------------

	public void OnPointerDown (PointerEventData eventData)
	{
		HoldTime = 0;
		SlidRange = Vector3.zero;
		SLIDDIRECTION = Vector3.zero;
		SLIDDISTANCE = 0;
		Change (STATE.Pick,null);
		RectTransformUtility.ScreenPointToWorldPointInRectangle (mRect,eventData.position,Camera.main,out BeginPosition);
//		BeginPosition = eventData.position;
		LastPosition = BeginPosition;
		Action = true;
		OffsetRange = Vector3.zero;
		CheckView ();
	}
	public void OnPointerUp (PointerEventData eventData)
	{
//		OffsetRange = Vector3.zero;
		if (HoldTime < 0.2f) 
		{
			TurnPage ();
		} 
		CheckCenter ();
	}
	void TurnPage()
	{
		if(SLIDDIRECTION.x < 0)
		{
			//向左
			CurrentIndex++;
			if(CurrentIndex >= TOTALPAGES.Length)
			{
				CurrentIndex = TOTALPAGES.Length - 1;
			}
			PageInView = TOTALPAGES [CurrentIndex];

		}
		if(SLIDDIRECTION.x > 0)
		{
			//向右
			CurrentIndex--;
			if(CurrentIndex < 0)
			{
				CurrentIndex = 0;
			}
			PageInView = TOTALPAGES [CurrentIndex];
		}
//		Debug.Log ("SLIDDIRECTION: "+SLIDDIRECTION.x+"TurnPage: "+PageInView);
	}
	public void OnDrag(PointerEventData eventData)
	{
//		Debug.Log("eventData.position: "+eventData.position);
		Change (STATE.Drag,null);
		RectTransformUtility.ScreenPointToWorldPointInRectangle (mRect,eventData.position,Camera.main,out LastPosition);
//		LastPosition = eventData.position;
//		Debug.Log("LastPosition: "+LastPosition);
		OffsetRange = LastPosition - BeginPosition;
		OffsetRange.Set (OffsetRange.x,0,0);
		this.ExecuteMove (OffsetRange);

		SlidRange += OffsetRange;
		CurrentScale = MoveScale;
		SLIDDIRECTION = SlidRange.normalized;
		SLIDDISTANCE = SlidRange.magnitude;
		BeginPosition = LastPosition;
	}
	#endregion


	void CheckMove()
	{
		if(state == STATE.Drag){return;}
		if(SLIDDISTANCE <= 0){return;}

		delta = SLIDDISTANCE  * Time.deltaTime * CurrentScale;
		SLIDDISTANCE -= delta;
		Vector3 range = Vector3.zero;
		if (SLIDDISTANCE >= 1f) 
		{
			range = delta * SLIDDIRECTION;
			for (int i = 0; i < TOTALPAGES.Length; i++) 
			{	
				TOTALPAGES [i].Page.localPosition += range;
			}
		}
		else 
		{
			delta += SLIDDISTANCE;
			range = delta * SLIDDIRECTION;
			for(int i = 0; i<TOTALPAGES.Length; i++)
			{	
				TOTALPAGES [i].Page.localPosition += range;
			}
			SLIDDISTANCE = 0;
			if(ChangeOverCall != null)
			{
				ChangeOverCall ();
				ChangeOverCall = null;
			}
		}
	}
	public void ExecuteMove(Vector3 range)
	{
		for(int i = 0; i<TOTALPAGES.Length; i++)
		{	
			TOTALPAGES [i].Page.localPosition += range;
		}
	}
	int CurrentIndex = -1;
	void CheckView()
	{
		for(int i = 0; i<TOTALPAGES.Length; i++)
		{
			GuidePage current = TOTALPAGES [i];
			if(CurrentIndex == i){continue;}
			if(current.Page.position.x > Left.position.x && current.Page.position.x < Right.position.x)
			{
				
				CurrentIndex = i;
				ChangeIndex (CurrentIndex);
				PageInView = current;
				break;
			}
		}
	}
	void CheckCenter()
	{
		Change (STATE.Center,Stop);
		CurrentScale = BackScale;
		OffsetRange = Vector3.zero - PageInView.Page.localPosition;
		OffsetRange.Set (OffsetRange.x,0,0);
		SLIDDIRECTION = OffsetRange.normalized;
		SLIDDISTANCE = OffsetRange.magnitude;
	}
	void Stop()
	{
		Change (STATE.None,null);
	}
	public void ChangeIndex(int index)
	{
		CurrentIndexIcon.transform.position = IndexIcon [index].transform.position;
		if(!Skip2.activeInHierarchy)
		{
			int FinalIndex = IndexIcon.Length - 1;
			if(index == FinalIndex)
			{
				Skip1.SetActive (false);
				Skip2.SetActive (true);
			}	
		}
	}

	public void Open()
	{
		gameObject.SetActive (true);
	}
	public void Close()
	{
		PlayerPrefs.SetInt ("FirstGame",1);
		gameObject.SetActive (false);

	}
}

