using UnityEngine;
using UnityEditor;
using System;

public class CharacterAsset
{
	[MenuItem("Assets/Create/U.F.E./Character File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<CharacterInfo> ();
    }
}
