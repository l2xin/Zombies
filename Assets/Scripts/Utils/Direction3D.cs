using UnityEngine;
using System.Collections;

public class Direction3D : MonoBehaviour {


	void LateUpdate()
	{
		if(LookToTarget != null)
		{
			transform.rotation = Quaternion.Lerp (transform.rotation,LookToTarget.transform.rotation,0.5f);
		}
	}
	GameObject LookToTarget;
	public void LookTo(GameObject target)
	{
		LookToTarget = target;
	}
}
