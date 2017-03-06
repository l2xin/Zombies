using UnityEngine;
using System.Collections;

public class DestroyScript : MonoBehaviour {
	public float destroyTime = .5f;
	
	void Start () {
		Destroy(gameObject, destroyTime);
	}
}
