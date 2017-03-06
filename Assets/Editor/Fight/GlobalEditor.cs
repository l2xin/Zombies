using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GlobalInfo))]
public class GlobalEditor : Editor {
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open U.F.E Global Config")) 
			GlobalEditorWindow.Init();
		
	}
}