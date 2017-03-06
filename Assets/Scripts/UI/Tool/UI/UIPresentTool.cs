using UnityEngine;
using System.Collections;
using UnityEngine.UI;
[ExecuteInEditMode]
public class UIPresentTool : MonoBehaviour {

	public CanvasRenderer [] CR;
	public AnimationCurve AlphaCurve;
	void Start ()
	{
		if(CR == null)
		{
			CR = gameObject.GetComponentsInChildren<CanvasRenderer> ();
		}

	}
	bool Visible = true;
	float TargetAlpha = 1;
	float CurrentAplpha = 1;
	public void Show()
	{
		TargetAlpha = 1;
	}
	public void Hide()
	{
		TargetAlpha = 0.1f;
	}
	void LateUpdate ()
	{
		if(Input.GetKeyDown(KeyCode.Alpha9)){Show ();}
		if(Input.GetKeyDown(KeyCode.Alpha0)){Hide ();}
		if(CurrentAplpha != TargetAlpha)
		{
//			Debug.Log ("Action");
			CurrentAplpha = Mathf.Lerp (CurrentAplpha,TargetAlpha,0.5f);
			for(int i = 0;i<CR.Length;i++)
			{
				CR [i].SetAlpha (CurrentAplpha);
			}
		}
	}
}
