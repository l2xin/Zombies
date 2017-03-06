using UnityEngine;
using System.Collections;

/// <summary>
/// 角度转向测试
/// 根据坐标点算出 Transform 的三个平面和坐标的角度关系
/// X,Y,Z 三平面（轴）
/// </summary>
public class DirectionTest : MonoBehaviour {

	public GameObject Target;
	public Vector3 NEWFORWARD = Vector3.forward;
	public Vector3 EulerAngle = Vector3.zero;
	public float Angle = 0;
	void Update () 
	{
//		transform.rotation = Quaternion.AngleAxis (359,transform.up);
		transform.LookAt(Target.transform,Vector3.up);
		EulerAngle = transform.eulerAngles;
	}

	/// <summary>
	/// 获取目标方向  Y轴需要转动的角度
	/// </summary>
	/// <returns>The X angle.</returns>
	/// <param name="_position">Position.</param>
	public float CheckYAngle(Vector3 _TargetPos)
	{
		Vector3 CurrentPos = transform.position;
		//将坐标缩进 _Plane 平面 将某方向值归零
		_TargetPos.Set (_TargetPos.x,CurrentPos.y,_TargetPos.z);
		Vector3 direction = _TargetPos + CurrentPos;
		float Dot = Vector3.Dot (transform.forward,direction.normalized);
		Vector3 Cross = Vector3.Cross (transform.forward,direction.normalized);
		int Dir = CheckDirection (Cross.y);
		float rad = Mathf.Acos (Dot);
		float AngleY = rad * Mathf.Rad2Deg;
		return AngleY * Dir;
	}
	public float CheckXAngle(Vector3 _TargetPos)
	{
		Vector3 CurrentPos = transform.position;
		//将坐标缩进 _Plane 平面 将某方向值归零
		_TargetPos.Set (CurrentPos.x,_TargetPos.y,_TargetPos.z);
		Vector3 direction = _TargetPos + CurrentPos;
		float Dot = Vector3.Dot (transform.forward,direction.normalized);
		Vector3 Cross = Vector3.Cross (transform.forward,direction.normalized);
		int Dir = CheckDirection (Cross.x);
		float rad = Mathf.Acos (Dot);
		float AngleX = rad * Mathf.Rad2Deg;
		return AngleX * Dir;
	}

	int CheckDirection(float dotValue)
	{
		if (dotValue < 0) {
			return -1;
		} else if (dotValue > 0) {
			return 1;
		} else {
			return 0;
		}
	}
}

