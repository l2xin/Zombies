using UnityEngine;
using System.Collections;

/// <summary>
/// HUD manager. 
/// 1.摇杆
/// 2.CombatInfo
/// 3.进入战斗后在加载
/// </summary>
public class HUDManager : SingleMono<HUDManager>
{
	public Camera CAMERA;
	public RectTransform RECT;
	private HUDPlayerController mPlayerController;
	public HUDPlayerController PLAYERCONTROLLER
	{
		get
		{
			return mPlayerController;	
		}
		set
		{
			mPlayerController = value;
		}
	}

	public delegate void Call();
	private Call InitOver;
	public void Ready(Call call)
	{
		CAMERA = Camera.main;
		InitOver = call;
	}

	void FixedUpdate()
	{
		if(PLAYERCONTROLLER != null)
		{
			if(InitOver != null)
			{
				InitOver ();
				InitOver = null;
			}
		}
	}

	public override void Online ()
	{	
	}

	public override void Offline()
	{
	}
}
