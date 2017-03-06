using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HUDPlayerController : BaseUI
{
	public Dropdown ControlType;
	public HRYJoyStick Left;
	public HRYJoyStick Right;

	Transform LeftJoy;
	Transform RightJoy;
	Transform LeftArea;
	Transform RightArea;

	public void Awake()
	{
        isNeedCache = true;
        LeftJoy = transform.FindChild("Contain/JoyLeft");
        Left = LeftJoy.gameObject.AddComponent<HRYJoyStick>();
        Left.Online();

        RightJoy = transform.FindChild("Contain/JoyRight");
        Right = RightJoy.gameObject.AddComponent<HRYJoyStick>();
        Right.Online();

        LeftArea = transform.FindChild("Contain/LeftArea");
        ControlArea Area1 = LeftArea.gameObject.AddComponent<ControlArea>();
        Area1.STICK = Left;
        RightArea = transform.FindChild("Contain/RightArea");
        ControlArea Area2 = RightArea.gameObject.AddComponent<ControlArea>();
        Area2.STICK = Right;
    }

	public override void OnShow(object data = null)
	{
		base.OnShow (data);
		HUDManager.Instance.PLAYERCONTROLLER = this;
		BattleInputController.Instance.Initialize();
	}

	public override void OnHide()
	{
		base.OnHide();
		BattleInputController.Instance.isStartSetRightAxis = false;
		Left.Reset();
		Right.Reset();
	}

	public void Change()
	{
		HRYJoyStick.TYPE type = (HRYJoyStick.TYPE)ControlType.value;
		Left.Change(type);
		Right.Change(type);
	}
}
