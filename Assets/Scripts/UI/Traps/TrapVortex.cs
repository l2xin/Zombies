using UnityEngine;
using System.Collections;

public class TrapVortex : MonoBehaviour {

	void Start ()
	{
		
	}

	void LateUpdate () 
	{
//		if(Speed != 0)
//		{
//			Vector3 delta = DIRECTION * Speed * Time.deltaTime;
//			TARGET.transform.position += delta;
//			DIRECTION = Vector3.zero;
//			Speed = 0;
//		}
	}
	GameObject TARGET;
	float Speed = 0;
	Vector3 DIRECTION = Vector3.zero;
	public void Pull(GameObject target,Vector3 _from,Vector3 _to)
	{
//		DIRECTION = _to - _from;
//		DIRECTION.Normalize ();
//		TARGET = target;
//		Speed = 100 * UFE.MAP_SCALE;
	}

	public void Remove()
	{
//		PoolUtil.Despawner(gameObject);
	}
}
