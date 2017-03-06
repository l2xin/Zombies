using System;
using System.Reflection;
using UnityEngine;

///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class tries to read the player input using Control-Freak's TouchController:
/// https://www.assetstore.unity3d.com/#/content/11562
/// 
/// If Control-Freak's TouchController is not available, it will use cInput or the Unity Input instead.
/// </summary>
///--------------------------------------------------------------------------------------------------------------------
public class InputTouchController : InputController
{
	#region public instance properties
	public float deadZone = 0f;
	#endregion

	#region public instance properties
	protected bool useControlFreak = false;
	#endregion

	#region public overriden methods
	public override InputEvents ReadInput (InputReferences inputReference)
    {
		InputEvents ev = base.ReadInput(inputReference);

		if (this.useControlFreak && inputReference.inputType != InputType.Button && Mathf.Abs(ev.axis) < this.deadZone)
        {
			return new InputEvents(0f);
		}

		return ev;
	}
	#endregion

	#region protected overriden methods
	protected override void SelectInputType ()
    {
		Type type = FightManager.SearchClass("TouchController");
		UnityEngine.Object touchController = null;

		if (type != null)
        {
			touchController = GameObject.FindObjectOfType(type);
		}

		if (touchController != null){
			this.InitializeControlFreakTouchController(touchController);
		}
        else
        {
			base.SelectInputType();
		}
	}
	#endregion

	#region protected instance methods
	protected virtual void InitializeControlFreakTouchController(UnityEngine.Object touchController){
	}
	#endregion
}