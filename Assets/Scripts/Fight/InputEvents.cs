public class InputEvents{
	#region public class properties
	public static InputEvents Default{
		get{
			return InputEvents._Default;
		}
	}
	#endregion

	#region private class properties
	private static InputEvents _Default = new InputEvents();
	#endregion

	#region public instance properties
	public float axis {get; set;}
	public float axisRaw {get; set;}
	public bool button {get; set;}
	#endregion

	#region public constructors
	public InputEvents() : this(0f, 0f, false){}
	public InputEvents(bool button) : this(0f, 0f, button){}
	public InputEvents(float axis) : this(axis, axis, axis != 0f){}
	public InputEvents(float axis, float axisRaw) : this(axis, axisRaw, axisRaw != 0f){}
	public InputEvents(InputEvents other) : this(other.axis, other.axisRaw, other.button){}
	#endregion

	#region protected constructors
	protected InputEvents(float axis, float axisRaw, bool button){
		this.axis = axis;
		this.axisRaw = axisRaw;
		this.button = button;
	}

    public void Reset()
    {
        this.axis = 0f;
        this.axisRaw = 0f;
        this.button = false;
    }
	#endregion
}
