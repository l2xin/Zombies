using UnityEngine;
using UnityEditor;

public static class PlayerPrefsEditor{
	[MenuItem("Window/U.F.E./Clear PlayerPrefs")]
	public static void Clear(){
		PlayerPrefs.DeleteAll();
	}
}