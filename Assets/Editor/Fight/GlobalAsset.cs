using UnityEngine;
using UnityEditor;
using System;

public class GlobalAsset
{
	[MenuItem("Assets/Create/U.F.E./Config File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<GlobalInfo> ();
    }
}
