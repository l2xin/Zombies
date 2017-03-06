using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
/// <summary>
/// 3D轨迹
/// </summary>
public class ParabolaTrack : MonoBehaviour
{
	public GameObject TrackGO;
	public GameObject AimGO;
	[Range(1,4)]
	public float Width = 4;
	[Range(1,10)]
	public float Height = 10;
	[Range(1,10)]
	public float MaxRange = 10;
	public float CurrentRange = 1;
	Vector3 TargetScale = Vector3.zero;
    GameObject LookToTarget;

    [Range(0,1)]
	public float DragAmount = 0;

	void Awake()
	{
		TrackGO = transform.Find ("Track").gameObject;
		AimGO = transform.Find ("Aim").gameObject;
		Width = 4;
		Height = 6;
		MaxRange = 10;
		CurrentRange = 10;
		DragAmount = 0.1f;
	}

	void LateUpdate()
	{
		if(LookToTarget != null)
		{
			transform.rotation = Quaternion.Lerp (transform.rotation,LookToTarget.transform.rotation,0.5f);
		}
		TrackGO.transform.localScale = Vector3.Lerp (TrackGO.transform.localScale, TargetScale, 0.5f);
		AimGO.transform.localPosition = Vector3.forward * CurrentRange;
	}
	/// <summary>
	/// 拖拽力度 0~1;
	/// </summary>
	/// <param name="percent">Percent.</param>
	public void Drag(float percent)
	{
		CurrentRange = MaxRange * percent;
		if(CurrentRange < 1){CurrentRange = 1;}
		if(Width < 1){Width = 1;}
		if(Height < 1){Height = 1;}
		TargetScale.Set (Width,Height,CurrentRange);
	}

	public void LookTo(GameObject target)
	{
		LookToTarget = target;
	}
}
