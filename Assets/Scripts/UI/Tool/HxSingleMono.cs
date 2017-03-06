using UnityEngine;
using System.Collections;

/// <summary>
/// mono单例模式 Version 1.0
/// 占据SINGLE空间
/// 调用Online之后初始化完成
/// </summary>
public abstract class HxSingleMono<T> : MonoBehaviour
{
	private static T SINGLE;
		

	public static T Ins
	{
		get
		{ 
			if(SINGLE == null)
			{
				object obj = Object.FindObjectOfType (typeof(T)) as object;
				SINGLE = (T)obj;
			}
			return SINGLE;
		}
	}
}