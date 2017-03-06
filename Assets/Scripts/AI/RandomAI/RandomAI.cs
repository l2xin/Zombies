using UnityEngine;
using System.Collections.Generic;

public class RandomAI : AbstractInputController {
	#region protected instance fields
	protected float timeLastDecision = float.NegativeInfinity;
	#endregion

	#region public override methods
	public override void Initialize (IEnumerable<InputReferences> inputs, int bufferSize){
		this.timeLastDecision = float.NegativeInfinity;
		base.Initialize (inputs, bufferSize);
	}

	public override void DoUpdate (){
	}

	public override InputEvents ReadInput (InputReferences inputReference){
		return InputEvents.Default;
	}
	#endregion
}
