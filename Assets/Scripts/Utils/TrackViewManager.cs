using UnityEngine;
using System.Collections;

/// <summary>
/// TrackViewManager
/// </summary>
public class TrackViewManager : SingleMono<TrackViewManager>
{
	/// <summary>
	/// 抛物轨迹
	/// </summary>
	public ParabolaTrack parabolaTrack;
	public Direction3D direction;

	public void StartTrack3D(GameObject target)
	{
		parabolaTrack.gameObject.SetActive(true);
		parabolaTrack.transform.parent = target.transform;
		parabolaTrack.transform.localPosition = Vector3.zero;
		parabolaTrack.transform.localScale = Vector3.one;
		parabolaTrack.transform.localRotation = Quaternion.identity;

		parabolaTrack.LookTo(target);
	}

	public void RunTrack3D(float Percent)
	{
		parabolaTrack.Drag(Percent);
	}

	public void EndTrack3D()
	{
		parabolaTrack.gameObject.SetActive (false);
		parabolaTrack.transform.parent = transform;
	}

	public void StartDirection3D(GameObject target)
	{
		direction.gameObject.SetActive (true);
		direction.transform.parent = target.transform.parent;
		direction.transform.localPosition = Vector3.zero;
		direction.transform.localScale = Vector3.one;
		direction.transform.localRotation = Quaternion.identity;

		direction.LookTo (target);
	}

	public void EndDirection3D()
	{
		direction.gameObject.SetActive (false);
		direction.transform.parent = transform;
	}

	public override void Online()
	{
		if(parabolaTrack == null)
		{
			MainEntry.Instance.StartLoad ("track3d", AssetType.prefab, (GameObject go, string s) => {
				parabolaTrack = go.AddComponent<ParabolaTrack> ();
				parabolaTrack.gameObject.SetActive(false);
			});
		}
		if(direction == null)
		{
			MainEntry.Instance.StartLoad ("direction3d", AssetType.prefab, (GameObject go, string s) => {
				direction = go.AddComponent<Direction3D> ();
				direction.gameObject.SetActive(false);
			});
		}
	}

	public override void Offline()
	{
		if(parabolaTrack != null)
		{
			parabolaTrack.transform.parent = transform;
			parabolaTrack.gameObject.SetActive (false);
		}
		if(direction != null)
		{
			direction.transform.parent = transform;
			direction.gameObject.SetActive (false);	
		}
	}

	public void Add(GameObject target)
	{
		target.transform.parent = transform;
		target.transform.localPosition = Vector3.zero;
	}
}
