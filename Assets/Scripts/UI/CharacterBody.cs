using UnityEngine;
using System.Collections;

public class CharacterBody : MonoBehaviour {

	public AnimationCurve ChangeCurve;
	public GameObject Shadow;
	bool StartChange = false;
    private Player controlsScript;

    float CurrentTime = 0;
	void Start()
	{
        controlsScript = transform.parent.GetComponent<Player>();
        PresentParts();
	}

	void LateUpdate()
	{
		if(StartChange)
		{
			CurrentTime += Time.deltaTime;
			float duration = GetDuration ();
			if (CurrentTime > duration) 
			{
				StartChange = false;
				CurrentTime = duration;
                controlsScript.UpdateHeadOffsetY();
            }
			float percent = ChangeCurve.Evaluate (CurrentTime);
			transform.localScale = CurrentScale + ChangeScale * percent;
		}

	}

	Vector3 ChangeScale = Vector3.zero;
	Vector3 CurrentScale = Vector3.one;
    Vector3 targetScale = Vector3.one;
    public void LevelUp(Vector3 targetScale)
	{
#if UNITY_EDITOR
        //Debug.Log ("<color=#ff00ff>Change to: "+targetScale+"</color>");
#endif
        StartChange = true;
		CurrentTime = 0;
		CurrentScale = transform.localScale;
		ChangeScale = targetScale - CurrentScale;
        this.targetScale = targetScale;
    }

	float GetDuration()
	{
		float Duration = ChangeCurve.keys [ChangeCurve.length - 1].time;
		return Duration;
	}

	public void Reset()
	{
		StartChange = false;
		CurrentTime = 0;
		CurrentScale = Vector3.one;
		ChangeScale = Vector3.zero;
        transform.localScale = CurrentScale;
    }

    public void Scale()
    {
        transform.localScale = targetScale;
    }

	#region -> 逐步显示 <-
	public void PresentParts()
	{
        if (Shadow != null)
        {
            Shadow.SetActive(true);
        }
    }
	#endregion
}
