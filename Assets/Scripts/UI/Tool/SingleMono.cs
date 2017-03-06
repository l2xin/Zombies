////////////////////////////////////////////////////////////////////////////
// hry version 1.0 Mono 单例构造器
////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

/// <summary>
/// Mono 单例构造器
/// Mono模块化工具
/// </summary>
public abstract class SingleMono<T> : MonoBehaviour {

	private static T SINGLE;
	/// <summary>
	/// 上线接口 必须重载
	/// </summary>
	public abstract void Online ();
	/// <summary>
	/// 离线接口 必须重载
	/// </summary>
	public abstract void Offline ();
	public static T Instance
	{
		get
		{
			if(SINGLE == null)
			{
				object obj = null;
				string goname = "SINGLE - " + typeof(T).ToString ();

				//------------------------ 构建 ----------------------------
				GameObject root = new GameObject (goname);
				obj = root.AddComponent (typeof(T));

				SINGLE = (T)obj;
			}
			return SINGLE;
		}
	}
}
