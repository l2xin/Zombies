using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class tries to read the player input using cInput:
/// https://www.assetstore.unity3d.com/#/content/3129
/// 
/// If cInput is not available, it will use the Unity Input instead.
/// </summary>
///--------------------------------------------------------------------------------------------------------------------
public class InputController : AbstractInputController
{
	#region public instance properties
	//-----------------------------------------------------------------------------
	// TODO: This value should be read from cInput
	protected string None = "None";
	//-----------------------------------------------------------------------------
	#endregion
	
	#region protected instance properties
	protected Func<string, float>	getAxis			= null;
	protected Func<string, float>	getAxisRaw		= null;
	protected Func<string, bool>	getButton		= null;
	protected bool					inputManager	= false;
	#endregion
	
	#region public overriden methods 
	public override void Initialize(IEnumerable<InputReferences> inputs, int bufferSize)
    {
		base.Initialize (inputs, bufferSize);
		this.SelectInputType();
	}
	
	public override InputEvents ReadInput(InputReferences inputReference)
    {
        return InputEvents.Default;
    }
	#endregion
	
	#region protected instance methods
	protected virtual void SelectInputType(){
	}
	
	protected virtual void InitializeInput(){
		// Otherwise, use the built-in Unity Input
		if (this.getAxis == null){
			this.getAxis = Input.GetAxis;
		}
		
		if (this.getAxisRaw == null){
			this.getAxisRaw = Input.GetAxisRaw;
		}
		
		if (this.getButton == null){
			this.getButton = Input.GetButton;
		}
		
		this.inputManager = true;
	}
	
	protected virtual void InitializeCInput(){
	}
	#endregion
}