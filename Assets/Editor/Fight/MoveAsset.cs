using UnityEngine;
using UnityEditor;
using System;

public class MoveAsset
{
    [MenuItem("Assets/Create/U.F.E./Move File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<MoveInfo> ();
    }
}
