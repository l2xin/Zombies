using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 可以拖动的页面
/// </summary>
public class GuidePage : MonoBehaviour {

	public PlayerGuide PageManager;
	public RectTransform Left;
	public RectTransform Right;
	public RectTransform Page;

	public void Online()
	{
		Left = transform.Find ("Left").gameObject.GetComponent<RectTransform> ();
		Right = transform.Find ("Right").gameObject.GetComponent<RectTransform> ();
		Page = gameObject.GetComponent<RectTransform> ();
	}
	void LateUpdate()
	{
		
	}
}

