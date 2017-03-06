using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public abstract class AbstractInputController : MonoBehaviour
{
	#region public instance properties
	public ReadOnlyCollection<InputReferences> inputReferences {get; protected set;}
	public InputReferences horizontalAxis {get; protected set;}
	public InputReferences verticalAxis {get; protected set;}
	public ReadOnlyCollection<InputReferences> buttons {get; protected set;}

    public string ufePlayerName = "";

    public ulong playerId;

    public virtual int player{get; set;}

	public virtual bool isReady
    {
		get{return true;}
	}

	public virtual Dictionary<InputReferences, InputEvents> inputs{
		get{
			return inputBuffer[1];
		}
		protected set{
			inputBuffer[1] = value;
		}
	}
	
	public virtual Dictionary<InputReferences, InputEvents> previousInputs{
		get{
			return inputBuffer[0];
		}
		protected set{
			inputBuffer[0] = value;
		}
	}
	#endregion

	#region protected instance fields
	protected List<Dictionary<InputReferences, InputEvents>> inputBuffer;
	protected Dictionary<InputReferences, InputEvents> currentFrameInputs = new Dictionary<InputReferences, InputEvents>();
    protected InputReferences inputReferencesOfH;
    protected InputReferences inputReferencesOfV;
    #endregion

    #region public instance methods
    public float GetAxis(InputReferences inputReference)
    {
		InputEvents currentEvent = this.GetCurrentInput(inputReference);
		if (currentEvent != null)
        {
			return currentEvent.axis;
		}

		return 0f;
	}

	public float GetAxisRaw(InputReferences inputReference)
    {
        InputEvents currentEvent = this.GetCurrentInput(inputReference);
		if (currentEvent != null)
        {
			return currentEvent.axisRaw;
		}
		
		return 0f;
	}
	
    public Vector2 GetTwoAxisRaw()
    {
        float h = GetAxisRaw(inputReferencesOfH);
        float v = GetAxisRaw(inputReferencesOfV);
        return new Vector2(h, v);
    }

    public Vector2 GetTwoAxis()
    {
        float h = GetAxis(inputReferencesOfH);
        float v = GetAxis(inputReferencesOfV);
        return new Vector2(h, v);
    }

    public float GetHorizontalAxisRaw()
    {
        return GetAxisRaw(inputReferencesOfH);
    }

    public float GetVerticalAxisRaw()
    {
        return GetAxisRaw(inputReferencesOfV);
    }

    public bool GetButton(ButtonPress engineRelatedButton){
		foreach (InputReferences button in this.buttons) {
			if (
				button != null &&
				button.engineRelatedButton == engineRelatedButton &&
				this.GetButton(button)
			){
				return true;
			}
		}
		return false;
	}
	
	public bool GetButton(InputReferences inputReference){
		InputEvents currentEvent = this.GetCurrentInput(inputReference);
		
		if (currentEvent != null){
			return currentEvent.button;
		}
		
		return false;
	}

	public bool GetButtonUp(ButtonPress engineRelatedButton){
		bool buttonUp = false;
		foreach (InputReferences button in this.buttons) {
			if (
				button != null &&
				button.engineRelatedButton == engineRelatedButton &&
				this.GetButtonUp (button)
			){
				return true;
			}
		}
		return buttonUp;
	}

	public bool GetButtonUp(InputReferences inputReference){
		InputEvents currentEvent = this.GetCurrentInput(inputReference);
		InputEvents previousEvent = this.GetPreviousInput(inputReference);
		
		if (currentEvent != null && previousEvent != null){
			return !currentEvent.button && previousEvent.button;
		}
		
		return false;
	}
	
	public bool GetButtonDown(ButtonPress engineRelatedButton){
		foreach (InputReferences button in this.buttons) {
			if (
				button != null &&
				button.engineRelatedButton == engineRelatedButton &&
				this.GetButtonDown (button)
			){	
				return true;
			}
		}
		return false;
	}
	
	public bool GetButtonDown(InputReferences inputReference)
    {
		InputEvents currentEvent = this.GetCurrentInput(inputReference);
		InputEvents previousEvent = this.GetPreviousInput(inputReference);
		
		if (currentEvent != null && previousEvent != null)
        {
			return (currentEvent.button && !previousEvent.button);
		}
		return false;
	}

    public bool GetNetButtonDown(InputReferences inputReference)
    {
        InputEvents currentEvent = this.GetCurrentInput(inputReference);
        if (currentEvent != null)
        {
            return (currentEvent.button);
        }
        return false;
    }

    public InputEvents GetCurrentInput(InputReferences inputReference){
		InputEvents currentEvent = null;
		if (inputReference != null && 
            this.inputs != null && 
            this.inputs.TryGetValue(inputReference, out currentEvent))
        {
			return currentEvent;
		}
		return null;
	}

	public InputReferences GetInputReference(ButtonPress button){
		foreach (InputReferences inputReference in this.inputReferences){
			if (inputReference != null && inputReference.engineRelatedButton == button){
				return inputReference;
			}
		}
		return null;
	}

	public InputEvents GetPreviousInput(InputReferences inputReference){
		InputEvents previousEvent = null;
		if (inputReference != null && 
            this.previousInputs != null &&
            this.previousInputs.TryGetValue(inputReference, out previousEvent)){
			return previousEvent;
		}
		return null;
	}

	public void Initialize(IEnumerable<InputReferences> inputs){
		this.Initialize(inputs, 2);
	}

	public virtual void Initialize(IEnumerable<InputReferences> inputs, int bufferSize){
		List<InputReferences> buttonList = new List<InputReferences>();
		List<InputReferences> inputList = new List<InputReferences>();

		//-------------------------------------------------
		// We need at least a buffer of 2 positions:
		// + buffer[0] -------> previous Input
		// + buffer[1] -------> current Input
		// + buffer[i > 1] ---> future Inputs 
		//-------------------------------------------------
		bufferSize = Mathf.Max(bufferSize, 2);

		this.inputBuffer = new List<Dictionary<InputReferences, InputEvents>>();
		for (int i = 0; i < bufferSize; ++i)
        {
			this.inputBuffer.Add(new Dictionary<InputReferences, InputEvents>());
		}

		if (inputs != null){
			foreach (InputReferences input in inputs){
				input.heldDown = 0;
				if (input != null){
					for (int i = 0; i < bufferSize; ++i){
						this.inputBuffer[i][input] = InputEvents.Default;
					}

					inputList.Add(input);
					if (input.inputType == InputType.HorizontalAxis){
						this.horizontalAxis = input;
					}else if (input.inputType == InputType.VerticalAxis){
						this.verticalAxis = input;
					}else{
						buttonList.Add(input);
					}
				}
			}
		}
		
		this.inputReferences = new ReadOnlyCollection<InputReferences>(inputList);
		this.buttons = new ReadOnlyCollection<InputReferences>(buttonList);
    }
    #endregion

    #region abstract methods definition
    public abstract InputEvents ReadInput(InputReferences inputReference);
	#endregion

	#region MonoBehaviour methods
	//-----------------------------------------------------------------------------------------------------------------
	// At the beginning, we were reading the user input on Update() instead of reading it on FixedUpdate()
	// and that was the desired behaviour because Unity updates the built-in input system once per frame.
	// 
	// However, we decided to use the frame-delay (which requires a fixed deltaTime) for implementing the network code,
	// so we needed to move a lot of code from Update() to FixedUpdate() in order to maintain the synchronization 
	// between both players.
	//
	// As a result, we read the player input on Update(), but we don't inform the rest of the application about 
	// the new input value until FixedUpdate().
	//-----------------------------------------------------------------------------------------------------------------
	public virtual void DoFixedUpdate(){
		if (this.inputReferences != null)
        {
			InputEvents ev;
            for (int i = 0; i < this.inputReferences.Count; i++)
            {
                InputReferences inputReference = this.inputReferences[i];
                if (this.inputs.TryGetValue(inputReference, out ev))
                {
                    this.previousInputs[inputReference] = ev;
                }
                else
                {
                    this.previousInputs[inputReference] = InputEvents.Default;
                }

                if (this.currentFrameInputs.TryGetValue(inputReference, out ev))
                {
                    this.inputs[inputReference] = ev;
                }
                else
                {
                    this.inputs[inputReference] = InputEvents.Default;
                }
            }
		}
	}

	public virtual void DoUpdate()
    {
		if (this.inputReferences != null)
        {
			//---------------------------------------------------------------------------------------------------------
			// Read the player input.
			//---------------------------------------------------------------------------------------------------------
            /*for (int i = 0; i < this.inputReferences.Count; i++)
            {
                InputReferences inputReference = this.inputReferences[i];
                InputEvents inputEvents;
                this.currentFrameInputs.TryGetValue(inputReference, out inputEvents);
                if (inputEvents != null)
                {
                    PoolUtil.DespawnerInputEvents(inputEvents);
                }
            }*/
            this.currentFrameInputs.Clear();
            for (int i = 0; i < this.inputReferences.Count; i++)
            {
                InputReferences inputReference = this.inputReferences[i];
                this.currentFrameInputs[inputReference] = this.ReadInput(inputReference);
            }
        }
	}
	#endregion
}
